using Ecommerce.Core.Interfaces.IRepository;
using Ecommerce.Core.Interfaces.IServices;
using Ecommerce.Core.Models;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Org.BouncyCastle.Security;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Ecommerce.Core.DTOs;
using Serilog;

namespace Ecommerce.Core.Services
{
     public class UserService : IUserService
     {
          private readonly IUsersRepository _usersRepository;
          private readonly IConfiguration _configuration;
          private readonly IValidator<LoginViewModel> _loginValidator;
          private readonly IValidator<RegisterViewModel> _registerValidator;
          private readonly ILogger _logger;

          public UserService(
              IUsersRepository usersRepository,
              IConfiguration configuration,
              IValidator<LoginViewModel> loginValidator,
              IValidator<RegisterViewModel> registerValidator)
          {
               _usersRepository = usersRepository;
               _configuration = configuration;
               _loginValidator = loginValidator;
               _registerValidator = registerValidator;
               _logger = Log.ForContext<UserService>();
          }

          public async Task<LoginResponseDTO> Login(LoginViewModel model)
          {
               _logger.Information("Login attempt for email: {Email}", model.Email);

               var loginValidationResult = await _loginValidator.ValidateAsync(model);
               if (!loginValidationResult.IsValid)
               {
                    _logger.Warning("Login validation failed for email: {Email}. Errors: {Errors}",
                        model.Email, string.Join(", ", loginValidationResult.Errors.Select(e => e.ErrorMessage)));
                    throw new ArgumentException(string.Join(", ", loginValidationResult.Errors.Select(e => e.ErrorMessage)));
               }

               if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
               {
                    _logger.Warning("Login failed: Email or Password is null for email: {Email}", model.Email);
                    throw new ArgumentNullException("Email and Password cannot be null");
               }

               var existingUser = await _usersRepository.GetUserCredentials(model.Email);
               if (existingUser == null)
               {
                    _logger.Warning("Invalid login attempt for email: {Email}", model.Email);
                    throw new Exception("Invalid login credentials");
               }

               // Verify password
               byte[] salt = Convert.FromBase64String(existingUser.Salt);
               byte[] passwordBytes = Encoding.UTF8.GetBytes(model.Password);
               byte[] saltedPassword = salt.Concat(passwordBytes).ToArray();
               byte[] enteredHash = HashPassword(saltedPassword);

               if (!enteredHash.SequenceEqual(existingUser.PasswordHash))
               {
                    _logger.Warning("Invalid password attempt for email: {Email}", model.Email);
                    throw new Exception("Invalid login credentials");
               }

               var token = await GenerateJwtToken(existingUser);
               _logger.Information("User logged in successfully: {Email}", model.Email);

               return new LoginResponseDTO
               {
                    Token = token,
                    User = existingUser
               };
          }

          public async Task Register(RegisterViewModel model)
          {
               _logger.Information("Registration attempt for email: {Email}", model.Email);

               var registerValidationResult = await _registerValidator.ValidateAsync(model);
               if (!registerValidationResult.IsValid)
               {
                    _logger.Warning("Registration validation failed for email: {Email}. Errors: {Errors}",
                        model.Email, string.Join(", ", registerValidationResult.Errors.Select(e => e.ErrorMessage)));
                    throw new ArgumentException(string.Join(", ", registerValidationResult.Errors.Select(e => e.ErrorMessage)));
               }

               if (model.Email == null || model.Password == null || model.Username == null)
               {
                    _logger.Warning("Registration failed: Missing required fields for email: {Email}", model.Email);
                    throw new ArgumentNullException("Email, Password, and Username cannot be null");
               }

               var existingUser = await _usersRepository.GetUserCredentials(model.Email);
               if (existingUser != null)
               {
                    _logger.Warning("Registration failed: User already exists with email: {Email}", model.Email);
                    throw new Exception("User already exists");
               }

               byte[] salt = GenerateSalt(16);
               byte[] passwordBytes = Encoding.UTF8.GetBytes(model.Password);
               byte[] saltedPassword = salt.Concat(passwordBytes).ToArray();
               byte[] hashedPassword = HashPassword(saltedPassword);

               User user = new User
               {
                    CreatedAt = DateTime.Now,
                    Email = model.Email,
                    PasswordHash = hashedPassword,
                    Salt = Convert.ToBase64String(salt),
                    Username = model.Username,
                    Role = "User"
               };

               await _usersRepository.AddNewUser(user);
               _logger.Information("User registered successfully: {Email}", model.Email);
          }

          public async Task<User> GetUser(string email)
          {
               _logger.Information("Fetching user details for email: {Email}", email);
               return await _usersRepository.GetUserCredentials(email);
          }

          private async Task<string> GenerateJwtToken(User user)
          {
               _logger.Information("Generating JWT token for user: {Email}", user.Email);

               var claims = new[]
               {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

               var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSecretKey"]));
               var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

               var token = new JwtSecurityToken(
                   issuer: "EcommerceApp",
                   audience: "EcommerceAppUser",
                   claims: claims,
                   expires: DateTime.Now.AddHours(1),
                   signingCredentials: creds
               );

               _logger.Information("JWT token generated successfully for user: {Email}", user.Email);
               return await Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));
          }

          private byte[] GenerateSalt(int length)
          {
               var jwtSecretKey = _configuration["JwtSecretKey"];
               if (string.IsNullOrEmpty(jwtSecretKey))
               {
                    _logger.Error("JWT Secret Key is not configured.");
                    throw new Exception("JWT Secret Key is not configured.");
               }

               var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey));
               SecureRandom random = new SecureRandom();
               byte[] salt = new byte[length];
               random.NextBytes(salt);

               _logger.Information("Generated salt for password hashing.");
               return salt;
          }

          private byte[] HashPassword(byte[] saltedPassword)
          {
               using (SHA256 sha256 = SHA256.Create())
               {
                    var hashedPassword = sha256.ComputeHash(saltedPassword);
                    _logger.Information("Password hashed successfully.");
                    return hashedPassword;
               }
          }
     }
}
