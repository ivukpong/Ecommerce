using Ecommerce.Core.Interfaces.IServices;
using Ecommerce.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Web.Controllers
{
     [ApiExplorerSettings(IgnoreApi = true)]
     [Route("[controller]")]
     public class AccountController : Controller
     {
          private readonly IUserService _userService;
          private readonly IConfiguration _configuration;

          public AccountController(IUserService userService, IConfiguration configuration)
          {
               _userService = userService;
               _configuration = configuration;
          }

          // Render registration view
          [HttpGet("register")]
          public IActionResult Register()
          {
               return View();
          }

          // Handle registration POST request
          [HttpPost("register")]
          [ValidateAntiForgeryToken]
          public async Task<IActionResult> Register([Bind("Email,Username,Password,ConfirmPassword")] RegisterViewModel model)
          {
               if (ModelState.IsValid)
               {
                    try
                    {
                         // Register user
                         await _userService.Register(model);

                         // Redirect to login page after registration
                         return RedirectToAction("Login");
                    }
                    catch (Exception ex)
                    {
                         // Capture the error and add it to the ModelState
                         ModelState.AddModelError(string.Empty, ex.Message);
                    }
               }
               else
               {
                    ModelState.AddModelError(string.Empty, "Please correct the errors and try again.");
               }

               // If the model state is invalid, return the same view with validation errors
               return View(model);
          }

          // Render login view
          [HttpGet("login")]
          public IActionResult Login()
          {
               return View();
          }

          // Handle login POST request
          [HttpPost("login")]
          [ValidateAntiForgeryToken]
          public async Task<IActionResult> Login([Bind("Email,Password")] LoginViewModel model)
          {
               if (ModelState.IsValid)
               {
                    try
                    {
                         // Call the service to handle login
                         var loginResult = await _userService.Login(model);

                         // Store the JWT token in the cookie
                         Response.Cookies.Append("jwtToken", loginResult.Token, new CookieOptions
                         {
                              HttpOnly = true,
                              Secure = true,
                              SameSite = SameSiteMode.Strict,
                              Expires = DateTime.UtcNow.AddHours(1)
                         });

                         // Redirect to the homepage or user dashboard
                         return RedirectToAction("Index", "Home");
                    }
                    catch (Exception ex)
                    {
                         // Capture the error and add it to the ModelState
                         ModelState.AddModelError(string.Empty, ex.Message);
                    }
               }
               else
               {
                    ModelState.AddModelError(string.Empty, "Please correct the errors and try again.");
               }

               // If the model state is invalid, return the same view with validation errors
               return View(model);
          }

          // Handle logout
          [HttpPost("logout")]
          [ValidateAntiForgeryToken]
          public IActionResult Logout()
          {
               // Remove the JWT token cookie
               Response.Cookies.Delete("jwtToken");

               // Optionally, you can also clear any session or temporary data

               // Redirect to login page or homepage
               return RedirectToAction("Login", "Account");
          }

          [HttpGet("AccessDenied")]
          public IActionResult AccessDenied()
          {
               return View();
          }

          // Helper method to check if the user is authenticated
          private bool IsUserAuthenticated()
          {
               var email = HttpContext.Items["UserEmail"] as string;
               return !string.IsNullOrEmpty(email);
          }
     }
}
