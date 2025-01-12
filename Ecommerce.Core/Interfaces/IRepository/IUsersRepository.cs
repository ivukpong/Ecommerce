using Ecommerce.Core.Models;

namespace Ecommerce.Core.Interfaces.IRepository
{
     public interface IUsersRepository
     {
          Task AddNewUser(User user);

          Task<User> GetUserCredentials(string email);

     }
}
