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
               var queryCart = @"SELECT * FROM [dbo].[Carts] WHERE [UserId] = @UserId";
               var queryCartItems = @"
SELECT ci.*, p.* 
FROM [dbo].[CartItems] ci
LEFT JOIN [dbo].[Products] p ON ci.ProductId = p.ProductId
WHERE ci.CartId = (SELECT TOP 1 [CartId] FROM [dbo].[Carts] WHERE [UserId] = @UserId);
";

               var cart = await connection.QueryFirstOrDefaultAsync<Cart>(queryCart, new { UserId = userId });
               if (cart != null)
               {
                    var cartItems = await connection.QueryAsync(queryCartItems, new { UserId = userId });
                    cart.Items = cartItems.Select(row => new CartItem
                    {
                         CartItemId = row.CartItemId,
                         CartId = row.CartId,
                         ProductId = row.ProductId,
                         Quantity = row.Quantity,
                         Product = new Product
                         {
                              ProductId = row.ProductId,
                              Name = row.Name,
                              Price = row.Price,
                              Description = row.Description,
                              ImageUrl = row.ImageUrl
                         }
                    }).ToList();
               }

               return cart;
          }


          public async Task ClearCart(string userId)
          {
               using var connection = _dbConnectionFactory.CreateECommerceDbConnection();
               var query = @"
                DELETE FROM [dbo].[CartItems] WHERE [CartId] IN 
                (SELECT [CartId] FROM [dbo].[Carts] WHERE [UserId] = @UserId)";
               await connection.ExecuteAsync(query, new { UserId = userId });
          }

          public async Task UpdateCart(Cart cart)
          {
               using var connection = _dbConnectionFactory.CreateECommerceDbConnection();

               // Ensure the cart exists
               var cartId = cart.CartId;
               if (cartId == 0)
               {
                    var insertCartQuery = @"
          INSERT INTO [dbo].[Carts] ([UserId]) 
          VALUES (@UserId);
          SELECT CAST(SCOPE_IDENTITY() AS INT);
          ";
                    cartId = await connection.QuerySingleAsync<int>(insertCartQuery, new { cart.UserId });
               }

               // Upsert cart items
               var upsertItemsQuery = @"
     MERGE INTO [dbo].[CartItems] AS Target
     USING (VALUES (@CartId, @ProductId, @Quantity)) 
           AS Source ([CartId], [ProductId], [Quantity])
     ON Target.[CartId] = Source.[CartId] AND Target.[ProductId] = Source.[ProductId]
     WHEN MATCHED THEN 
         UPDATE SET [Quantity] = Source.[Quantity]
     WHEN NOT MATCHED THEN
         INSERT ([CartId], [ProductId], [Quantity])
         VALUES (Source.[CartId], Source.[ProductId], Source.[Quantity]);
     ";

               foreach (var item in cart.Items)
               {
                    await connection.ExecuteAsync(upsertItemsQuery, new
                    {
                         CartId = cartId,
                         ProductId = item.ProductId,
                         Quantity = item.Quantity
                    });
               }

               // Remove items no longer in the cart
               var itemIds = cart.Items.Select(i => i.ProductId).ToList();
               var deleteMissingItemsQuery = @"
     DELETE FROM [dbo].[CartItems]
     WHERE [CartId] = @CartId AND [ProductId] NOT IN @ProductIds;
     ";

               await connection.ExecuteAsync(deleteMissingItemsQuery, new { CartId = cartId, ProductIds = itemIds });
          }

          public async Task<List<Cart>> GetAllCarts()
          {
               using var connection = _dbConnectionFactory.CreateECommerceDbConnection();
               var query = @"
                SELECT * FROM [dbo].[Carts];
                SELECT * FROM [dbo].[CartItems];
            ";

               using var multi = await connection.QueryMultipleAsync(query);
               var carts = (await multi.ReadAsync<Cart>()).ToList();
               var cartItems = (await multi.ReadAsync<CartItem>()).ToList();

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

               // Ensure the cart exists, if not, create a new cart
               var cartId = cart.CartId;
               if (cartId == 0)
               {
                    var insertCartQuery = @"
                    INSERT INTO [dbo].[Carts] ([UserId]) VALUES (@UserId);
                    SELECT CAST(SCOPE_IDENTITY() AS INT);
                ";
                    cartId = await connection.QuerySingleAsync<int>(insertCartQuery, new { cart.UserId });
               }

               // Add items to the cart
               var insertItemQuery = @"
                INSERT INTO [dbo].[CartItems] ([CartId], [ProductId], [Quantity]) 
                VALUES (@CartId, @ProductId, @Quantity);
            ";

               foreach (var item in cart.Items)
               {
                    await connection.ExecuteAsync(insertItemQuery, new
                    {
                         CartId = cartId,
                         ProductId = item.ProductId,
                         Quantity = item.Quantity
                    });
               }
          }

          public async Task RemoveItemFromCart(string userId, int productId)
          {
               using var connection = _dbConnectionFactory.CreateECommerceDbConnection();

               // Get the CartId for the user
               var cartQuery = "SELECT [Id] FROM [dbo].[Carts] WHERE [UserId] = @UserId";
               var cartId = await connection.QuerySingleOrDefaultAsync<int>(cartQuery, new { UserId = userId });

               if (cartId == 0)
               {
                    throw new ArgumentException($"No cart found for user with ID: {userId}");
               }

               // Remove the item from the CartItems table
               var removeItemQuery = "DELETE FROM [dbo].[CartItems] WHERE [CartId] = @CartId AND [ProductId] = @ProductId";
               await connection.ExecuteAsync(removeItemQuery, new { CartId = cartId, ProductId = productId });
          }
     }
}
