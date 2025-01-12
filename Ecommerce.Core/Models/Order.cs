namespace Ecommerce.Core.Models
{
     public class Order
     {
          public int OrderId { get; set; }
          public string UserId { get; set; } = null!;
          public DateTime OrderDate { get; set; } = DateTime.UtcNow;

          // Shipping address fields directly on the Order table
          public string Street { get; set; } = null!;
          public string City { get; set; } = null!;
          public string PostalCode { get; set; } = null!;
          public string Country { get; set; } = null!;

          public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
     }

     public class OrderItem
     {
          public int OrderItemId { get; set; }
          public int OrderId { get; set; }
          public Order Order { get; set; } = null!;
          public int ProductId { get; set; }
          public Product Product { get; set; } = null!;
          public decimal Quantity { get; set; }
          public decimal Price { get; set; } // Price of the product
     }
}
