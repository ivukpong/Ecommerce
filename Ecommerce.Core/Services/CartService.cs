using Ecommerce.Core.Interfaces.IRepository;
using Ecommerce.Core.Interfaces.IServices;
using Ecommerce.Core.Models;
using Serilog;
using System.Linq;
using System.Threading.Tasks;

namespace Ecommerce.Core.Services
{
     public class CartService : ICartService
     {
          private readonly ICartsRepository _cartsRepository;
          private readonly IProductsRepository _productsRepository;
          private readonly ILogger _logger;

          public CartService(ICartsRepository cartsRepository, IProductsRepository productsRepository, ILogger logger)
          {
               _cartsRepository = cartsRepository;
               _productsRepository = productsRepository;
               _logger = logger;
          }

          public async Task AddItemToCart(string userId, int productId)
          {
               _logger.Information("Attempting to add product {ProductId} to cart for user {UserId}.", productId, userId);

               // Check if the user already has a cart
               var cart = await _cartsRepository.GetCart(userId);

               // If no cart exists, create a new one
               if (cart == null)
               {
                    cart = new Cart { UserId = userId, Items = new List<CartItem>() };
                    await _cartsRepository.AddItemToCart(cart);  // Assuming CreateCart handles the insert
                    _logger.Information("Created a new cart for user {UserId}.", userId);
               }

               var product = await _productsRepository.GetProductById(productId);
               if (product == null)
               {
                    _logger.Error("Product {ProductId} not found when adding to cart for user {UserId}.", productId, userId);
                    throw new KeyNotFoundException("Product not found.");
               }

               // Check if the product is already in the cart
               var cartItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);

               if (cartItem == null)
               {
                    cart.Items.Add(new CartItem { ProductId = productId, Quantity = 1 });
                    _logger.Information("Added new product {ProductId} to cart for user {UserId}.", productId, userId);
               }
               else
               {
                    cartItem.Quantity++;
                    _logger.Information("Incremented quantity of product {ProductId} in cart for user {UserId}.", productId, userId);
               }

               // Update the cart with the new item or updated quantity
               await _cartsRepository.UpdateCart(cart);
               _logger.Information("Updated cart for user {UserId} after adding product {ProductId}.", userId, productId);
          }

          public async Task ClearCart(string userId)
          {
               _logger.Information("Clearing cart for user {UserId}.", userId);
               await _cartsRepository.ClearCart(userId);
               _logger.Information("Cart cleared for user {UserId}.", userId);
          }

          public async Task<Cart> GetCart(string userId)
          {
               _logger.Information("Retrieving cart for user {UserId}.", userId);
               var cart = await _cartsRepository.GetCart(userId);
               if (cart == null)
               {
                    _logger.Warning("Cart not found for user {UserId}.", userId);
               }
               return cart;
          }

          public async Task RemoveItemFromCart(string userId, int productId)
          {
               _logger.Information("Attempting to remove product {ProductId} from cart for user {UserId}.", productId, userId);

               var cart = await _cartsRepository.GetCart(userId);

               if (cart == null)
               {
                    _logger.Error("Cart not found for user {UserId}.", userId);
                    throw new KeyNotFoundException("Cart not found.");
               }

               var cartItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);
               if (cartItem != null)
               {
                    cart.Items.Remove(cartItem);
                    await _cartsRepository.UpdateCart(cart);
                    _logger.Information("Removed product {ProductId} from cart for user {UserId}.", productId, userId);
               }
               else
               {
                    _logger.Warning("Product {ProductId} not found in cart for user {UserId}.", productId, userId);
               }
          }
     }
}
