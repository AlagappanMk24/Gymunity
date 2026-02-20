using Gymunity.Application.DTOs.Auth;
using Gymunity.Domain.Entities.Identity;
using Gymunity.Domain.Enums;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Gymunity.Admin.MVC.Controllers
{
    public class AuthController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IConfiguration configuration) : Controller
    {
        private readonly UserManager<AppUser> _userManager = userManager;
        private readonly SignInManager<AppUser> _signInManager = signInManager;
        private readonly IConfiguration _configuration = configuration;

        [HttpGet]
        public IActionResult Login()
        {
            // If user is already authenticated, redirect to dashboard
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Dashboard");
            }
            return View();
        }

        //[HttpPost]
        //public async Task<IActionResult> Login(LoginRequest request)
        //{
        //    if (!ModelState.IsValid)
        //        return View(request);

        //    var user = (await _userManager.FindByEmailAsync(request.EmailOrUserName))
        //               ?? await _userManager.FindByNameAsync(request.EmailOrUserName);

        //    if (user is null)
        //    {
        //        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        //        return View(request);
        //    }

        //    var isAdmin = await _userManager.IsInRoleAsync(user, UserRole.Admin.ToString());

        //    if (!isAdmin)
        //    {
        //        ModelState.AddModelError(string.Empty, "Access denied. You Don't have the permesion.");
        //        return View(request);
        //    }

        //    var result = await _signInManager.PasswordSignInAsync(user, request.Password,
        //       isPersistent: true, 
        //       lockoutOnFailure: true);

        //    if (!result.Succeeded)
        //    {
        //        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        //        return View(request);
        //    }
        //    return RedirectToAction("Index", "Dashboard");
        //}

        [HttpPost]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            if (!ModelState.IsValid)
                return View(request);

            var user = (await _userManager.FindByEmailAsync(request.EmailOrUserName))
                       ?? await _userManager.FindByNameAsync(request.EmailOrUserName);

            if (user is null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(request);
            }

            var isAdmin = await _userManager.IsInRoleAsync(user, UserRole.Admin.ToString());

            if (!isAdmin)
            {
                ModelState.AddModelError(string.Empty, "Access denied. You Don't have the permesion.");
                return View(request);
            }

            // Verify password
            var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!passwordValid)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(request);
            }

            // Create claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, UserRole.Admin.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, IdentityConstants.ApplicationScheme);

            // IMPORTANT: Set expiration explicitly
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,  // This creates a persistent cookie
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(1),  // Set to 1 minute
                AllowRefresh = false  // Don't allow sliding expiration
            };

            // Sign in manually
            await HttpContext.SignInAsync(
                IdentityConstants.ApplicationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            Console.WriteLine($"[{DateTime.Now}] Login successful. Cookie expires at: {authProperties.ExpiresUtc}");

            return RedirectToAction("Index", "Dashboard");
        }
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Auth");
        }

        /// <summary>
        /// Gets the JWT token for the current authenticated user
        /// This endpoint extracts the token from the authentication cookie
        /// </summary>
        [Authorize]
        [HttpGet("token")]
        public async Task<IActionResult> GetToken()
        {
            try
            {
                // Get the authenticated user
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized(new { error = "User not found" });
                }

                // Check if user is admin
                var isAdmin = await _userManager.IsInRoleAsync(user, UserRole.Admin.ToString());
                if (!isAdmin)
                {
                    return Forbid();
                }

                // Generate JWT token (you need to implement this method)
                var token = await GenerateJwtTokenAsync(user);

                return Ok(new { token });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
        private async Task<string> GenerateJwtTokenAsync(AppUser user)
        {
            var jwtKey = _configuration["JWT:Key"] ?? "YourStrongSecretKeyHere_MakeItLongAndComplex";
            var jwtIssuer = _configuration["JWT:Issuer"] ?? "https://localhost:7182";
            var jwtAudience = _configuration["JWT:Audience"] ?? "MySecuredAPIsUsers";

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(60),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}