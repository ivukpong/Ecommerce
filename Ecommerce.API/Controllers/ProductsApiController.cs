using Ecommerce.Core.Interfaces.IServices;
using Ecommerce.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers
{
     [Route("api/[controller]")]
     [ApiController]
     public class ProductsApiController : ControllerBase
     {
          private readonly IProductsService _productsService;

          public ProductsApiController(IProductsService productsService)
          {
               _productsService = productsService;
          }

          [HttpGet]
          public async Task<IActionResult> GetProducts()
          {
               var products = await _productsService.GetAllProducts();
               return Ok(products); // Return list of products as JSON
          }

          [HttpGet("{id}")]
          public async Task<IActionResult> GetProductById(int id)
          {
               var product = await _productsService.GetProductById(id);
               if (product == null)
               {
                    return NotFound();
               }
               return Ok(product); // Return product details as JSON
          }

          [HttpPost]
          public async Task<IActionResult> CreateProduct(Product product)
          {
               await _productsService.CreateProduct(product);
               return CreatedAtAction(nameof(GetProductById), new { id = product.ProductId }, product);
          }

          [HttpPut("{id}")]
          public async Task<IActionResult> UpdateProduct(int id, Product product)
          {
               if (id != product.ProductId)
               {
                    return BadRequest();
               }

               await _productsService.UpdateProduct(product);
               return NoContent();
          }

          [HttpDelete("{id}")]
          public async Task<IActionResult> DeleteProduct(int id)
          {
               await _productsService.DeleteProduct(id);
               return NoContent();
          }
     }
}
