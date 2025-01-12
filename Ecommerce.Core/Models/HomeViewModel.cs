namespace Ecommerce.Core.Models
{
     public class HomeViewModel
     {
          public IEnumerable<Product> FeaturedProducts { get; set; } = new List<Product>();
          public string Username { get; set; } = string.Empty;
     }
}
