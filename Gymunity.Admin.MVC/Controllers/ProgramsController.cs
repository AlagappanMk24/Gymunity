using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.Rendering;
using Gymunity.Application.Contracts.Services.Admin;
using Gymunity.Domain.Enums;
using Gymunity.Application.Specifications.Admin;
using Gymunity.Admin.MVC.ViewModels.Programs;
using Gymunity.Application.DTOs.Program;

namespace Gymunity.Admin.MVC.Controllers
{
    /// <summary>
    /// Admin Programs Controller
    /// Manages programs, visibility, and program lifecycle
    /// </summary>
    public class ProgramsController(ILogger<ProgramsController> logger, IProgramAdminService programService) : BaseAdminController
    {
        private readonly ILogger<ProgramsController> _logger = logger;
        private readonly IProgramAdminService _programService = programService;

        /// <summary>
        /// Displays list of all programs with filtering and pagination
        /// </summary>
        [HttpGet("programs")]
        public async Task<IActionResult> Index(
            int pageNumber = 1,
            int pageSize = 10,
            bool? isPublic = null,
            string? programType = null,
            string? searchTerm = null)
        {
            try
            {
                SetPageTitle("Manage Programs");
                SetBreadcrumbs("Dashboard", "Programs");

                ProgramType? typeFilter = null;
                if (!string.IsNullOrWhiteSpace(programType) &&
                    Enum.TryParse<ProgramType>(programType, out var parsedType))
                {
                    typeFilter = parsedType;
                }

                var specs = new ProgramFilterSpecs(
                    isPublic: isPublic,
                    programType: typeFilter,
                    pageNumber: pageNumber,
                    pageSize: pageSize,
                    searchTerm: searchTerm);

                var programs = await _programService.GetAllProgramsAsync(specs);
                var totalCount = await _programService.GetProgramCountAsync(specs);

                var model = new ProgramsListViewModel
                {
                    Programs = programs.ToList(),
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    SearchTerm = searchTerm,
                    IsPublicFilter = isPublic,
                    ProgramTypeFilter = programType
                };

                _logger.LogInformation("Programs list accessed by user: {User}", User.Identity?.Name);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading programs list");
                ShowErrorMessage("An error occurred while loading programs");
                return RedirectToDashboard();
            }
        }

        /// <summary>
        /// AJAX endpoint for filtering programs without page reload
        /// </summary>
        [HttpGet("programs/filter")]
        public async Task<IActionResult> FilterPrograms(
            int pageNumber = 1,
            int pageSize = 10,
            bool? isPublic = null,
            string? programType = null,
            string? searchTerm = null)
        {
            try
            {
                ProgramType? typeFilter = null;
                if (!string.IsNullOrWhiteSpace(programType) &&
                    Enum.TryParse<ProgramType>(programType, out var parsedType))
                {
                    typeFilter = parsedType;
                }

                var specs = new ProgramFilterSpecs(
                    isPublic: isPublic,
                    programType: typeFilter,
                    pageNumber: pageNumber,
                    pageSize: pageSize,
                    searchTerm: searchTerm);

                var programs = await _programService.GetAllProgramsAsync(specs);
                var totalCount = await _programService.GetProgramCountAsync(specs);

                var model = new ProgramsListViewModel
                {
                    Programs = programs.ToList(),
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    SearchTerm = searchTerm,
                    IsPublicFilter = isPublic,
                    ProgramTypeFilter = programType
                };

                return Json(new
                {
                    success = true,
                    data = model,
                    html = await RenderPartialViewToStringAsync("_ProgramsTablePartial", model)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error filtering programs");
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Displays detailed view of a specific program
        /// </summary>
        [HttpGet("programs/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                SetPageTitle("Program Details");
                SetBreadcrumbs("Dashboard", "Programs", "Details");

                // Load program data with trainer information using specification pattern
                var program = await _programService.GetProgramDetailsWithTrainerAsync(id);
                if (program == null)
                {
                    ShowErrorMessage("Program not found");
                    return RedirectToAction(nameof(Index));
                }

                var vm = new ProgramDetailsViewModel
                {
                    Id = program.Id,
                    Title = program.Title,
                    Description = program.Description,
                    Type = program.Type,
                    DurationWeeks = program.DurationWeeks,
                    Price = program.Price,
                    IsPublic = program.IsPublic,
                    MaxClients = program.MaxClients,
                    ThumbnailUrl = program.ThumbnailUrl,
                    TrainerProfileId = program.TrainerProfileId,
                    TrainerUserName = program.TrainerUserName,
                    TrainerHandle = program.TrainerHandle,
                    TrainerEmail = program.TrainerEmail,
                    CreatedAt = program.CreatedAt,
                    UpdatedAt = program.UpdatedAt,
                    TotalWeeks = program.DurationWeeks,
                    TotalExercises = program.DurationWeeks > 0 ? program.DurationWeeks * 6 : 0 // Approximate
                };

                _logger.LogInformation("Program {ProgramId} details viewed by {User}", id, User.Identity?.Name);
                return View(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading program details for ID {ProgramId}", id);
                ShowErrorMessage("An error occurred while loading program details");
                return RedirectToDashboard();
            }
        }

        /// <summary>
        /// Displays public programs
        /// </summary>
        [HttpGet("programs/public")]
        public async Task<IActionResult> Public(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                SetPageTitle("Public Programs");
                SetBreadcrumbs("Dashboard", "Programs", "Public");

                var programs = await _programService.GetPublicProgramsAsync(pageNumber, pageSize);
                var totalCount = await _programService.GetPublicProgramCountAsync();

                var model = new ProgramsListViewModel
                {
                    Programs = programs.ToList(),
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    IsPublicFilter = true
                };

                _logger.LogInformation("Public programs list accessed by {User}", User.Identity?.Name);
                return View("Index", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading public programs");
                ShowErrorMessage("An error occurred while loading public programs");
                return RedirectToDashboard();
            }
        }

        /// <summary>
        /// Displays private programs
        /// </summary>
        [HttpGet("programs/private")]
        public async Task<IActionResult> Private(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                SetPageTitle("Private Programs");
                SetBreadcrumbs("Dashboard", "Programs", "Private");

                var programs = await _programService.GetPrivateProgramsAsync(pageNumber, pageSize);
                var totalCount = await _programService.GetPrivateProgramCountAsync();

                var model = new ProgramsListViewModel
                {
                    Programs = programs.ToList(),
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    IsPublicFilter = false
                };

                _logger.LogInformation("Private programs list accessed by {User}", User.Identity?.Name);
                return View("Index", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading private programs");
                ShowErrorMessage("An error occurred while loading private programs");
                return RedirectToDashboard();
            }
        }

        /// <summary>
        /// Searches programs by title or description
        /// </summary>
        [HttpGet("programs/search")]
        public async Task<IActionResult> Search(
            string? q,
            int pageNumber = 1,
            int pageSize = 10)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(q))
                    return RedirectToAction(nameof(Index));

                var programs = await _programService.SearchProgramsAsync(q, pageNumber, pageSize);

                var model = new ProgramsListViewModel
                {
                    Programs = programs.ToList(),
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    SearchTerm = q
                };

                return Json(new { success = true, data = model });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching programs with term {SearchTerm}", q);
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Toggles program visibility (public/private)
        /// </summary>
        [HttpPost("programs/{id}/toggle-visibility")]
        public async Task<IActionResult> ToggleVisibility(int id)
        {
            try
            {
                var result = await _programService.ToggleProgramVisibilityAsync(id);
                if (!result)
                    return BadRequest(new { success = false, message = "Failed to toggle program visibility" });

                ShowSuccessMessage("Program visibility updated successfully");
                _logger.LogInformation("Program {ProgramId} visibility toggled by {User}", id, User.Identity?.Name);
                return Ok(new { success = true, message = "Program visibility updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling visibility for program {ProgramId}", id);
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Updates program details
        /// </summary>
        [HttpPost("programs/{id}/update")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, [FromBody] ProgramUpdateRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid request data" });

                var result = await _programService.UpdateProgramAsync(id, request);
                if (!result)
                    return BadRequest(new { success = false, message = "Failed to update program" });

                ShowSuccessMessage("Program updated successfully");
                _logger.LogInformation("Program {ProgramId} updated by {User}", id, User.Identity?.Name);
                return Ok(new { success = true, message = "Program updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating program {ProgramId}", id);
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Deletes a program
        /// </summary>
        [HttpPost("programs/{id}/delete")]
        public async Task<IActionResult> Delete(int id, [FromQuery] bool hardDelete = false)
        {
            try
            {
                var result = await _programService.DeleteProgramAsync(id, !hardDelete);
                if (!result)
                    return BadRequest(new { success = false, message = "Failed to delete program" });

                ShowSuccessMessage("Program deleted successfully");
                _logger.LogInformation("Program {ProgramId} deleted (hard: {HardDelete}) by {User}", id, hardDelete, User.Identity?.Name);
                return Ok(new { success = true, message = "Program deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting program {ProgramId}", id);
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Gets program statistics
        /// </summary>
        [HttpGet("programs/{id}/stats")]
        public async Task<IActionResult> GetStats(int id)
        {
            try
            {
                var stats = await _programService.GetProgramStatsAsync(id);
                return Ok(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting program stats for ID {ProgramId}", id);
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Gets programs summary statistics
        /// </summary>
        [HttpGet("programs/summary/overview")]
        public async Task<IActionResult> GetSummary()
        {
            try
            {
                var summary = await _programService.GetProgramsSummaryAsync();
                return Ok(new { success = true, data = summary });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting programs summary");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// AJAX endpoint for DataTable - Returns programs as JSON
        /// </summary>
        [HttpGet("programs/list-json")]
        public async Task<IActionResult> GetProgramsJson(
            int draw = 1,
            int start = 0,
            int length = 10,
            string? search = null)
        {
            try
            {
                var pageNumber = start / length + 1;
                var specs = new ProgramFilterSpecs(
                    pageNumber: pageNumber,
                    pageSize: length,
                    searchTerm: search);

                var programs = await _programService.GetAllProgramsAsync(specs);
                var totalCount = await _programService.GetProgramCountAsync(new ProgramFilterSpecs());

                return Json(new
                {
                    draw,
                    recordsTotal = totalCount,
                    recordsFiltered = totalCount,
                    data = programs
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting programs JSON");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Helper method to render partial view to string
        /// </summary>
        private async Task<string> RenderPartialViewToStringAsync(string viewName, object model)
        {
            if (string.IsNullOrEmpty(viewName))
                viewName = ControllerContext.ActionDescriptor.ActionName;

            ViewData.Model = model;

            using (var writer = new StringWriter())
            {
                IViewEngine viewEngine = HttpContext.RequestServices.GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;
                ViewEngineResult viewResult = viewEngine.FindView(ControllerContext, viewName, false);

                if (!viewResult.Success)
                    throw new InvalidOperationException($"The view '{viewName}' was not found.");

                var viewContext = new ViewContext(
                    ControllerContext,
                    viewResult.View,
                    ViewData,
                    TempData,
                    writer,
                    new HtmlHelperOptions());

                await viewResult.View.RenderAsync(viewContext);
                return writer.GetStringBuilder().ToString();
            }
        }
    }
}
