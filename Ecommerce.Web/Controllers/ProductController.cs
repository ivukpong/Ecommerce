using Ecommerce.Core.Interfaces.IServices;
using Ecommerce.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Web.Controllers
{
     [Route("[controller]")]
     public class ProductController : Controller
     {
          private readonly IProductsService _productsService;

          public ProductController(IProductsService productsService)
          {
               _productsService = productsService;
          }

          // Display all products (Read)
          [HttpGet]
          public async Task<IActionResult> Index()
          {
               var products = await _productsService.GetAllProducts();
               return View(products); // Return list of products to the view
          }

          // Display specific product details (Read)
          [HttpGet("{id}")]
          public async Task<IActionResult> Details(int id)
          {
               var product = await _productsService.GetProductById(id);

               if (product == null)
               {
                    return NotFound();
               }

               return View(product); // Return product details to the view
          }

          // Display form for creating a new product (Create - GET)
          [HttpGet("create")]
          public IActionResult Create()
          {
               return View(); // Return the create product form
          }

          // Handle form submission for creating a new product (Create - POST)
          [HttpPost("create")]
          [ValidateAntiForgeryToken]
          public async Task<IActionResult> Create([Bind("Name,Description,Price,ImageUrl")] Product product)
          {
               if (ModelState.IsValid)
               {
                    await _productsService.CreateProduct(product);
                    return RedirectToAction(nameof(Index)); // Redirect to the list of products
               }
               return View(product); // Return the form with validation errors
          }

          // Display form for editing a product (Update - GET)
          [HttpGet("edit/{id}")]
          public async Task<IActionResult> Edit(int id)
          {
               var product = await _productsService.GetProductById(id);

               if (product == null)
               {
                    return NotFound();
               }

               return View(product); // Return the edit product form
          }

          // Handle form submission for updating a product (Update - POST)
          [HttpPost("edit/{id}")]
          [ValidateAntiForgeryToken]
          public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Price,ImageUrl")] Product product)
          {
               if (id != product.ProductId)
               {
                    return BadRequest();
               }

               if (ModelState.IsValid)
               {
                    try
                    {
                        await _productsService.UpdateProduct(product);
                         return RedirectToAction(nameof(Index)); // Redirect to the list of products
                    }
                    catch
                    {
                         if (!ProductExists(product.ProductId).Result)
                         {
                              return NotFound();
                         }
                         else
                         {
                              throw;
                         }
                    }
               }
               return View(product); // Return the form with validation errors
          }

          // Display confirmation for deleting a product (Delete - GET)
          [HttpGet("delete/{id}")]
          public async Task<IActionResult> Delete(int id)
          {
               var product = await _productsService.GetProductById(id);

               if (product == null)
               {
                    return NotFound();
               }

               return View(product); // Return the delete confirmation page
          }

          // Handle product deletion (Delete - POST)
          [HttpPost("delete/{id}"), ActionName("Delete")]
          [ValidateAntiForgeryToken]
          public async Task<IActionResult> DeleteConfirmed(int id)
          {
               var product = await _productsService.GetProductById(id);
               if (product != null)
               {
                    await _productsService.DeleteProduct(id);
               }
               return RedirectToAction(nameof(Index)); // Redirect to the list of products
          }

          // Check if a product exists by ID
          private async Task<bool> ProductExists(int id)
          {
               return await _productsService.CheckForProduct(id);
          }
     }
}
