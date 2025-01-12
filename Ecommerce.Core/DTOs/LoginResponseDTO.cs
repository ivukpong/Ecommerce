using Ecommerce.Core.Models;

namespace Ecommerce.Core.DTOs
{
     public class LoginResponseDTO
     {
          public required string Token { get; set; }
          public required User User { get; set; }
     }

}
