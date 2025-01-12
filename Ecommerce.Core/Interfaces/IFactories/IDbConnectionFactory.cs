using System.Data;

namespace Ecommerce.Core.Interfaces.IFactories
{
     public interface IDbConnectionFactory
     {
          IDbConnection CreateECommerceDbConnection();
     }
}
