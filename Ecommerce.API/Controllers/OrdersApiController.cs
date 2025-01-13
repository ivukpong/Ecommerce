using Ecommerce.Core.Interfaces.IServices;
using Ecommerce.Core.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Ecommerce.Api.Controllers
{
     [Route("api/[controller]")]
     [ApiController]
     public class OrdersApiController : ControllerBase
     {
          private readonly IOrderService _orderService;
          private readonly ICartService _cartService;

          public OrdersApiController(IOrderService orderService, ICartService cartService)
          {
               _orderService = orderService;
               _cartService = cartService;
          }

          [HttpGet]
          public async Task<IActionResult> GetOrders()
          {
               var userEmail = User.FindFirstValue(ClaimTypes.Email);
               var orders = await _orderService.GetAllOrders(userEmail); // Fetch all orders for the user
               return Ok(orders);
          }

          [HttpGet("{id}")]
          public async Task<IActionResult> GetOrderById(int id)
          {
               var userEmail = User.FindFirstValue(ClaimTypes.Email);
               var order = await _orderService.GetOrder(id, userEmail);
               if (order == null)
               {
                    return NotFound(); // Handle the case where the order is not found
               }
               return Ok(order);
          }

          [HttpPost]
          public async Task<IActionResult> CreateOrder(Order order)
          {
               var userEmail = User.FindFirstValue(ClaimTypes.Email);
               order.UserId = userEmail;
               var createdOrder = await _orderService.CreateOrder(userEmail, order);
               return CreatedAtAction(nameof(GetOrderById), new { id = createdOrder.OrderId }, createdOrder);
          }
     }
}
