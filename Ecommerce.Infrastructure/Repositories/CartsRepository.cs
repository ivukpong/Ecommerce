using Ecommerce.Core.Interfaces.IRepository;
using Ecommerce.Core.Models;
using Dapper;
using System.Data;
using Ecommerce.Core.Interfaces.IFactories;
using Serilog;

namespace Ecommerce.Infrastructure.Repositories
{
     public class CartsRepository : ICartsRepository
     {
          private readonly IDbConnectionFactory _dbConnectionFactory;
          private readonly ILogger _logger; // Change ILogger<CartsRepository> to Serilog's ILogger

          public CartsRepository(IDbConnectionFactory dbConnectionFactory, ILogger logger)
          {
               _dbConnectionFactory = dbConnectionFactory;
               _logger = logger; // Assign the logger
          }

          public async Task<Cart> GetCart(string userId)
          {
               _logger.Information("Fetching cart for user {UserId}", userId); // Serilog log method
               using var connection = _dbConnectionFactory.CreateECommerceDbConnection();
               var queryCart = "proc_GetCartByUserId";
               var queryCartItems = "proc_GetCartItemsByCartId";

               var parameters = new DynamicParameters();
               parameters.Add("@UserId", userId);

               var cart = await connection.QueryFirstOrDefaultAsync<Cart>(queryCart, parameters, commandType: CommandType.StoredProcedure);

               if (cart != null)
               {
                    var cartItemsParams = new DynamicParameters();
                    cartItemsParams.Add("@CartId", cart.CartId);
                    var cartItems = await connection.QueryAsync<CartItem, Product, CartItem>(
                        queryCartItems,
                        (cartItem, product) =>
                        {
                             cartItem.Product = product;
                             return cartItem;
                        },
                        cartItemsParams,
                        splitOn: "ProductId",
                        commandType: CommandType.StoredProcedure
                    );
                    cart.Items = cartItems.ToList();
               }

               _logger.Information("Cart retrieved for user {UserId}: {Cart}", userId, cart);
               return cart;
          }

          public async Task ClearCart(string userId)
          {
               _logger.Information("Clearing cart for user {UserId}", userId);
               using var connection = _dbConnectionFactory.CreateECommerceDbConnection();
               var query = "proc_ClearCartItemsByUserId";
               var parameters = new DynamicParameters();
               parameters.Add("@UserId", userId);

               await connection.ExecuteAsync(query, parameters, commandType: CommandType.StoredProcedure);
               _logger.Information("Cart cleared for user {UserId}", userId);
          }

          public async Task UpdateCart(Cart cart)
          {
               _logger.Information("Updating cart for user {UserId}", cart.UserId);
               using var connection = _dbConnectionFactory.CreateECommerceDbConnection();
               var parameters = new DynamicParameters();
               parameters.Add("@UserId", cart.UserId);

               var cartId = await connection.ExecuteScalarAsync<int>("proc_CheckIfCartExist", parameters, commandType: CommandType.StoredProcedure);
               if (cartId == 0)
               {
                    var createCartParams = new DynamicParameters();
                    createCartParams.Add("@UserId", cart.UserId);
                    cartId = await connection.ExecuteScalarAsync<int>("proc_CreateCart", createCartParams, commandType: CommandType.StoredProcedure);
               }

               foreach (var item in cart.Items)
               {
                    var upsertParams = new DynamicParameters();
                    upsertParams.Add("@CartId", cartId);
                    upsertParams.Add("@ProductId", item.ProductId);
                    upsertParams.Add("@Quantity", item.Quantity);
                    await connection.ExecuteAsync("proc_UpsertCartItem", upsertParams, commandType: CommandType.StoredProcedure);
               }

               _logger.Information("Cart updated for user {UserId}", cart.UserId);
          }

          public async Task<List<Cart>> GetAllCarts()
          {
               _logger.Information("Fetching all carts");
               using var connection = _dbConnectionFactory.CreateECommerceDbConnection();
               var query = "proc_GetAllCartsWithItems";
               var carts = new List<Cart>();
               var cartItems = new List<CartItem>();
               using (var multi = await connection.QueryMultipleAsync(query, commandType: CommandType.StoredProcedure))
               {
                    carts = (await multi.ReadAsync<Cart>()).ToList();
                    cartItems = (await multi.ReadAsync<CartItem>()).ToList();
               }
               foreach (var cart in carts)
               {
                    cart.Items = cartItems.Where(item => item.CartId == cart.CartId).ToList();
               }
               _logger.Information("Retrieved {Count} carts", carts.Count);
               return carts;
          }

          public async Task AddItemToCart(Cart cart)
          {
               _logger.Information("Adding item(s) to cart for user {UserId}", cart.UserId);
               using var connection = _dbConnectionFactory.CreateECommerceDbConnection();
               var parameters = new DynamicParameters();
               parameters.Add("@UserId", cart.UserId);

               var cartId = await connection.ExecuteScalarAsync<int>("proc_CheckIfCartExist", parameters, commandType: CommandType.StoredProcedure);
               if (cartId == 0)
               {
                    var createCartParams = new DynamicParameters();
                    createCartParams.Add("@UserId", cart.UserId);
                    cartId = await connection.ExecuteScalarAsync<int>("proc_CreateCart", createCartParams, commandType: CommandType.StoredProcedure);
               }

               foreach (var item in cart.Items)
               {
                    var insertItemParams = new DynamicParameters();
                    insertItemParams.Add("@CartId", cartId);
                    insertItemParams.Add("@ProductId", item.ProductId);
                    insertItemParams.Add("@Quantity", item.Quantity);
                    await connection.ExecuteAsync("proc_AddItemToCart", insertItemParams, commandType: CommandType.StoredProcedure);
               }
               _logger.Information("Item(s) added to cart for user {UserId}", cart.UserId);
          }

          public async Task RemoveItemFromCart(string userId, int productId)
          {
               _logger.Information("Removing item {ProductId} from cart for user {UserId}", productId, userId);
               using var connection = _dbConnectionFactory.CreateECommerceDbConnection();
               var parameters = new DynamicParameters();
               parameters.Add("@UserId", userId);
               parameters.Add("@ProductId", productId);
               await connection.ExecuteAsync("proc_RemoveItemFromCart", parameters, commandType: CommandType.StoredProcedure);
               _logger.Information("Item {ProductId} removed from cart for user {UserId}", productId, userId);
          }
     }
}
