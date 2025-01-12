using Ecommerce.Core.Interfaces.IServices;
using Ecommerce.Core.Interfaces.IRepository;
using Ecommerce.Core.Models;

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
               var product = await _productsRepository.GetProductById(id);
               return product != null;
          }

          // Method to create a new product
          public async Task CreateProduct(Product product)
          {
               // Validate the product using FluentValidation or other validation methods
               if (product == null)
               {
                    throw new ArgumentNullException(nameof(product), "Product cannot be null");
               }

               await _productsRepository.CreateProduct(product);
          }

          // Method to delete a product by its ID
          public async Task<Product> DeleteProduct(int id)
          {
               var product = await _productsRepository.GetProductById(id);
               if (product == null)
               {
                    throw new Exception("Product not found");
               }

               await _productsRepository.DeleteProduct(id);
               return product; // Returning the deleted product
          }

          // Method to get all products
          public async Task<List<Product>> GetAllProducts()
          {
               return await _productsRepository.GetAllProducts();
          }

          // Method to get a product by its ID
          public async Task<Product> GetProductById(int id)
          {
               var product = await _productsRepository.GetProductById(id);
               if (product == null)
               {
                    throw new Exception("Product not found");
               }

               return product;
          }

          // Method to update an existing product
          public async Task UpdateProduct(Product product)
          {
               if (product == null)
               {
                    throw new ArgumentNullException(nameof(product), "Product cannot be null");
               }

               var existingProduct = await _productsRepository.GetProductById(product.ProductId);
               if (existingProduct == null)
               {
                    throw new Exception("Product not found");
               }

               // Update product details
               existingProduct.Name = product.Name;
               existingProduct.Description = product.Description;
               existingProduct.Price = product.Price;
               existingProduct.ImageUrl = product.ImageUrl;
               existingProduct.IsFeatured = product.IsFeatured;

               await _productsRepository.UpdateProduct(existingProduct);
          }
     }
}
