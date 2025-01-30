using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Ecommerce.Core.Interfaces.IFactories;
using Ecommerce.Core.Interfaces.IRepository;
using Ecommerce.Core.Models;
using Serilog;

namespace Ecommerce.Infrastructure.Repositories
{
     public class ProductsRepository : IProductsRepository
     {
          private readonly IDbConnectionFactory _dbConnectionFactory;
          private readonly ILogger _logger;

          public ProductsRepository(IDbConnectionFactory dbConnectionFactory, ILogger logger)
          {
               _dbConnectionFactory = dbConnectionFactory;
               _logger = logger;
          }

          // Get all products
          public async Task<List<Product>> GetAllProducts()
          {
               _logger.Information("Fetching all products from the database.");

               using var connection = _dbConnectionFactory.CreateECommerceDbConnection();
               var products = (await connection.QueryAsync<Product>("proc_GetAllProducts", commandType: CommandType.StoredProcedure)).ToList();

               _logger.Information("Successfully fetched {ProductCount} products.", products.Count);
               return products;
          }

          // Get a product by ID
          public async Task<Product> GetProductById(int id)
          {
               _logger.Information("Fetching product with ID {ProductId}.", id);

               using var connection = _dbConnectionFactory.CreateECommerceDbConnection();
               var product = await connection.QuerySingleOrDefaultAsync<Product>("proc_GetProductById", new { ProductId = id }, commandType: CommandType.StoredProcedure);

               if (product == null)
               {
                    _logger.Warning("Product with ID {ProductId} not found.", id);
               }
               else
               {
                    _logger.Information("Successfully fetched product with ID {ProductId}.", id);
               }

               return product;
          }

          // Create a new product
          public async Task CreateProduct(Product product)
          {
               _logger.Information("Creating a new product: {ProductName}", product.Name);

               try
               {
                    using var connection = _dbConnectionFactory.CreateECommerceDbConnection();
                    var parameters = new DynamicParameters();
                    parameters.Add("Name", product.Name);
                    parameters.Add("Price", product.Price);
                    parameters.Add("Description", product.Description);
                    parameters.Add("ImageUrl", product.ImageUrl);
                    parameters.Add("IsFeatured", product.IsFeatured);
                    parameters.Add("ProductId", dbType: DbType.Int32, direction: ParameterDirection.Output);

                    await connection.ExecuteAsync("proc_CreateProduct", parameters, commandType: CommandType.StoredProcedure);
                    product.ProductId = parameters.Get<int>("ProductId");

                    _logger.Information("Product created successfully with ID {ProductId}.", product.ProductId);
               }
               catch (Exception ex)
               {
                    _logger.Error(ex, "Error creating product: {ProductName}", product.Name);
                    throw;
               }
          }

          // Update a product
          public async Task UpdateProduct(Product product)
          {
               _logger.Information("Updating product with ID {ProductId}.", product.ProductId);

               try
               {
                    using var connection = _dbConnectionFactory.CreateECommerceDbConnection();
                    var parameters = new DynamicParameters(product);

                    await connection.ExecuteAsync("proc_UpdateProduct", parameters, commandType: CommandType.StoredProcedure);

                    _logger.Information("Product with ID {ProductId} updated successfully.", product.ProductId);
               }
               catch (Exception ex)
               {
                    _logger.Error(ex, "Error updating product with ID {ProductId}.", product.ProductId);
                    throw;
               }
          }

          // Delete a product by ID
          public async Task<Product> DeleteProduct(int id)
          {
               _logger.Information("Deleting product with ID {ProductId}.", id);

               try
               {
                    using var connection = _dbConnectionFactory.CreateECommerceDbConnection();
                    var parameters = new DynamicParameters();
                    parameters.Add("ProductId", id);

                    var product = await connection.QuerySingleOrDefaultAsync<Product>("proc_GetProductById", parameters, commandType: CommandType.StoredProcedure);
                    if (product != null)
                    {
                         await connection.ExecuteAsync("proc_DeleteProduct", parameters, commandType: CommandType.StoredProcedure);
                         _logger.Information("Product with ID {ProductId} deleted successfully.", id);
                    }
                    else
                    {
                         _logger.Warning("Product with ID {ProductId} not found. Nothing to delete.", id);
                    }

                    return product;
               }
               catch (Exception ex)
               {
                    _logger.Error(ex, "Error deleting product with ID {ProductId}.", id);
                    throw;
               }
          }

          // Check if a product exists by ID
          public async Task<bool> CheckForProduct(int id)
          {
               _logger.Information("Checking if product with ID {ProductId} exists.", id);

               using var connection = _dbConnectionFactory.CreateECommerceDbConnection();
               var product = await connection.QuerySingleOrDefaultAsync<Product>("proc_GetProductById", new { ProductId = id }, commandType: CommandType.StoredProcedure);

               bool exists = product != null;
               _logger.Information("Product with ID {ProductId} {Exists}.", id, exists ? "exists" : "does not exist");

               return exists;
          }
     }
}
