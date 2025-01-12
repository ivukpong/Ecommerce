using Ecommerce.Core.Models;

namespace Ecommerce.Core.Interfaces.IRepository
{
     public interface ICartsRepository
     {
          Task<List<Cart>> GetAllCarts();

          Task<Cart> GetCart(string userId);

          Task AddItemToCart(Cart cart);

          Task UpdateCart(Cart cart);

          Task RemoveItemFromCart(string userId, int productId);

          Task ClearCart(string userId);
     }
}
