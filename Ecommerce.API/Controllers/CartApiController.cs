using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Ecommerce.Core.Interfaces.IServices;
using System.Security.Claims;

namespace Ecommerce.Api.Controllers
{
     [Route("api/[controller]")]
     [ApiController]
     [Authorize]
     public class CartApiController : ControllerBase
     {
          private readonly ICartService _cartService;

          public CartApiController(ICartService cartService)
          {
               _cartService = cartService;
          }

          [HttpGet]
          public async Task<IActionResult> GetCart()
          {
               var userEmail = User.FindFirstValue(ClaimTypes.Email);
               var cart = await _cartService.GetCart(userEmail);
               return Ok(cart);
          }

          [HttpPost("AddToCart/{productId}")]
          public async Task<IActionResult> AddToCart(int productId)
          {
               var userEmail = User.FindFirstValue(ClaimTypes.Email);
               await _cartService.AddItemToCart(userEmail, productId);
               return Ok();
          }

          [HttpPost("RemoveFromCart/{productId}")]
          public async Task<IActionResult> RemoveFromCart(int productId)
          {
               var userEmail = User.FindFirstValue(ClaimTypes.Email);
               await _cartService.RemoveItemFromCart(userEmail, productId);
               return Ok();
          }

          [HttpPost("ClearCart")]
          public async Task<IActionResult> ClearCart()
          {
               var userEmail = User.FindFirstValue(ClaimTypes.Email);
               await _cartService.ClearCart(userEmail);
               return Ok();
          }
     }
}
