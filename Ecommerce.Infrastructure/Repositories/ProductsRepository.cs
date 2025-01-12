using Ecommerce.Core.Interfaces.IFactories;
using Ecommerce.Core.Interfaces.IRepository;
using Ecommerce.Core.Models;
using Dapper;

namespace Ecommerce.Infrastructure.Repositories
     {
     public class ProductsRepository : IProductsRepository
          {
               private readonly IDbConnectionFactory _dbConnectionFactory;

               public ProductsRepository(IDbConnectionFactory dbConnectionFactory)
               {
                    _dbConnectionFactory = dbConnectionFactory;
               }

               // Check if a product exists by its ID
               public async Task<bool> CheckForProduct(int id)
               {
                    using (var connection = _dbConnectionFactory.CreateECommerceDbConnection())
                    {
                         var query = "SELECT COUNT(1) FROM [dbo].[Products] WHERE [ProductId] = @ProductId";
                         var parameters = new { ProductId = id };
                         var count = await connection.ExecuteScalarAsync<int>(query, parameters);
                         return count > 0;
                    }
               }

               // Create a new product
               public async Task CreateProduct(Product product)
               {
                    using (var connection = _dbConnectionFactory.CreateECommerceDbConnection())
                    {
                         var query = @"
                    INSERT INTO [dbo].[Products] 
                    ([Name], [Description], [Price], [ImageUrl], [IsFeatured]) 
                    VALUES 
                    (@Name, @Description, @Price, @ImageUrl, @IsFeatured)";

                         var parameters = new
                         {
                              product.Name,
                              product.Description,
                              product.Price,
                              product.ImageUrl,
                              product.IsFeatured
                         };

                         await connection.ExecuteAsync(query, parameters);
                    }
               }

               // Delete a product by its ID
               public async Task<Product> DeleteProduct(int id)
               {
                    using (var connection = _dbConnectionFactory.CreateECommerceDbConnection())
                    {
                         var query = "SELECT * FROM [dbo].[Products] WHERE [ProductId] = @ProductId";
                         var parameters = new { ProductId = id };

                         var product = await connection.QuerySingleOrDefaultAsync<Product>(query, parameters);

                         if (product == null)
                         {
                              throw new KeyNotFoundException($"Product with id {id} not found");
                         }

                         var deleteQuery = "DELETE FROM [dbo].[Products] WHERE [ProductId] = @ProductId";
                         await connection.ExecuteAsync(deleteQuery, parameters);

                         return product;
                    }
               }

               // Get all products
               public async Task<List<Product>> GetAllProducts()
               {
                    using (var connection = _dbConnectionFactory.CreateECommerceDbConnection())
                    {
                         var query = "SELECT * FROM [dbo].[Products]";
                         var products = await connection.QueryAsync<Product>(query);
                         return products.ToList();
                    }
               }

               // Get a product by its ID
               public async Task<Product> GetProductById(int id)
               {
                    using (var connection = _dbConnectionFactory.CreateECommerceDbConnection())
                    {
                         var query = "SELECT * FROM [dbo].[Products] WHERE [ProductId] = @ProductId";
                         var parameters = new { ProductId = id };
                         var product = await connection.QuerySingleOrDefaultAsync<Product>(query, parameters);

                         if (product == null)
                         {
                              throw new KeyNotFoundException($"Product with id {id} not found");
                         }

                         return product;
                    }
               }

               // Update a product
               public async Task UpdateProduct(Product product)
               {
                    using (var connection = _dbConnectionFactory.CreateECommerceDbConnection())
                    {
                         var query = @"
                    UPDATE [dbo].[Products] 
                    SET 
                        [Name] = @Name,
                        [Description] = @Description,
                        [Price] = @Price,
                        [ImageUrl] = @ImageUrl,
                        [IsFeatured] = @IsFeatured
                    WHERE [ProductId] = @ProductId";

                         var parameters = new
                         {
                              product.Name,
                              product.Description,
                              product.Price,
                              product.ImageUrl,
                              product.IsFeatured,
                              product.ProductId
                         };

                         var affectedRows = await connection.ExecuteAsync(query, parameters);

                         if (affectedRows == 0)
                         {
                              throw new KeyNotFoundException($"Product with id {product.ProductId} not found");
                         }
                    }
               }
          }
     }
