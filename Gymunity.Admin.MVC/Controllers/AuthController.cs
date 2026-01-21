using Gymunity.Domain.Entities.Identity;
using Gymunity.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Gymunity.Application.DTOs.Account;

namespace Gymunity.Admin.MVC.Controllers
{
    public class AuthController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager) : Controller
    {
        private readonly UserManager<AppUser> userManager = userManager;
        private readonly SignInManager<AppUser> signInManager = signInManager;

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            if (!ModelState.IsValid)
                return View(request);

            var user = (await userManager.FindByEmailAsync(request.EmailOrUserName))
                       ?? await userManager.FindByNameAsync(request.EmailOrUserName);

            if (user is null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(request);
            }

            var isAdmin = await userManager.IsInRoleAsync(user, UserRole.Admin.ToString());

            if (!isAdmin)
            {
                ModelState.AddModelError(string.Empty, "Access denied. You Don't have the permesion.");
                return View(request);
            }

            var result = await signInManager.PasswordSignInAsync(user, request.Password, isPersistent: false, lockoutOnFailure: true);

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(request);
            }
            return RedirectToAction("Index", "Dashboard");
        }
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Login", "Auth");
        }
    }
}