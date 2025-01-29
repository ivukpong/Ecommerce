using Ecommerce.Core.Interfaces.IFactories;
using Ecommerce.Core.Interfaces.IRepository;
using Ecommerce.Core.Models;
using Dapper;
using System.Data;
using Serilog;

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
               try
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

                         Log.Information("User {Email} successfully added to the database.", user.Email);
                    }
               }
               catch (Exception ex)
               {
                    Log.Error(ex, "Error adding user {Email} to the database.", user.Email);
                    throw new Exception("An error occurred while adding a new user. Please try again later.");
               }
          }

          public async Task<User> GetUserCredentials(string email)
          {
               try
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

                         if (user != null)
                         {
                              Log.Information("User credentials retrieved successfully for {Email}.", email);
                         }
                         else
                         {
                              Log.Warning("No user found with email {Email}.", email);
                         }

                         return user;
                    }
               }
               catch (Exception ex)
               {
                    Log.Error(ex, "Error retrieving user credentials for {Email}.", email);
                    throw new Exception("An error occurred while fetching user credentials. Please try again later.");
               }
          }
     }
}
