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
               _productsRepository = productsRepository;
          }

          // Method to check if a product exists by ID
          public async Task<bool> CheckForProduct(int id)
          {
               Log.Information("Checking if product with ID {ProductId} exists.", id);
               var product = await _productsRepository.GetProductById(id);
               bool exists = product != null;
               Log.Information("Product with ID {ProductId} exists: {ProductExists}.", id, exists);
               return exists;
          }

          // Method to create a new product
          public async Task CreateProduct(Product product)
          {
               Log.Information("Creating new product with name {ProductName}.", product.Name);
               if (product == null)
               {
                    Log.Error("Product creation failed because the provided product is null.");
                    throw new ArgumentNullException(nameof(product), "Product cannot be null");
               }

               try
               {
                    await _productsRepository.CreateProduct(product);
                    Log.Information("Product {ProductName} created successfully.", product.Name);
               }
               catch (Exception ex)
               {
                    Log.Error(ex, "Error occurred while creating product {ProductName}.", product.Name);
                    throw;
               }
          }

          // Method to delete a product by its ID
          public async Task<Product> DeleteProduct(int id)
          {
               Log.Information("Attempting to delete product with ID {ProductId}.", id);
               var product = await _productsRepository.GetProductById(id);
               if (product == null)
               {
                    Log.Warning("Product with ID {ProductId} not found. Cannot delete.", id);
                    throw new Exception("Product not found");
               }

               await _productsRepository.DeleteProduct(id);
               Log.Information("Product with ID {ProductId} deleted successfully.", id);
               return product; // Returning the deleted product
          }

          // Method to get all products
          public async Task<List<Product>> GetAllProducts()
          {
               Log.Information("Fetching all products.");
               var products = await _productsRepository.GetAllProducts();
               Log.Information("Fetched {ProductCount} products.", products.Count);
               return products;
          }

          // Method to get a product by its ID
          public async Task<Product> GetProductById(int id)
          {
               Log.Information("Fetching product with ID {ProductId}.", id);
               var product = await _productsRepository.GetProductById(id);
               if (product == null)
               {
                    Log.Warning("Product with ID {ProductId} not found.", id);
                    throw new Exception("Product not found");
               }

               Log.Information("Fetched product with ID {ProductId}: {ProductName}.", id, product.Name);
               return product;
          }

          // Method to update an existing product
          public async Task UpdateProduct(Product product)
          {
               Log.Information("Updating product with ID {ProductId}.", product.ProductId);
               if (product == null)
               {
                    Log.Error("Product update failed because the provided product is null.");
                    throw new ArgumentNullException(nameof(product), "Product cannot be null");
               }

               try
               {
                    var existingProduct = await _productsRepository.GetProductById(product.ProductId);
                    if (existingProduct == null)
                    {
                         Log.Warning("Product with ID {ProductId} not found. Cannot update.", product.ProductId);
                         throw new Exception("Product not found");
                    }

                    // Update product details
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
                    throw;
               }
          }
     }
}
