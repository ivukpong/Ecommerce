using Ecommerce.Core.Models;

namespace Ecommerce.Core.Interfaces.IServices
{
     public interface IProductsService
     {
          Task<List<Product>> GetAllProducts();

          Task<Product> GetProductById(int id);

          Task CreateProduct(Product product);

          Task UpdateProduct(Product product);

          Task<Product> DeleteProduct(int id);

          Task<bool> CheckForProduct(int id);

     }

}
