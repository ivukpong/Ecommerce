using Ecommerce.Core.Interfaces.IServices;
using Ecommerce.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Web.Controllers
{
     public class HomeController : Controller
     {
          private readonly IProductsService _productsService;
          private readonly IUserService _userService;

          public HomeController(IProductsService productsService, IUserService userService)
          {
               _productsService = productsService;
               _userService = userService;
          }

          public async Task<IActionResult> Index()
          {
               var userEmail = HttpContext.Items["UserEmail"] as string;
               var featuredProducts = await _productsService.GetAllProducts();
               var user = await _userService.GetUser(userEmail);
               var viewModel = new HomeViewModel
               {              
                    FeaturedProducts = featuredProducts,
                    Username = user?.Username ?? ""
               };

               return View(viewModel);
          }
     }
}
