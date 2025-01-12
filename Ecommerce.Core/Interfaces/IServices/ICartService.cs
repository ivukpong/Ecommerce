using Ecommerce.Core.Models;

namespace Ecommerce.Core.Interfaces.IServices
{
     public interface ICartService
     {
         
          Task<Cart> GetCart(string userId);

          Task AddItemToCart(string userId, int productId);
       
          Task RemoveItemFromCart(string userId, int productId);

          Task ClearCart(string userId);
     }
}
