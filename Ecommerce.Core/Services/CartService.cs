using Ecommerce.Core.Interfaces.IRepository;
using Ecommerce.Core.Interfaces.IServices;
using Ecommerce.Core.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Ecommerce.Core.Services
{
     public class CartService : ICartService
     {
          private readonly ICartsRepository _cartsRepository;
          private readonly IProductsRepository _productsRepository;

          public CartService(ICartsRepository cartsRepository, IProductsRepository productsRepository)
          {
               _cartsRepository = cartsRepository;
               _productsRepository = productsRepository;
          }

          public async Task AddItemToCart(string userId, int productId)
          {
               // Check if the user already has a cart
               var cart = await _cartsRepository.GetCart(userId);

               // If no cart exists, create a new one
               if (cart == null)
               {
                    cart = new Cart { UserId = userId, Items = new List<CartItem>() };
                    await _cartsRepository.AddItemToCart(cart);  // Assuming CreateCart handles the insert
               }

               var product = await _productsRepository.GetProductById(productId);
               if (product == null)
               {
                    throw new KeyNotFoundException("Product not found.");
               }

               // Check if the product is already in the cart
               var cartItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);

               if (cartItem == null)
               {
                    cart.Items.Add(new CartItem { ProductId = productId, Quantity = 1 });
               }
               else
               {
                    cartItem.Quantity++;
               }

               // Update the cart with the new item or updated quantity
               await _cartsRepository.UpdateCart(cart);
          }


          public async Task ClearCart(string userId)
          {
               await _cartsRepository.ClearCart(userId);
          }

          public Task<List<Cart>> GetAllCarts()
          {
               throw new NotImplementedException();
          }

          public async Task<Cart> GetCart(string userId)
          {
               return await _cartsRepository.GetCart(userId);
          }

          public async Task RemoveItemFromCart(string userId, int productId)
          {
               var cart = await _cartsRepository.GetCart(userId);

               if (cart == null)
                    throw new KeyNotFoundException("Cart not found.");

               var cartItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);
               if (cartItem != null)
               {
                    cart.Items.Remove(cartItem);
                    await _cartsRepository.UpdateCart(cart);
               }
          }
     }
}
