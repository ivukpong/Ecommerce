using Ecommerce.Core.DTOs;
using Ecommerce.Core.Models;

namespace Ecommerce.Core.Interfaces.IServices
{
     public interface IUserService
     {
          Task Register(RegisterViewModel model);

          Task<LoginResponseDTO> Login(LoginViewModel model);

          Task<User> GetUser(string email);
     }
}

