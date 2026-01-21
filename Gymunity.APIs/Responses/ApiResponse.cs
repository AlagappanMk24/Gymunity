namespace Gymunity.APIs.Responses
{
    /// <summary>
    /// Represents a standard response returned by an API, including an HTTP status code and an optional message
    /// describing the result.
    /// </summary>
    public class ApiResponse
    {
        /// <summary>
        /// Gets or sets the HTTP status code of the response.
        /// </summary>
        /// <value>
        /// An integer representing the HTTP status code (e.g., 200 for success, 400 for bad request, 500 for server error).
        /// </value>
        public int StatusCode { get; set; }

        /// <summary>
        /// Gets or sets the message describing the response.
        /// </summary>
        /// <value>
        /// A string containing a human-readable message about the operation result. If null, a default message based on the status code is used.
        /// </value>
        public string? Message { get; set; }

        /// <summary>
        /// Initializes a new instance of the ApiResponse class with the specified status code and an optional message.
        /// </summary>
        /// <param name="statusCode">The HTTP status code to associate with the response.</param>
        /// <param name="message">An optional message describing the response. If null, a default message based on the status code is used.</param>
        public ApiResponse(int statusCode, string? message = null)
        {
            StatusCode = statusCode;
            Message = message ?? GetDefaultMessageForStatusCode(statusCode);
        }
        private static string GetDefaultMessageForStatusCode(int statusCode)
        {
            return statusCode switch
            {
                400 => "A bad request, you have made",
                401 => "Authorized, you are not",
                404 => "Resource was not found",
                500 => "Internal server error occurred",
                _ => "An error occurred"
            };
        }
    }
    /// <summary>
    /// Represents a standard response returned by an API operation, including the result data and a success indicator.
    /// </summary>
    /// <typeparam name="T">The type of the data returned by the API operation.</typeparam>
    /// <param name="data">The data associated with the API response. This value is typically the result of the operation.</param>
    /// <param name="success">A value indicating whether the API operation was successful. The default is <see langword="true"/>.</param>
    public class ApiResponse<T>(T data, bool success = true)
    {
        /// <summary>
        /// Gets a value indicating whether the API operation was successful.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the operation completed successfully; otherwise, <see langword="false"/>.
        /// </value>
        public bool Success { get; } = success;

        /// <summary>
        /// Gets the data associated with the current instance.
        /// </summary>
        /// <value>
        /// The data returned by the API operation, of type <typeparamref name="T"/>.
        /// </value>
        public T Data { get; } = data;
    }
}