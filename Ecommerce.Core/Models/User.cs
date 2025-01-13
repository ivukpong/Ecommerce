namespace Ecommerce.Core.Models
{
     public class User
     {
          public Guid Id { get; set; } // Unique identifier for the user

          public string Username { get; set; } = null!; // Non-nullable, required field

          public string Email { get; set; } = null!; // Non-nullable, required field

          public byte[] PasswordHash { get; set; } = null!; // Store the hashed password as a byte array

          public string Salt { get; set; } = null!; // Salt for password hashing

          public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Default to current time

          public string Role { get; set; } = "User"; // Default role is User
     }
}
