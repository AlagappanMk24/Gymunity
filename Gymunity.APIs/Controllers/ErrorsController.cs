using Gymunity.APIs.Responses;
using Gymunity.APIs.Responses.Errors;
using Microsoft.AspNetCore.Mvc;

namespace Gymunity.APIs.Controllers
{
    /// <summary>
    /// Controller responsible for handling and consistent formatting of HTTP error responses.
    /// This controller is typically called via middleware re-execution when a status code is triggered.
    /// </summary>
    [Route("/errors/{code}")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ErrorsController : ControllerBase
    {
        /// <summary>
        /// Generates a standardized <see cref="ApiResponse"/> based on the provided HTTP status code.
        /// </summary>
        /// <param name="code">The HTTP status code to be processed.</param>
        /// <returns>An <see cref="ActionResult"/> containing the formatted error response.</returns>
        public ActionResult Error(int code)
        {
            return code switch
            {
                400 => BadRequest(new ApiResponse(400)),
                401 => Unauthorized(new ApiResponse(401)),
                403 => new ObjectResult(new ApiResponse(403)) { StatusCode = 403 },
                404 => NotFound(new ApiResponse(404)),
                500 => new ObjectResult(new ApiExceptionResponse(500, "Internal Server Error", "An error occurred. Please try again later.")) { StatusCode = 500 },
                _ => new ObjectResult(new ApiResponse(code)) { StatusCode = code }
            };
        }
    }
}