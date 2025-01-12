using Dapper;
using Ecommerce.Core.Interfaces.IFactories;
using Ecommerce.Core.Interfaces.IRepository;
using Ecommerce.Core.Models;
using System.Data;

public class OrdersRepository : IOrdersRepository
{
     private readonly IDbConnectionFactory _dbConnectionFactory;

     public OrdersRepository(IDbConnectionFactory dbConnectionFactory)
     {
          _dbConnectionFactory = dbConnectionFactory;
     }

     // Create a new order
     public async Task CreateOrder(Order order)
     {
          using (var connection = _dbConnectionFactory.CreateECommerceDbConnection())
          {
               // Insert the order and retrieve the generated ID
               var orderQuery = @"
                INSERT INTO [dbo].[Orders] 
                ([UserId], [OrderDate], [Street], [City], [PostalCode], [Country]) 
                VALUES 
                (@UserId, @OrderDate, @Street, @City, @PostalCode, @Country);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

               var orderParameters = new
               {
                    order.UserId,
                    order.OrderDate,
                    order.Street,
                    order.City,
                    order.PostalCode,
                    order.Country
               };

               order.OrderId = await connection.ExecuteScalarAsync<int>(orderQuery, orderParameters);

               if (order.Items.Any())
               {
                    // Insert order items
                    var orderItemQuery = @"
                    INSERT INTO [dbo].[OrderItems] 
                    ([OrderId], [ProductId], [Quantity], [Price]) 
                    VALUES 
                    (@OrderId, @ProductId, @Quantity, @Price);";

                    foreach (var item in order.Items)
                    {
                         var itemParameters = new
                         {
                              OrderId = order.OrderId,
                              item.ProductId,
                              item.Quantity,
                              item.Price
                         };

                         await connection.ExecuteAsync(orderItemQuery, itemParameters);
                    }
               }
          }
     }

     // Delete an order and its items
     public async Task<Order> DeleteOrder(int id)
     {
          using (var connection = _dbConnectionFactory.CreateECommerceDbConnection())
          {
               // Retrieve the order to confirm its existence
               var orderQuery = "SELECT * FROM [dbo].[Orders] WHERE [OrderId] = @OrderId;";
               var orderParameters = new { OrderId = id };

               var order = await connection.QuerySingleOrDefaultAsync<Order>(orderQuery, orderParameters);

               if (order != null)
               {
                    // Delete associated order items
                    var deleteItemsQuery = "DELETE FROM [dbo].[OrderItems] WHERE [OrderId] = @OrderId;";
                    await connection.ExecuteAsync(deleteItemsQuery, orderParameters);

                    // Delete the order itself
                    var deleteOrderQuery = "DELETE FROM [dbo].[Orders] WHERE [OrderId] = @OrderId;";
                    await connection.ExecuteAsync(deleteOrderQuery, orderParameters);
               }

               return order;
          }
     }

     // Get all orders with their items
     public async Task<List<Order>> GetAllOrders()
     {
          using (var connection = _dbConnectionFactory.CreateECommerceDbConnection())
          {
               var query = @"
                SELECT * FROM [dbo].[Orders];
                SELECT * FROM [dbo].[OrderItems];";

               using (var multi = await connection.QueryMultipleAsync(query))
               {
                    var orders = (await multi.ReadAsync<Order>()).ToList();
                    var orderItems = await multi.ReadAsync<OrderItem>();

                    foreach (var order in orders)
                    {
                         order.Items = orderItems
                             .Where(item => item.OrderId == order.OrderId)
                             .ToList();
                    }

                    return orders;
               }
          }
     }

     // Get an order by ID and UserId
     public async Task<Order> GetOrder(int id, string userId)
     {
          using (var connection = _dbConnectionFactory.CreateECommerceDbConnection())
          {
               var query = @"
                SELECT * FROM [dbo].[Orders] WHERE [OrderId] = @OrderId AND [UserId] = @UserId;
                SELECT * FROM [dbo].[OrderItems] WHERE [OrderId] = @OrderId;";

               var parameters = new { OrderId = id, UserId = userId };

               using (var multi = await connection.QueryMultipleAsync(query, parameters))
               {
                    var order = await multi.ReadSingleOrDefaultAsync<Order>();

                    if (order != null)
                    {
                         var orderItems = await multi.ReadAsync<OrderItem>();
                         order.Items = orderItems
                             .Where(item => item.OrderId == order.OrderId)
                             .ToList();
                    }

                    return order;
               }
          }
     }

     // Update an order and its items
     public async Task UpdateOrder(Order order)
     {
          using (var connection = _dbConnectionFactory.CreateECommerceDbConnection())
          {
               // Update the order details
               var updateOrderQuery = @"
                UPDATE [dbo].[Orders]
                SET [Street] = @Street,
                    [City] = @City,
                    [PostalCode] = @PostalCode,
                    [Country] = @Country
                WHERE [OrderId] = @OrderId;";

               var updateOrderParameters = new
               {
                    order.OrderId,
                    order.Street,
                    order.City,
                    order.PostalCode,
                    order.Country
               };

               await connection.ExecuteAsync(updateOrderQuery, updateOrderParameters);

               // Delete existing order items
               var deleteItemsQuery = "DELETE FROM [dbo].[OrderItems] WHERE [OrderId] = @OrderId;";
               await connection.ExecuteAsync(deleteItemsQuery, new { OrderId = order.OrderId });

               // Insert updated order items
               if (order.Items.Any())
               {
                    var insertItemsQuery = @"
                    INSERT INTO [dbo].[OrderItems] 
                    ([OrderId], [ProductId], [Quantity], [Price]) 
                    VALUES 
                    (@OrderId, @ProductId, @Quantity, @Price);";

                    foreach (var item in order.Items)
                    {
                         var itemParameters = new
                         {
                              OrderId = order.OrderId,
                              item.ProductId,
                              item.Quantity,
                              item.Price
                         };

                         await connection.ExecuteAsync(insertItemsQuery, itemParameters);
                    }
               }
          }
     }
}
