using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Ecommerce.Core.Models;
using Ecommerce.Core.Interfaces.IServices;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Ecommerce.Web.Controllers
{
     [Route("[controller]")]
     [Authorize]
     public class CartController : Controller
     {
          private readonly ICartService _cartService;

          public CartController(ICartService cartService)
          {
               _cartService = cartService;
          }

          [HttpGet]
          public async Task<IActionResult> Index()
          {
               var userEmail = User.FindFirstValue(ClaimTypes.Email);
               if (string.IsNullOrEmpty(userEmail))
                    return Unauthorized("User is not authenticated.");

               var cart = await _cartService.GetCart(userEmail);
               return View(cart);
          }

          [HttpPost("AddToCart/{productId}")]
          public async Task<IActionResult> AddToCart(int productId)
          {
               var userEmail = User.FindFirstValue(ClaimTypes.Email);
               if (string.IsNullOrEmpty(userEmail))
                    return Unauthorized("User is not authenticated.");

               await _cartService.AddItemToCart(userEmail, productId);
               TempData["SuccessMessage"] = "Product added to cart successfully.";
               return RedirectToAction("Index");
          }

          [HttpPost("RemoveFromCart/{productId}")]
          public async Task<IActionResult> RemoveFromCart(int productId)
          {
               var userEmail = User.FindFirstValue(ClaimTypes.Email);
               if (string.IsNullOrEmpty(userEmail))
                    return Unauthorized("User is not authenticated.");

               await _cartService.RemoveItemFromCart(userEmail, productId);
               return RedirectToAction("Index");
          }

          [HttpPost("ClearCart")]
          public async Task<IActionResult> ClearCart()
          {
               var userEmail = User.FindFirstValue(ClaimTypes.Email);
               if (string.IsNullOrEmpty(userEmail))
                    return Unauthorized("User is not authenticated.");

               await _cartService.ClearCart(userEmail);
               return RedirectToAction("Index");
          }
     }
}
