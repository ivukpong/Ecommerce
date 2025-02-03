using Dapper;
using Ecommerce.Core.Interfaces.IFactories;
using Ecommerce.Core.Interfaces.IRepository;
using Ecommerce.Core.Models;
using Microsoft.Data.SqlClient;
using Serilog;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

public class OrdersRepository : IOrdersRepository
{
     private readonly IDbConnectionFactory _dbConnectionFactory;

     public OrdersRepository(IDbConnectionFactory dbConnectionFactory)
     {
          _dbConnectionFactory = dbConnectionFactory;
          // Set up Serilog for logging.
          Log.Logger = new LoggerConfiguration()
              .WriteTo.Console() // You can add more sinks, such as file, Seq, etc.
              .CreateLogger();
     }

     // Create a new order
     public async Task CreateOrder(Order order)
     {
          try
          {
               using (var connection = _dbConnectionFactory.CreateECommerceDbConnection())
               {
                    Log.Information("Creating order for user {UserId} on {OrderDate}", order.UserId, order.OrderDate);

                    // Insert the order and retrieve the generated ID
                    DynamicParameters orderParameters = new();
                    orderParameters.Add("UserId", order.UserId);
                    orderParameters.Add("OrderDate", order.OrderDate);
                    orderParameters.Add("Street", order.Street);
                    orderParameters.Add("City", order.City);
                    orderParameters.Add("PostalCode", order.PostalCode);
                    orderParameters.Add("Country", order.Country);

                    orderParameters.Add("OrderId", dbType: DbType.Int32, direction: ParameterDirection.Output);

                    await connection.ExecuteAsync("proc_CreateOrder", orderParameters, commandType: CommandType.StoredProcedure);

                    order.OrderId = orderParameters.Get<int>("@OrderId");

                    Log.Information("Created order with ID {OrderId}", order.OrderId);

                    if (order.Items.Any())
                    {
                         foreach (var item in order.Items)
                         {
                              Log.Information("Adding item {ProductId} with quantity {Quantity}", item.ProductId, item.Quantity);

                              DynamicParameters orderItemParameters = new();
                              orderItemParameters.Add("OrderId", order.OrderId);
                              orderItemParameters.Add("ProductId", item.ProductId);
                              orderItemParameters.Add("Quantity", item.Quantity);
                              orderItemParameters.Add("Price", item.Price);

                              await connection.ExecuteAsync("proc_CreateOrderItem", orderItemParameters, commandType: CommandType.StoredProcedure);
                         }
                    }
               }
          }
          catch (Exception ex)
          {
               Log.Error(ex, "Error occurred while creating order {OrderId}", order.OrderId);
               throw;
          }
     }

     // Delete an order and its items
     public async Task DeleteOrder(int id)
     {
          try
          {
               using (var connection = _dbConnectionFactory.CreateECommerceDbConnection())
               {
                    Log.Information("Deleting order with ID {OrderId}", id);

                    DynamicParameters orderParameters = new();
                    orderParameters.Add("OrderId", id);

                    var order = await connection.QuerySingleOrDefaultAsync<Order>("proc_GetOrderById", orderParameters, commandType: CommandType.StoredProcedure);

                    if (order != null)
                    {
                         await connection.ExecuteAsync("proc_DeleteOrderItem", orderParameters, commandType: CommandType.StoredProcedure);
                         await connection.ExecuteAsync("proc_DeleteOrder", orderParameters, commandType: CommandType.StoredProcedure);
                         Log.Information("Order with ID {OrderId} deleted successfully", id);
                    }
                    else
                    {
                         Log.Warning("No order found with ID {OrderId}", id);
                    }
               }
          }
          catch (Exception ex)
          {
               Log.Error(ex, "Error occurred while deleting order {OrderId}", id);
               throw;
          }
     }

     // Get all orders with their items
     public async Task<List<Order>> GetAllOrders(string email)
     {
          try
          {
               using (var connection = _dbConnectionFactory.CreateECommerceDbConnection())
               {
                    Log.Information("Fetching orders for user with email {Email}", email);

                    DynamicParameters orderItemParameters = new();
                    orderItemParameters.Add("UserId", email);

                    // Execute the stored procedure
                    using (var multi = await connection.QueryMultipleAsync(
                        "proc_GetOrdersByUserId",
                        orderItemParameters,
                        commandType: CommandType.StoredProcedure))
                    {
                         // Read the orders
                         var orders = (await multi.ReadAsync<Order>()).ToList();

                         // Read the order items
                         var orderItems = (await multi.ReadAsync<OrderItem>()).ToList();

                         // Map order items to their respective orders
                         foreach (var order in orders)
                         {
                              order.Items = orderItems.Where(item => item.OrderId == order.OrderId).ToList();
                         }

                         Log.Information("Fetched {OrderCount} orders for user {Email}", orders.Count, email);

                         return orders;
                    }
               }
          }
          catch (Exception ex)
          {
               Log.Error(ex, "Error occurred while fetching orders for user {Email}", email);
               throw;
          }
     }

     // Get an order by ID and UserId
     public async Task<Order?> GetOrder(int id, string email)
     {
          try
          {
               using (var connection = _dbConnectionFactory.CreateECommerceDbConnection())
               {
                    Log.Information("Fetching order with ID {OrderId} for user {Email}", id, email);

                    DynamicParameters orderItemParameters = new();
                    orderItemParameters.Add("UserId", email);
                    orderItemParameters.Add("orderId", id);

                    using (var multi = await connection.QueryMultipleAsync("proc_GetOrderByUserId", orderItemParameters))
                    {
                         var order = await multi.ReadSingleOrDefaultAsync<Order>();

                         if (order != null)
                         {
                              var orderItems = await multi.ReadAsync<OrderItem>();
                              order.Items = orderItems
                                  .Where(item => item.OrderId == order.OrderId)
                                  .ToList();
                         }

                         Log.Information("Fetched order with ID {OrderId} for user {Email}", id, email);

                         return order;
                    }
               }
          }
          catch (Exception ex)
          {
               Log.Error(ex, "Error occurred while fetching order with ID {OrderId} for user {Email}", id, email);
               throw;
          }
     }

     // Update an order and its items
     public async Task UpdateOrder(Order order)
     {
          try
          {
               using (var connection = _dbConnectionFactory.CreateECommerceDbConnection())
               {
                    Log.Information("Updating order with ID {OrderId}", order.OrderId);

                    // Update the order details using a stored procedure
                    var updateOrderParameters = new DynamicParameters();
                    updateOrderParameters.Add("OrderId", order.OrderId);
                    updateOrderParameters.Add("Street", order.Street);
                    updateOrderParameters.Add("City", order.City);
                    updateOrderParameters.Add("PostalCode", order.PostalCode);
                    updateOrderParameters.Add("Country", order.Country);

                    await connection.ExecuteAsync(
                        "proc_UpdateOrder", // Replace with your actual procedure name
                        updateOrderParameters,
                        commandType: CommandType.StoredProcedure
                    );

                    // Delete existing order items using a stored procedure
                    var deleteItemsParameters = new DynamicParameters();
                    deleteItemsParameters.Add("OrderId", order.OrderId);

                    await connection.ExecuteAsync(
                        "proc_DeleteOrderItems", // Replace with your actual procedure name
                        deleteItemsParameters,
                        commandType: CommandType.StoredProcedure
                    );

                    // Insert updated order items using a stored procedure
                    if (order.Items.Any())
                    {
                         foreach (var item in order.Items)
                         {
                              Log.Information("Adding updated item {ProductId} with quantity {Quantity}", item.ProductId, item.Quantity);

                              var insertItemParameters = new DynamicParameters();
                              insertItemParameters.Add("OrderId", order.OrderId);
                              insertItemParameters.Add("ProductId", item.ProductId);
                              insertItemParameters.Add("Quantity", item.Quantity);
                              insertItemParameters.Add("Price", item.Price);

                              await connection.ExecuteAsync(
                                  "proc_InsertOrderItem", // Replace with your actual procedure name
                                  insertItemParameters,
                                  commandType: CommandType.StoredProcedure
                              );
                         }
                    }

                    Log.Information("Order with ID {OrderId} updated successfully", order.OrderId);
               }
          }
          catch (Exception ex)
          {
               Log.Error(ex, "Error occurred while updating order {OrderId}", order.OrderId);
               throw;
          }
     }
}
