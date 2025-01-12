using Ecommerce.Core.Models;

namespace Ecommerce.Core.Interfaces.IRepository
{
     public interface IOrdersRepository
     {
          Task<List<Order>> GetAllOrders();

          Task<Order> GetOrder(int id, string userId);

          Task CreateOrder(Order order);

          Task UpdateOrder(Order order);

          Task<Order> DeleteOrder(int id);
     }

}
