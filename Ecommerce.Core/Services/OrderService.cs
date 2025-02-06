using Ecommerce.Core.Interfaces.IRepository;
using Ecommerce.Core.Interfaces.IServices;
using Ecommerce.Core.Models;
using Serilog;
using System.Linq;
using System.Threading.Tasks;

public class OrderService : IOrderService
{
     private readonly IOrdersRepository _ordersRepository;
     private readonly ICartService _cartService;
     private readonly IProductsService _productsService;

     public OrderService(
         IOrdersRepository ordersRepository,
         ICartService cartService,
         IProductsService productsService)
     {
          _ordersRepository = ordersRepository;
          _cartService = cartService;
          _productsService = productsService;
     }

     // Create a new order from the cart
     public async Task<Order> CreateOrder(string userEmail, Order model)
     {
          Log.Information("Creating order for user {UserEmail}.", userEmail);

          if (model == null)
          {
               Log.Error("Order creation failed because the provided order model is null.");
               throw new ArgumentNullException(nameof(model), "Order model cannot be null.");
          }

          var cart = await _cartService.GetCart(userEmail);
          if (cart == null || !cart.Items.Any())
          {
               Log.Warning("Cart is empty for user {UserEmail}.", userEmail);
               throw new InvalidOperationException("Cart is empty.");
          }

          var order = new Order
          {
               UserId = userEmail,
               OrderDate = DateTime.UtcNow,
               Street = model.Street,
               City = model.City,
               PostalCode = model.PostalCode,
               Country = model.Country,
               Items = cart.Items.Select(i => new OrderItem
               {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    Price = i.Product.Price
               }).ToList()
          };

          foreach (var item in order.Items)
          {
               var product = await _productsService.GetProductById(item.ProductId);
               if (product == null)
               {
                    Log.Error("Product with ID {ProductId} not found for order creation.", item.ProductId);
                    throw new ArgumentException($"Product with ID {item.ProductId} not found.");
               }

               item.Product = product;
          }

          try
          {
               await _ordersRepository.CreateOrder(order);
               Log.Information("Order created successfully for user {UserEmail} with {ItemCount} items.", userEmail, order.Items.Count);

               await _cartService.ClearCart(userEmail);
               Log.Information("Cart cleared for user {UserEmail} after order creation.", userEmail);
          }
          catch (Exception ex)
          {
               Log.Error(ex, "Error occurred while creating order for user {UserEmail}.", userEmail);
               throw;
          }

          return order;
     }

     // Get all orders for a user
     public async Task<List<Order>> GetAllOrders(string email)
     {
          Log.Information("Fetching all orders for user {UserEmail}.", email);

          var orders = await _ordersRepository.GetAllOrders(email);
          Log.Information("Fetched {OrderCount} orders for user {UserEmail}.", orders.Count, email);

          return orders;
     }

     // Get an order by ID and UserId
     public async Task<Order> GetOrder(int id, string userId)
     {
          Log.Information("Fetching order with ID {OrderId} for user {UserId}.", id, userId);

          var order = await _ordersRepository.GetOrder(id, userId);
          if (order == null)
          {
               Log.Warning("Order with ID {OrderId} not found for user {UserId}.", id, userId);
               throw new KeyNotFoundException($"Order with ID {id} not found for user {userId}.");
          }

          foreach (var item in order.Items)
          {
               var product = await _productsService.GetProductById(item.ProductId);
               if (product != null)
               {
                    item.Product = product;
                    Log.Information("Added product {ProductName} to order with ID {OrderId}.", product.Name, id);
               }
          }

          Log.Information("Order with ID {OrderId} fetched successfully for user {UserId}.", id, userId);
          return order;
     }

     // Update an order
     public async Task UpdateOrder(Order order)
     {
          Log.Information("Updating order with ID {OrderId} for user {UserId}.", order.OrderId, order.UserId);

          if (order == null)
          {
               Log.Error("Order update failed because the provided order is null.");
               throw new ArgumentNullException(nameof(order), "Order cannot be null.");
          }

          var existingOrder = await _ordersRepository.GetOrder(order.OrderId, order.UserId);
          if (existingOrder == null)
          {
               Log.Warning("Order with ID {OrderId} not found for user {UserId}. Cannot update.", order.OrderId, order.UserId);
               throw new KeyNotFoundException($"Order with ID {order.OrderId} not found.");
          }

          try
          {
               await _ordersRepository.UpdateOrder(order);
               Log.Information("Order with ID {OrderId} updated successfully for user {UserId}.", order.OrderId, order.UserId);
          }
          catch (Exception ex)
          {
               Log.Error(ex, "Error occurred while updating order with ID {OrderId} for user {UserId}.", order.OrderId, order.UserId);
               throw;
          }
     }

     // Delete an order by ID
     public async Task DeleteOrder(int id)
     {
          Log.Information("Attempting to delete order with ID {OrderId}.", id);

          var order = await _ordersRepository.GetOrder(id, string.Empty);
          if (order == null)
          {
               Log.Warning("Order with ID {OrderId} not found. Cannot delete.", id);
               throw new KeyNotFoundException($"Order with ID {id} not found.");
          }

          try
          {
               await _ordersRepository.DeleteOrder(id);
               Log.Information("Order with ID {OrderId} deleted successfully.", id);
          }
          catch (Exception ex)
          {
               Log.Error(ex, "Error occurred while deleting order with ID {OrderId}.", id);
               throw;
          }
     }
}
