using Ecommerce.Core.Interfaces.IServices;
using Ecommerce.Core.Interfaces.IRepository;
using Ecommerce.Core.Models;
using Serilog;

namespace Ecommerce.Core.Services
{
     public class ProductsService : IProductsService
     {
          private readonly IProductsRepository _productsRepository;

          public ProductsService(IProductsRepository productsRepository)
          {
               _productsRepository = productsRepository ?? throw new ArgumentNullException(nameof(productsRepository), "Products repository cannot be null.");
          }

          // Check if a product exists by ID
          public async Task<bool> CheckForProduct(int id)
          {
               Log.Information("Checking if product with ID {ProductId} exists.", id);

               try
               {
                    var product = await _productsRepository.GetProductById(id);
                    bool exists = product != null;
                    Log.Information("Product with ID {ProductId} exists: {ProductExists}.", id, exists);
                    return exists;
               }
               catch (Exception ex)
               {
                    Log.Error(ex, "Error occurred while checking for product with ID {ProductId}.", id);
                    throw;
               }
          }

          // Create a new product
          public async Task CreateProduct(Product product)
          {
               if (product == null)
               {
                    Log.Error("Product creation failed: provided product is null.");
                    throw new ArgumentNullException(nameof(product), "Product cannot be null.");
               }

               Log.Information("Creating new product with name {ProductName}.", product.Name);

               try
               {
                    await _productsRepository.CreateProduct(product);
                    Log.Information("Product {ProductName} created successfully.", product.Name);
               }
               catch (Exception ex)
               {
                    Log.Error(ex, "Error occurred while creating product {ProductName}.", product.Name);
                    throw new InvalidOperationException($"Failed to create product {product.Name}.", ex);
               }
          }

          // Delete a product by ID
          public async Task<Product> DeleteProduct(int id)
          {
               Log.Information("Attempting to delete product with ID {ProductId}.", id);

               var product = await _productsRepository.GetProductById(id);
               if (product == null)
               {
                    Log.Warning("Product with ID {ProductId} not found. Cannot delete.", id);
                    throw new KeyNotFoundException($"Product with ID {id} not found.");
               }

               try
               {
                    await _productsRepository.DeleteProduct(id);
                    Log.Information("Product with ID {ProductId} deleted successfully.", id);
                    return product;
               }
               catch (Exception ex)
               {
                    Log.Error(ex, "Error occurred while deleting product with ID {ProductId}.", id);
                    throw new InvalidOperationException($"Failed to delete product with ID {id}.", ex);
               }
          }

          // Get all products
          public async Task<List<Product>> GetAllProducts()
          {
               Log.Information("Fetching all products.");

               try
               {
                    var products = await _productsRepository.GetAllProducts();
                    Log.Information("Fetched {ProductCount} products.", products.Count);
                    return products;
               }
               catch (Exception ex)
               {
                    Log.Error(ex, "Error occurred while fetching all products.");
                    throw new InvalidOperationException("Failed to fetch all products.", ex);
               }
          }

          // Get a product by ID
          public async Task<Product> GetProductById(int id)
          {
               Log.Information("Fetching product with ID {ProductId}.", id);

               var product = await _productsRepository.GetProductById(id);
               if (product == null)
               {
                    Log.Warning("Product with ID {ProductId} not found.", id);
                    throw new KeyNotFoundException($"Product with ID {id} not found.");
               }

               Log.Information("Fetched product with ID {ProductId}: {ProductName}.", id, product.Name);
               return product;
          }

          // Update an existing product
          public async Task UpdateProduct(Product product)
          {
               if (product == null)
               {
                    Log.Error("Product update failed: provided product is null.");
                    throw new ArgumentNullException(nameof(product), "Product cannot be null.");
               }

               Log.Information("Updating product with ID {ProductId}.", product.ProductId);

               try
               {
                    var existingProduct = await _productsRepository.GetProductById(product.ProductId);
                    if (existingProduct == null)
                    {
                         Log.Warning("Product with ID {ProductId} not found. Cannot update.", product.ProductId);
                         throw new KeyNotFoundException($"Product with ID {product.ProductId} not found.");
                    }

                    // Update product properties
                    existingProduct.Name = product.Name;
                    existingProduct.Description = product.Description;
                    existingProduct.Price = product.Price;
                    existingProduct.ImageUrl = product.ImageUrl;
                    existingProduct.IsFeatured = product.IsFeatured;

                    await _productsRepository.UpdateProduct(existingProduct);
                    Log.Information("Product with ID {ProductId} updated successfully.", product.ProductId);
               }
               catch (Exception ex)
               {
                    Log.Error(ex, "Error occurred while updating product with ID {ProductId}.", product.ProductId);
                    throw new InvalidOperationException($"Failed to update product with ID {product.ProductId}.", ex);
               }
          }
     }
}
