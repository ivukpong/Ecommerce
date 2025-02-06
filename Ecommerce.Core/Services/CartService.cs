using Ecommerce.Core.Interfaces.IRepository;
using Ecommerce.Core.Interfaces.IServices;
using Ecommerce.Core.Models;
using Serilog;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

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
               _logger.Information("Adding product {ProductId} to cart for user {UserId}.", productId, userId);

               var cart = await GetOrCreateCart(userId);

               var product = await _productsRepository.GetProductById(productId);
               if (product == null)
               {
                    _logger.Error("Product {ProductId} not found for user {UserId}.", productId, userId);
                    throw new KeyNotFoundException($"Product with ID {productId} not found.");
               }

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

               await _cartsRepository.UpdateCart(cart);
               _logger.Information("Cart updated for user {UserId} after adding product {ProductId}.", userId, productId);
          }

          public async Task<Cart> GetCart(string userId)
          {
               _logger.Information("Retrieving cart for user {UserId}.", userId);
               var cart = await _cartsRepository.GetCart(userId);
               if (cart == null)
               {
                    _logger.Warning("No cart found for user {UserId}.", userId);
                    return new Cart { UserId = userId, Items = new List<CartItem>() };
               }
               return cart;
          }

          public async Task RemoveItemFromCart(string userId, int productId)
          {
               _logger.Information("Removing product {ProductId} from cart for user {UserId}.", productId, userId);

               var cart = await _cartsRepository.GetCart(userId);
               if (cart == null)
               {
                    _logger.Error("No cart found for user {UserId}.", userId);
                    throw new KeyNotFoundException("Cart not found.");
               }

               var cartItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);
               if (cartItem != null)
               {
                    cart.Items.Remove(cartItem);
                    await _cartsRepository.UpdateCart(cart);
                    _logger.Information("Product {ProductId} removed from cart for user {UserId}.", productId, userId);
               }
               else
               {
                    _logger.Warning("Product {ProductId} not found in cart for user {UserId}.", productId, userId);
               }
          }

          public async Task ClearCart(string userId)
          {
               _logger.Information("Clearing cart for user {UserId}.", userId);

               await _cartsRepository.ClearCart(userId);
               _logger.Information("Cart cleared for user {UserId}.", userId);
          }

          private async Task<Cart> GetOrCreateCart(string userId)
          {
               var cart = await _cartsRepository.GetCart(userId);
               if (cart == null)
               {
                    cart = new Cart { UserId = userId, Items = new List<CartItem>() };
                    await _cartsRepository.AddItemToCart(cart);
                    _logger.Information("Created a new cart for user {UserId}.", userId);
               }
               return cart;
          }
     }
}
