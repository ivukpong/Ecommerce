using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Core.Models
{
     public class Product
     {
          public int ProductId { get; set; }

          public string Name { get; set; } = null!;

          public string? Description { get; set; }

        
          public decimal Price { get; set; }

          public string? ImageUrl { get; set; }

          public bool IsFeatured { get; set; }
     }
}
