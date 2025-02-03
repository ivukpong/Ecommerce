using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class JwtAuthenticationMiddleware
{
     private readonly RequestDelegate _next;
     private readonly IConfiguration _configuration;

     public JwtAuthenticationMiddleware(RequestDelegate next, IConfiguration configuration)
     {
          _next = next;
          _configuration = configuration;
     }

     public async Task Invoke(HttpContext context)
     {
          var token = context.Request.Cookies["jwtToken"];
          if (!string.IsNullOrEmpty(token))
          {
               var tokenHandler = new JwtSecurityTokenHandler();
               var secretKey = _configuration["JwtSecretKey"];
               if (secretKey != null)
               {
                    var key = Encoding.UTF8.GetBytes(secretKey);

                    try
                    {
                         var claimsPrincipal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                         {
                              ValidateIssuerSigningKey = true,
                              IssuerSigningKey = new SymmetricSecurityKey(key),
                              ValidateIssuer = false,
                              ValidateAudience = false,
                              ClockSkew = TimeSpan.Zero
                         }, out _);

                         var userEmail = claimsPrincipal?.FindFirst(ClaimTypes.Email)?.Value;

                         if (!string.IsNullOrEmpty(userEmail) && claimsPrincipal != null)
                         {
                              context.Items["UserEmail"] = userEmail;
                              context.User = claimsPrincipal; // Set the user identity
                         }
                    }
                    catch
                    {
                         // Handle invalid token (log, clear cookies, etc.)
                    }
               }
          }

          await _next(context);
     }
}
