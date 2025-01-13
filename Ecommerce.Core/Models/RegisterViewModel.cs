using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Core.Models
{
     public class RegisterViewModel
     {
          [Required]
          public string? Email { get; set; } 

          public string? Username { get; set; } 

          public string? Password { get; set; }

          public string? ConfirmPassword { get; set; } 
     }
}
