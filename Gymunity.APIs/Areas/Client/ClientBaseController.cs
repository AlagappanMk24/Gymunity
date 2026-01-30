using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gymunity.APIs.Areas.Client
{
    /// <summary>
    /// Base controller for Client-facing API endpoints.
    /// Provides shared routing and "Client" role authorization.
    /// </summary>
    [ApiController]
    [Authorize(Roles = "Client")]
    [Route("api/v1/clients/[controller]")]
    public class ClientBaseController : ControllerBase
    {
       
    }
}