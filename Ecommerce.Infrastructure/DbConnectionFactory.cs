using Ecommerce.Core.Interfaces.IFactories;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace Ecommerce.Infrastructure
{
     public class DbConnectionFactory : IDbConnectionFactory
     {
          private readonly IConfiguration _configuration;

          public DbConnectionFactory(IConfiguration configuration)
          {
               _configuration = configuration;
          }

          public IDbConnection CreateECommerceDbConnection()
          {
               return new SqlConnection(_configuration["ConnectionStrings:ECommerceDb"]);
          }
     }
}
