using Ecommerce.Core.Interfaces.IRepository;
using Ecommerce.Core.Interfaces.IServices;
using Ecommerce.Core.Models;
using System.Reflection;

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
          var cart = await _cartService.GetCart(userEmail);
          if (cart == null || !cart.Items.Any())
          {
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

          // Validate each product in the order
          foreach (var item in order.Items)
          {
               var product = await _productsService.GetProductById(item.ProductId);
               if (product == null)
               {
                    throw new ArgumentException($"Product with ID {item.ProductId} not found.");
               }
               item.Product = product;
          }

          // Create the order
          await _ordersRepository.CreateOrder(order);

          // Clear the cart after the order is created
          await _cartService.ClearCart(userEmail);

          return order;
     }

     // Get all orders
     public async Task<List<Order>> GetAllOrders()
     {
          return await _ordersRepository.GetAllOrders();
     }

     // Get an order by ID and UserId
     public async Task<Order> GetOrder(int id, string userId)
     {
          var order = await _ordersRepository.GetOrder(id, userId);
          if (order == null)
          {
               throw new KeyNotFoundException($"Order with ID {id} not found for user {userId}.");
          }
          foreach (var item in order.Items.ToList()) 
          {
               var product = await _productsService.GetProductById(item.ProductId);
               item.Product = product;
          }
          return order;
     }

     // Update an order
     public async Task UpdateOrder(Order order)
     {
          var existingOrder = await _ordersRepository.GetOrder(order.OrderId, order.UserId);
          if (existingOrder == null)
          {
               throw new KeyNotFoundException($"Order with ID {order.OrderId} not found.");
          }

          await _ordersRepository.UpdateOrder(order);
     }

     // Delete an order by ID
     public async Task<Order> DeleteOrder(int id)
     {
          var order = await _ordersRepository.GetOrder(id, string.Empty);
          if (order == null)
          {
               throw new KeyNotFoundException($"Order with ID {id} not found.");
          }

          return await _ordersRepository.DeleteOrder(id);
     }
}
