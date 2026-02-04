using Gymunity.Application.Contracts.Services.Communication;
using Gymunity.Domain;
using Gymunity.Domain.Entities.Trainer;
using Gymunity.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace Gymunity.Application.Services.Communication
{
    /// <summary>
    /// Service responsible for handling AI-powered communication and automated package inquiries.
    /// Integrates with OpenRouter/DeepSeek and provides fallback logic when the external AI is unavailable.
    /// </summary>
    /// <remarks>
    /// This service serves as the primary interface for chatbot functionality, providing both
    /// package-specific queries and general AI interactions with robust error handling and fallback mechanisms.
    /// </remarks>
    public class ChatBotService : IChatBotService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiKey;
        private readonly ILogger<ChatBotService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatBotService"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unit of work for database operations.</param>
        /// <param name="httpClientFactory">Factory for creating HTTP clients.</param>
        /// <param name="configuration">Application configuration for retrieving API keys.</param>
        /// <param name="logger">Logger for tracking service operations.</param>
        /// <exception cref="InvalidOperationException">Thrown when no API key is configured.</exception>
        public ChatBotService(
            IUnitOfWork unitOfWork,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<ChatBotService> logger)
        {
            _unitOfWork = unitOfWork;
            _httpClientFactory = httpClientFactory;
            _apiKey = configuration["OpenRouter:ApiKey"] ?? configuration["HuggingFace:ApiKey"]
                      ?? throw new InvalidOperationException("API key is not configured");
            _logger = logger;

            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                _logger.LogWarning("No API key configured for ChatBotService (OpenRouter/HuggingFace)");
            }
        }

        /// <summary>
        /// Handles user questions regarding training packages by providing current database data to the AI.
        /// </summary>
        /// <param name="question">The user's natural language question about packages.</param>
        /// <returns>
        /// An AI-generated response based on current package data, or a local fallback response if the API fails.
        /// </returns>
        /// <remarks>
        /// This method performs the following steps:
        /// 1. Fetches active packages with their associated programs from the database.
        /// 2. Formats the package data for AI consumption.
        /// 3. Constructs a detailed prompt including the package context.
        /// 4. Calls the AI service with the enhanced prompt.
        /// 5. Provides fallback responses if any step fails.
        /// </remarks>
        public async Task<string> AskAboutPackagesAsync(string question)
        {
            try
            {
                // Step 1: Fetch live package data from database to provide context to the AI
                IReadOnlyList<Package> packages = [];
                try
                {
                    var packageRepo = _unitOfWork.Repository<Package, IPackageRepository>();
                    packages = await packageRepo.GetAllActiveWithProgramsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to load packages in AskAboutPackagesAsync");
                    // Continue with empty package list - the AI will handle missing data gracefully
                }

                // Step 2: Build the AI system prompt with current package information
                var packagesContext = FormatPackagesForAI(packages);

                var prompt = 
                    $@"You are an intelligent assistant specialized in answering questions about available training packages on the Gymunity platform.

                    Available Package Information:
                    {packagesContext}

                    User Question: {question}

                    Please provide a clear and helpful response in English. If the question is about pricing, detail the costs. If it's about trainers, mention their names. 
                    If it's about annual plans, list packages with yearly pricing.";

                // Step 3: Call AI with specific model optimized for package queries
                var model = "tngtech/deepseek-r1t2-chimera:free";
                return await CallAIAsync(prompt, modelName: model, requireJsonOnly: false, packages: packages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AskAboutPackagesAsync");
                return "Sorry, an error occurred while processing your question.";
            }
        }

        /// <summary>
        /// Sends a custom prompt to the AI service and returns the response.
        /// </summary>
        /// <param name="prompt">The custom prompt to send to the AI.</param>
        /// <param name="modelName">Optional specific model ID. Defaults to DeepSeek model if not provided.</param>
        /// <param name="requireJsonOnly">If true, forces the AI to return only valid JSON without any additional text.</param>
        /// <returns>The AI's response as a string, or an error message if the call fails.</returns>
        /// <remarks>
        /// This method pre-loads package data before making the API call to ensure
        /// fallback responses are available if the external service fails.
        /// </remarks>
        public async Task<string> AskAsync(string prompt, string? modelName = null, bool requireJsonOnly = false)
        {
            // Load all database data BEFORE any external API calls to ensure fallback availability
            IReadOnlyList<Package> packages = [];
            try
            {
                var packageRepo = _unitOfWork.Repository<Package, IPackageRepository>();
                packages = await packageRepo.GetAllActiveWithProgramsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load packages in AskAsync");
                // Continue with empty list - avoid reusing DbContext after disposal
            }

            try
            {
                return await CallAIAsync(prompt, modelName, requireJsonOnly, packages: packages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling OpenRouter API in AskAsync");
                return "Sorry, an error occurred while processing your question.";
            }
        }

        /// <summary>
        /// Makes an HTTP request to the OpenRouter API and processes the response.
        /// </summary>
        /// <param name="prompt">The user prompt to send to the AI.</param>
        /// <param name="modelName">The AI model to use. Defaults to DeepSeek if not specified.</param>
        /// <param name="requireJsonOnly">If true, adds a system message enforcing JSON-only responses.</param>
        /// <param name="packages">Optional package data for fallback response generation.</param>
        /// <returns>
        /// The AI-generated response, or a fallback response if the API call fails and package data is available.
        /// </returns>
        /// <exception cref="HttpRequestException">Thrown when the API request fails.</exception>
        /// <remarks>
        /// This method handles:
        /// 1. Request construction with proper authentication.
        /// 2. Timeout configuration (5 minutes).
        /// 3. Response parsing with multiple fallback strategies.
        /// 4. Automatic fallback to local response generation when the API fails.
        /// </remarks>
        private async Task<string> CallAIAsync(string prompt, string? modelName = null, bool requireJsonOnly = false, IReadOnlyList<Package>? packages = null)
        {
            string? systemPrefix = null;
            if (requireJsonOnly)
            {
                systemPrefix = "You are an API that MUST return only valid JSON and nothing else. No explanations, no markdown, no extra text. Return {} if you cannot comply.";
            }

            // Construct message array with optional system prefix
            var messages = systemPrefix != null
                ? [new { role = "system", content = systemPrefix }, new { role = "user", content = prompt }]
                : new object[] { new { role = "user", content = prompt } };

            // Configure HTTP client with authentication and timeout
            var httpClient = _httpClientFactory.CreateClient();

            httpClient.Timeout = TimeSpan.FromMinutes(5);
            httpClient.BaseAddress = new Uri("https://openrouter.ai/api/v1/");
            httpClient.DefaultRequestHeaders.Remove("Authorization");
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

            // Use specified model or default to DeepSeek
            var effectiveModel = modelName ?? "tngtech/deepseek-r1t2-chimera:free";

            // Construct request payload
            var payload = new
            {
                model = effectiveModel,
                messages,
                stream = false
            };

            var requestJson = JsonSerializer.Serialize(payload);
            using var request = new HttpRequestMessage(HttpMethod.Post, "chat/completions")
            {
                Content = new StringContent(requestJson, Encoding.UTF8, "application/json")
            };

            // Execute API request
            using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("OpenRouter API error: {Status} - {Content}", response.StatusCode, content);

                // Generate fallback response if package data is available
                return (packages != null && packages.Count > 0)
                           ? GenerateFallbackResponse(prompt, packages)
                           : "Sorry, there was an error processing your request.";
            }

            try
            {
                // Parse JSON response with multiple extraction strategies
                using var doc = JsonDocument.Parse(content);
                var root = doc.RootElement;

                // Strategy 1: Extract from choices[0].message.content (OpenRouter format)
                if (root.TryGetProperty("choices", out var choices) && choices.ValueKind == JsonValueKind.Array && choices.GetArrayLength() > 0)
                {
                    var first = choices[0];
                    if (first.TryGetProperty("message", out var message) && message.TryGetProperty("content", out var msgContent))
                    {
                        var text = msgContent.GetString();
                        if (!string.IsNullOrWhiteSpace(text)) return text.Trim();
                    }
                    // Strategy 2: Extract from choices[0].text (alternative format)
                    if (first.TryGetProperty("text", out var textProp))
                    {
                        var text = textProp.GetString();
                        if (!string.IsNullOrWhiteSpace(text)) return text.Trim();
                    }
                }

                // Strategy 3: Extract from root.message.content (fallback format)
                if (root.TryGetProperty("message", out var messageObj) && messageObj.TryGetProperty("content", out var contentProp))
                {
                    var text = contentProp.GetString();
                    if (!string.IsNullOrWhiteSpace(text)) return text.Trim();
                }
            }
            catch (JsonException)
            {
                _logger.LogWarning("Could not parse OpenRouter response as JSON, returning raw content");
                // Return raw content if JSON parsing fails
            }

            return content;
        }

        /// <summary>
        /// Formats package data into a structured string suitable for AI prompts.
        /// </summary>
        /// <param name="packages">The list of packages to format.</param>
        /// <returns>A formatted string containing package details and summary statistics.</returns>
        /// <remarks>
        /// The output includes:
        /// - Individual package details (name, description, pricing, trainer info)
        /// - Summary statistics (price ranges, total count)
        /// - Trainer information with user details when available
        /// </remarks>
        private static string FormatPackagesForAI(IReadOnlyList<Package> packages)
        {
            if (packages == null || packages.Count == 0)
                return "No packages currently available.";

            var sb = new StringBuilder();

            // Format individual package details
            foreach (var package in packages)
            {
                var trainer = package.Trainer;
                var user = trainer?.User;
                var trainerName = user?.FullName ?? trainer?.Handle ?? "Not Specified";

                sb.AppendLine($"- Package: {package.Name}");
                sb.AppendLine($"  Description: {package.Description}");
                sb.AppendLine($"  Monthly Price: {package.PriceMonthly} {package.Currency}");
                sb.AppendLine($"  Yearly Price: {(package.PriceYearly.HasValue ? package.PriceYearly.Value.ToString() : "Not Available")} {package.Currency}");
                sb.AppendLine($"  Trainer:");
                sb.AppendLine($"    - Name: {trainerName}");

                // Include additional trainer/user details if available
                if (!string.IsNullOrWhiteSpace(user?.UserName)) 
                    sb.AppendLine($"    - Username: {user!.UserName}");
                if (!string.IsNullOrWhiteSpace(user?.Email)) 
                    sb.AppendLine($"    - Email: {user!.Email}");
                if (trainer != null) 
                    sb.AppendLine($"    - Experience: {trainer.YearsExperience} years");

                sb.AppendLine();
            }

            // Calculate and append summary statistics
            var monthlyPrices = packages.Select(p => p.PriceMonthly).ToList();

            var yearlyPrices = packages.Where(p => p.PriceYearly.HasValue)
                                    .Select(p => p.PriceYearly!.Value)
                                    .ToList();

            sb.AppendLine("Summary:");
            sb.AppendLine($"- Total Packages: {packages.Count}");

            if (monthlyPrices.Count != 0)
            {
                sb.AppendLine($"- Monthly price starts from: {monthlyPrices.Min()} {packages.First().Currency}");
                sb.AppendLine($"- Monthly price goes up to: {monthlyPrices.Max()} {packages.First().Currency}");
            }

            if (yearlyPrices.Count != 0)
            {
                sb.AppendLine($"- Yearly price starts from: {yearlyPrices.Min()} {packages.First().Currency}");
                sb.AppendLine($"- Yearly price goes up to: {yearlyPrices.Max()} {packages.First().Currency}");
            }
            return sb.ToString();
        }

        /// <summary>
        /// Generates a local fallback response when the AI service is unavailable.
        /// </summary>
        /// <param name="question">The user's original question.</param>
        /// <param name="packages">Available package data for generating responses.</param>
        /// <returns>A locally generated response based on the question intent and available data.</returns>
        /// <remarks>
        /// This method analyzes the question for keywords and provides appropriate responses:
        /// - Annual/yearly questions: Lists packages with yearly pricing
        /// - Pricing questions: Shows price ranges
        /// - Trainer questions: Lists trainers and their packages
        /// - Default: Lists all available packages
        /// </remarks>
        private static string GenerateFallbackResponse(string question, IReadOnlyList<Package> packages)
        {
            if (packages == null || packages.Count == 0)
                return "Sorry, there are no packages available at the moment.";

            var questionLower = question.ToLower();

            // Handle annual package inquiries
            if (questionLower.Contains("annual") || questionLower.Contains("yearly") || questionLower.Contains("year"))
            {
                var annualPackages = packages.Where(p => p.PriceYearly.HasValue).ToList();
                if (annualPackages.Count != 0)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("Available Annual Packages:");
                    foreach (var pkg in annualPackages)
                    {
                        var trainerName = pkg.Trainer?.User?.FullName ?? "Not specified";
                        sb.AppendLine($"- {pkg.Name} (Trainer: {trainerName}) - Annual Price: {pkg.PriceYearly} {pkg.Currency}");
                    }
                    return sb.ToString();
                }
                return "There are no annual packages available currently.";
            }

            // Handle pricing inquiries
            if (questionLower.Contains("price") || questionLower.Contains("cost") || questionLower.Contains("how much"))
            {
                var monthlyPrices = packages.Select(p => p.PriceMonthly).ToList();
                var yearlyPrices = packages.Where(p => p.PriceYearly.HasValue).Select(p => p.PriceYearly!.Value).ToList();
                var sb = new StringBuilder();
                sb.AppendLine("Package Pricing:");
                sb.AppendLine($"- Monthly rates range from {monthlyPrices.Min()} to {monthlyPrices.Max()} {packages.First().Currency}");

                if (yearlyPrices.Any())
                    sb.AppendLine($"- Yearly rates range from {yearlyPrices.Min()} to {yearlyPrices.Max()} {packages.First().Currency}");

                return sb.ToString();
            }

            // Handle trainer inquiries
            if (questionLower.Contains("trainer") || questionLower.Contains("coach") || questionLower.Contains("name"))
            {
                var sb = new StringBuilder();
                sb.AppendLine("List of Packages and Trainers:");
                foreach (var pkg in packages)
                {
                    var trainer = pkg.Trainer;
                    var user = trainer?.User;
                    var trainerName = user?.FullName ?? trainer?.Handle ?? "Unknown";
                    sb.AppendLine($"- {pkg.Name} (Trainer: {trainerName})");
                    if (!string.IsNullOrWhiteSpace(user?.Email)) sb.AppendLine($"  Email: {user!.Email}");
                    if (!string.IsNullOrWhiteSpace(trainer?.Handle)) sb.AppendLine($"  Handle: {trainer.Handle}");
                }
                return sb.ToString();
            }

            // Default response - list all packages
            var defaultSb = new StringBuilder();
            defaultSb.AppendLine($"We have {packages.Count} packages available:");
            foreach (var pkg in packages)
            {
                var trainerName = pkg.Trainer?.User?.FullName ?? pkg.Trainer?.Handle ?? "Unknown";
                defaultSb.AppendLine($"- {pkg.Name} (Trainer: {trainerName}) - Monthly Price: {pkg.PriceMonthly} {pkg.Currency}");
            }
            return defaultSb.ToString();
        }
    }
}