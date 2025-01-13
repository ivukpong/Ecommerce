using Ecommerce.Core.Interfaces.IServices;
using Ecommerce.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers
{
     [Route("api/[controller]")]
     [ApiController]
     public class AccountApiController : ControllerBase
     {
          private readonly IUserService _userService;
          private readonly IConfiguration _configuration;

          public AccountApiController(IUserService userService, IConfiguration configuration)
          {
               _userService = userService;
               _configuration = configuration;
          }

          [HttpPost("register")]
          public async Task<IActionResult> Register(RegisterViewModel model)
          {
               if (ModelState.IsValid)
               {
                    await _userService.Register(model);
                    return Ok();
               }

               return BadRequest(ModelState);
          }

          [HttpPost("login")]
          public async Task<IActionResult> Login(LoginViewModel model)
          {
               if (ModelState.IsValid)
               {
                    try
                    {
                         var loginResult = await _userService.Login(model);
                         return Ok(new { token = loginResult.Token });
                    }
                    catch (Exception ex)
                    {
                         return Unauthorized(ex.Message);
                    }
               }

               return BadRequest(ModelState);
          }

          [HttpPost("logout")]
          [Authorize]
          public IActionResult Logout()
          {
               // Log out user
               return Ok("Logged out successfully");
          }
     }
}
