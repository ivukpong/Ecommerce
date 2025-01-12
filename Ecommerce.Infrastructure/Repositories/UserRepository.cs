using Ecommerce.Core.Interfaces.IFactories;
using Ecommerce.Core.Interfaces.IRepository;
using Ecommerce.Core.Models;
using Dapper;

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
                    var query = @"
                    INSERT INTO [dbo].[Users] 
                        ([Username], [Email], [PasswordHash], [Salt], [CreatedAt]) 
                    VALUES 
                        (@UserName, @Email, @PasswordHash, @Salt, @CreatedAt)";

                    var parameters = new
                    {
                         user.Username,
                         user.Email,
                         user.PasswordHash,
                         user.Salt,
                         user.CreatedAt
                    };

                    await connection.ExecuteAsync(query, parameters);
               }
          }

          public async Task<User> GetUserCredentials(string email)
          {
               using (var connection = _dbConnectionFactory.CreateECommerceDbConnection())
               {
                    var query = @"
                  SELECT [Email], [Username], [PasswordHash], [Salt]
                  FROM [dbo].[Users] 
                  WHERE [Email] = @Email";

                    var parameters = new { Email = email };

                    User? user = await connection.QuerySingleOrDefaultAsync<User>(query, parameters);

                    return user;
               }
          }
     }
}
