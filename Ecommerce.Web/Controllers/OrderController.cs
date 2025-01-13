using Ecommerce.Core.Interfaces.IServices;
using Ecommerce.Core.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;

public class OrderController : Controller
{
     private readonly IOrderService _orderService;
     private readonly ICartService _cartService;

     public OrderController(IOrderService orderService, ICartService cartService)
     {
          _orderService = orderService;
          _cartService = cartService;
     }

     // GET: /Order/Index
     public async Task<IActionResult> Index()
     {
          var userEmail = User.FindFirstValue(ClaimTypes.Email);
          if (string.IsNullOrEmpty(userEmail))
          {
               return RedirectToAction("Login", "Account");
          }

          var orders = await _orderService.GetAllOrders(userEmail); // Fetch all orders for the user
          return View(orders);
     }

     // POST: /Order/Checkout (submit order)
     [HttpPost]
     public async Task<IActionResult> Details(Order order)
     {
          var userEmail = User.FindFirstValue(ClaimTypes.Email);
          if (string.IsNullOrEmpty(userEmail))
          {
               return RedirectToAction("Login", "Account");
          }

          order.UserId = userEmail;

          // Save the order with associated shipping details
          var createdOrder = await _orderService.CreateOrder(userEmail, order);

          // Redirect to the order details page or confirmation page
          return RedirectToAction("Details", new { id = createdOrder.OrderId });
     }

     // GET: /Order/Checkout
     public async Task<IActionResult> Checkout()
     {
          var userEmail = User.FindFirstValue(ClaimTypes.Email);
          if (string.IsNullOrEmpty(userEmail))
          {
               return RedirectToAction("Login", "Account");
          }

          var cart = await _cartService.GetCart(userEmail);
          if (cart?.Items == null || !cart.Items.Any())
          {
               return RedirectToAction("Index", "Cart"); // Redirect to the cart if it’s empty
          }

          var order = new Order
          {
               UserId = userEmail,
               Street = string.Empty,
               City = string.Empty,
               PostalCode = string.Empty,
               Country = string.Empty,
               Items = cart.Items.Select(item => new OrderItem
               {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Product.Price,
                    Product = item.Product
               }).ToList()
          };

          return View(order);
     }

     // GET: /Order/Details
     public async Task<IActionResult> Details(int id)
     {
          var userEmail = User.FindFirstValue(ClaimTypes.Email);
          if (string.IsNullOrEmpty(userEmail))
          {
               return RedirectToAction("Login", "Account");
          }

          var order = await _orderService.GetOrder(id, userEmail);
          if (order == null)
          {
               return NotFound(); // Handle the case where the order is not found
          }

          return View(order);
     }
}
