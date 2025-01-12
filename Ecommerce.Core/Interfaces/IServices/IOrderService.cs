using Ecommerce.Core.Models;

namespace Ecommerce.Core.Interfaces.IServices
{
     public interface IOrderService
     {
          Task<List<Order>> GetAllOrders();
          Task<Order> GetOrder(int id, string userId);
          Task<Order> CreateOrder(string email, Order order);
          Task UpdateOrder(Order order);
          Task<Order> DeleteOrder(int id);
     }

}
