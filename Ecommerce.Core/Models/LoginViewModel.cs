using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Core.Models
{
     public class LoginViewModel
     {
          public string Email { get; set; } = null!; 
         
          public string Password { get; set; } = null!; 

          public bool RememberMe { get; set; } = false; 
     }
}
