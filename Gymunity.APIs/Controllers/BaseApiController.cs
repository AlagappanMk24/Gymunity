using Microsoft.AspNetCore.Mvc;

namespace Gymunity.APIs.Controllers
{
    /// <summary>
    /// Base controller for all API controllers in the application.
    /// </summary>
    /// <remarks>
    /// Provides common functionality and routing configuration for all derived API controllers.
    /// Controllers inheriting from this class will automatically use the "api/[controller]" route.
    /// </remarks>
    [Route("api/[controller]")]
    [ApiController]
    public class BaseApiController :ControllerBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseApiController"/> class.
        /// </summary>
        public BaseApiController()
        {
        }
    }
}