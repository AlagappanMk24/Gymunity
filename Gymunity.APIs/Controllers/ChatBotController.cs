using Gymunity.APIs.Responses;
using Gymunity.Application.Contracts.Services.Communication;
using Microsoft.AspNetCore.Mvc;

namespace Gymunity.APIs.Controllers
{
    /// <summary>
    /// Controller for chatbot that answers questions about packages
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ChatBotController(IChatBotService chatBotService, ILogger<ChatBotController> logger) : BaseApiController
    {
        private readonly IChatBotService _chatBotService = chatBotService;
        private readonly ILogger<ChatBotController> _logger = logger;

        /// <summary>
        /// Ask a question about packages
        /// </summary>
        /// <param name="request">The question to ask</param>
        /// <returns>Answer from the chatbot</returns>
        // ai from amr start: AI endpoint starts here
        [HttpPost("ask")]
        public async Task<ActionResult<ApiResponse<ChatBotResponse>>> AskQuestion([FromBody] ChatBotRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Question))
                {
                    return BadRequest(new ApiResponse(400, "Question is required."));
                }

                var answer = await _chatBotService.AskAboutPackagesAsync(request.Question);

                return Ok(new ApiResponse<ChatBotResponse>(new ChatBotResponse
                {
                    Answer = answer,
                    Question = request.Question
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing chatbot question");
                return StatusCode(500, new ApiResponse(500, "An error occurred while processing your question."));
            }
        }

        /// <summary>
        /// Request model for chatbot
        /// </summary>
        public class ChatBotRequest
        {
            public string Question { get; set; } = string.Empty;
        }

        /// <summary>
        /// Response model for chatbot
        /// </summary>
        public class ChatBotResponse
        {
            public string Question { get; set; } = string.Empty;
            public string Answer { get; set; } = string.Empty;
        }
    }
}

