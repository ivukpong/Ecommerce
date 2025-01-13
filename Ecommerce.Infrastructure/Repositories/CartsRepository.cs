using Ecommerce.Core.Interfaces.IRepository;
using Ecommerce.Core.Models;
using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Ecommerce.Core.Interfaces.IFactories;

namespace Ecommerce.Infrastructure.Repositories
{
     public class CartsRepository : ICartsRepository
     {
          private readonly IDbConnectionFactory _dbConnectionFactory;

          public CartsRepository(IDbConnectionFactory dbConnectionFactory)
          {
               _dbConnectionFactory = dbConnectionFactory;
          }

          public async Task<Cart> GetCart(string userId)
          {
               using var connection = _dbConnectionFactory.CreateECommerceDbConnection();
               var queryCart = "proc_GetCartByUserId";
               var queryCartItems = "proc_GetCartItemsByCartId";

               var parameters = new DynamicParameters();
               parameters.Add("@UserId", userId);

               // Get cart
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

               return cart;
          }

          public async Task ClearCart(string userId)
          {
               using var connection = _dbConnectionFactory.CreateECommerceDbConnection();
               var query = "proc_ClearCartItemsByUserId";

               var parameters = new DynamicParameters();
               parameters.Add("@UserId", userId);

               await connection.ExecuteAsync(query, parameters, commandType: CommandType.StoredProcedure);
          }

          public async Task UpdateCart(Cart cart)
          {
               using var connection = _dbConnectionFactory.CreateECommerceDbConnection();

               var parameters = new DynamicParameters();
               parameters.Add("@UserId", cart.UserId);

               // Check if cart exists, if not create
               var cartId = await connection.ExecuteScalarAsync<int>("proc_CheckIfCartExist", parameters, commandType: CommandType.StoredProcedure);

               if (cartId == 0)
               {
                    var createCartParams = new DynamicParameters();
                    createCartParams.Add("@UserId", cart.UserId);
                    cartId = await connection.ExecuteScalarAsync<int>("proc_CreateCart", createCartParams, commandType: CommandType.StoredProcedure);
               }

               // Upsert cart items
               foreach (var item in cart.Items)
               {
                    var upsertParams = new DynamicParameters();
                    upsertParams.Add("@CartId", cart.CartId);
                    upsertParams.Add("@ProductId", item.ProductId);
                    upsertParams.Add("@Quantity", item.Quantity);

                    await connection.ExecuteAsync("proc_UpsertCartItem", upsertParams, commandType: CommandType.StoredProcedure);
               }
          }

          public async Task<List<Cart>> GetAllCarts()
          {
               using var connection = _dbConnectionFactory.CreateECommerceDbConnection();
               var query = "proc_GetAllCartsWithItems";

               var carts = new List<Cart>();
               var cartItems = new List<CartItem>();

               var multiParams = new DynamicParameters();

               using (var multi = await connection.QueryMultipleAsync(query, multiParams, commandType: CommandType.StoredProcedure))
               {
                    carts = (await multi.ReadAsync<Cart>()).ToList();
                    cartItems = (await multi.ReadAsync<CartItem>()).ToList();
               }

               // Assign items to the corresponding carts
               foreach (var cart in carts)
               {
                    cart.Items = cartItems.Where(item => item.CartId == cart.CartId).ToList();
               }

               return carts;
          }

          public async Task AddItemToCart(Cart cart)
          {
               using var connection = _dbConnectionFactory.CreateECommerceDbConnection();

               var parameters = new DynamicParameters();
               parameters.Add("@UserId", cart.UserId);

               // Ensure the cart exists, if not, create a new cart
               var cartId = await connection.ExecuteScalarAsync<int>("proc_CheckIfCartExist", parameters, commandType: CommandType.StoredProcedure);

               if (cartId == 0)
               {
                    var createCartParams = new DynamicParameters();
                    createCartParams.Add("@UserId", cart.UserId);
                    cartId = await connection.ExecuteScalarAsync<int>("proc_CreateCart", createCartParams, commandType: CommandType.StoredProcedure);
               }

               // Add items to the cart
               foreach (var item in cart.Items)
               {
                    var insertItemParams = new DynamicParameters();
                    insertItemParams.Add("@CartId", cartId);
                    insertItemParams.Add("@ProductId", item.ProductId);
                    insertItemParams.Add("@Quantity", item.Quantity);

                    await connection.ExecuteAsync("proc_AddItemToCart", insertItemParams, commandType: CommandType.StoredProcedure);
               }
          }

          public async Task RemoveItemFromCart(string userId, int productId)
          {
               using var connection = _dbConnectionFactory.CreateECommerceDbConnection();

               var parameters = new DynamicParameters();
               parameters.Add("@UserId", userId);
               parameters.Add("@ProductId", productId);

               // Remove the item from the CartItems table
               await connection.ExecuteAsync("proc_RemoveItemFromCart", parameters, commandType: CommandType.StoredProcedure);
          }
     }
}
