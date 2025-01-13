using Ecommerce.Core.Interfaces.IFactories;
using Ecommerce.Core.Interfaces.IRepository;
using Ecommerce.Core.Models;
using Dapper;
using System.Data;

namespace Ecommerce.Infrastructure.Repositories
{
     public class UserRepository : IUsersRepository
     {
          private readonly IDbConnectionFactory _dbConnectionFactory;

          public UserRepository(IDbConnectionFactory dbConnectionFactory)
          {
               _dbConnectionFactory = dbConnectionFactory;
          }

          public async Task AddNewUser(User user)
          {
               using (var connection = _dbConnectionFactory.CreateECommerceDbConnection())
               {
                    var parameters = new DynamicParameters();
                    parameters.Add("UserName", user.Username);
                    parameters.Add("Email", user.Email);
                    parameters.Add("PasswordHash", user.PasswordHash);
                    parameters.Add("Salt", user.Salt);
                    parameters.Add("CreatedAt", user.CreatedAt);

                    await connection.ExecuteAsync(
                        "proc_AddNewUser", // Replace with the actual procedure name
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );
               }
          }

          public async Task<User> GetUserCredentials(string email)
          {
               using (var connection = _dbConnectionFactory.CreateECommerceDbConnection())
               {
                    var parameters = new DynamicParameters();
                    parameters.Add("Email", email);

                    var user = await connection.QuerySingleOrDefaultAsync<User>(
                        "proc_GetUserCredentials", // Replace with the actual procedure name
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );

                    return user;
               }
          }
     }
}
