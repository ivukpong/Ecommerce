using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Core.Models
{
     public class RegisterViewModel
     {
          [Required]
          public string Email { get; set; } = null!;
          [Required]
          public string Username { get; set; } = null!;
          [Required]
          public string Password { get; set; } = null!;
          [Required]
          public string ConfirmPassword { get; set; } = null!;
     }
}
