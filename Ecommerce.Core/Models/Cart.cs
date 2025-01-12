namespace Ecommerce.Core.Models
{
     public class Cart
     {
          public int CartId { get; set; }

          // Foreign key to the User
          public string? UserId { get; set; }

          // Navigation property for items in the cart
          public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
     }

     public class CartItem
     {
          public int CartItemId { get; set; }

          // Foreign key to the Cart
          public int CartId { get; set; }
          public Cart? Cart { get; set; }

          // Foreign key to the Product
          public int ProductId { get; set; }
          public Product? Product { get; set; }

          public decimal Quantity { get; set; } // Quantity of the product in the cart
     }
}
