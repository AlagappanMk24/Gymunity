using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gymunity.Admin.MVC.Controllers
{
    /// <summary>
    /// Base controller for all admin controllers.
    /// Ensures authorization and provides common functionality.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [Route("admin")]
    public abstract class BaseAdminController : Controller
    {
        /// <summary>
        /// Sets the page title for the view.
        /// </summary>
        protected void SetPageTitle(string title)
        {
            ViewData["Title"] = title;
        }

        /// <summary>
        /// Sets breadcrumb items for navigation.
        /// </summary>
        protected void SetBreadcrumbs(params string[] items)
        {
            ViewBag.BreadcrumbItems = items.ToList();
        }

        /// <summary>
        /// Shows a success message.
        /// </summary>
        protected void ShowSuccessMessage(string message)
        {
            TempData["SuccessMessage"] = message;
        }

        /// <summary>
        /// Shows an error message.
        /// </summary>
        protected void ShowErrorMessage(string message)
        {
            TempData["ErrorMessage"] = message;
        }

        /// <summary>
        /// Shows a warning message.
        /// </summary>
        protected void ShowWarningMessage(string message)
        {
            TempData["WarningMessage"] = message;
        }

        /// <summary>
        /// Redirects to admin dashboard.
        /// </summary>
        protected IActionResult RedirectToDashboard()
        {
            return RedirectToAction("Index", "Dashboard");
        }

        /// <summary>
        /// Redirects to specific admin page.
        /// </summary>
        protected IActionResult RedirectToAdminPage(string action, string controller)
        {
            return RedirectToAction(action, controller);
        }
    }
}
