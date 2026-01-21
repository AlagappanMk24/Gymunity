namespace Gymunity.APIs.Responses.Errors
{
    /// <summary>
    /// Represents an API response for exceptions, including detailed error information for debugging purposes.
    /// </summary>
    /// <remarks>
    /// This response is typically used in development environments to provide detailed exception information.
    /// In production, consider using a more generic error response.
    /// </remarks>
    public class ApiExceptionResponse(int statusCode, string message, string description) : ApiResponse(statusCode, message)
    {
        /// <summary>
        /// Gets the detailed description of the exception.
        /// </summary>
        /// <value>
        /// A string containing detailed exception information, typically including stack trace and inner exception details.
        /// </value>
        public string Description { get; } = description;
    }
}