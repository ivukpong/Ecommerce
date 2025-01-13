using Ecommerce.Core.Interfaces.IFactories;
using Ecommerce.Core.Interfaces.IRepository;
using Ecommerce.Core.Models;
using Dapper;
using System.Data;

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
                    var parameters = new DynamicParameters();
                    parameters.Add("ProductId", id);

                    var count = await connection.ExecuteScalarAsync<int>(
                        "proc_CheckForProduct", // Replace with the actual stored procedure name
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );

                    return count > 0;
               }
          }

          // Create a new product
          public async Task CreateProduct(Product product)
          {
               using (var connection = _dbConnectionFactory.CreateECommerceDbConnection())
               {
                    var parameters = new DynamicParameters();
                    parameters.Add("Name", product.Name);
                    parameters.Add("Description", product.Description);
                    parameters.Add("Price", product.Price);
                    parameters.Add("ImageUrl", product.ImageUrl);
                    parameters.Add("IsFeatured", product.IsFeatured);

                    await connection.ExecuteAsync(
                        "proc_CreateProduct", // Replace with the actual stored procedure name
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );
               }
          }

          // Delete a product by its ID
          public async Task<Product> DeleteProduct(int id)
          {
               using (var connection = _dbConnectionFactory.CreateECommerceDbConnection())
               {
                    var parameters = new DynamicParameters();
                    parameters.Add("ProductId", id);

                    var product = await connection.QuerySingleOrDefaultAsync<Product>(
                        "proc_GetProductById", // Replace with the actual stored procedure name
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );

                    if (product == null)
                    {
                         throw new KeyNotFoundException($"Product with id {id} not found");
                    }

                    await connection.ExecuteAsync(
                        "proc_DeleteProduct", // Replace with the actual stored procedure name
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );

                    return product;
               }
          }

          // Get all products
          public async Task<List<Product>> GetAllProducts()
          {
               using (var connection = _dbConnectionFactory.CreateECommerceDbConnection())
               {
                    var products = await connection.QueryAsync<Product>(
                        "proc_GetAllProducts", // Replace with the actual stored procedure name
                        commandType: CommandType.StoredProcedure
                    );

                    return products.ToList();
               }
          }

          // Get a product by its ID
          public async Task<Product> GetProductById(int id)
          {
               using (var connection = _dbConnectionFactory.CreateECommerceDbConnection())
               {
                    var parameters = new DynamicParameters();
                    parameters.Add("ProductId", id);

                    var product = await connection.QuerySingleOrDefaultAsync<Product>(
                        "proc_GetProductById", // Replace with the actual stored procedure name
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );

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
                    var parameters = new DynamicParameters();
                    parameters.Add("ProductId", product.ProductId);
                    parameters.Add("Name", product.Name);
                    parameters.Add("Description", product.Description);
                    parameters.Add("Price", product.Price);
                    parameters.Add("ImageUrl", product.ImageUrl);
                    parameters.Add("IsFeatured", product.IsFeatured);

                    var affectedRows = await connection.ExecuteAsync(
                        "proc_UpdateProduct", // Replace with the actual stored procedure name
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );

                    if (affectedRows == 0)
                    {
                         throw new KeyNotFoundException($"Product with id {product.ProductId} not found");
                    }
               }
          }
     }
}
