namespace Gymunity.APIs.Responses.Errors
{
    /// <summary>
    /// Represents an API response for validation errors, typically returned when request data fails validation.
    /// </summary>
    /// <remarks>
    /// This response includes a collection of validation error messages and is automatically returned with a 400 Bad Request status.
    /// </remarks>
    public class ApiValidationErrorResponse() : ApiResponse(400)
    {
        /// <summary>
        /// Gets or sets the collection of validation error messages.
        /// </summary>
        /// <value>
        /// A list of strings describing the validation errors that occurred.
        /// </value>
        public IEnumerable<string> Errors { get; set; } = [];
    }
}