using Ecommerce.Core.Interfaces.IRepository;
using Ecommerce.Core.Interfaces.IServices;
using Ecommerce.Core.Models;
using Ecommerce.Core.DTOs;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Org.BouncyCastle.Security;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
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

               var validationResult = await _loginValidator.ValidateAsync(model);
               if (!validationResult.IsValid)
               {
                    _logger.Warning("Login validation failed: {Errors}", string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
                    throw new ValidationException(validationResult.Errors);
               }

               var existingUser = await _usersRepository.GetUserCredentials(model.Email);
               if (existingUser == null)
               {
                    _logger.Warning("Login failed: User not found for email: {Email}", model.Email);
                    throw new UnauthorizedAccessException("Invalid login credentials.");
               }

               if (!VerifyPassword(model.Password, existingUser.Salt, existingUser.PasswordHash))
               {
                    _logger.Warning("Login failed: Incorrect password for email: {Email}", model.Email);
                    throw new UnauthorizedAccessException("Invalid login credentials.");
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

               var validationResult = await _registerValidator.ValidateAsync(model);
               if (!validationResult.IsValid)
               {
                    _logger.Warning("Registration validation failed: {Errors}", string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
                    throw new ValidationException(validationResult.Errors);
               }

               var existingUser = await _usersRepository.GetUserCredentials(model.Email);
               if (existingUser != null)
               {
                    _logger.Warning("Registration failed: User already exists with email: {Email}", model.Email);
                    throw new InvalidOperationException("User with this email already exists.");
               }

               byte[] salt = GenerateSalt(16);
               byte[] hashedPassword = HashPassword(salt, model.Password);

               User user = new User
               {
                    CreatedAt = DateTime.UtcNow,
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
                   expires: DateTime.UtcNow.AddHours(1),
                   signingCredentials: creds
               );

               _logger.Information("JWT token generated successfully for user: {Email}", user.Email);
               return await Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));
          }

          private bool VerifyPassword(string password, string saltBase64, byte[] storedHash)
          {
               byte[] salt = Convert.FromBase64String(saltBase64);
               byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
               byte[] saltedPassword = salt.Concat(passwordBytes).ToArray();
               byte[] enteredHash = HashPassword(saltedPassword);

               return enteredHash.SequenceEqual(storedHash);
          }

          private byte[] GenerateSalt(int length)
          {
               var salt = new byte[length];
               using (var rng = RandomNumberGenerator.Create())
               {
                    rng.GetBytes(salt);
               }
               _logger.Information("Generated salt for password hashing.");
               return salt;
          }

          private byte[] HashPassword(byte[] saltedPassword)
          {
               using (SHA256 sha256 = SHA256.Create())
               {
                    return sha256.ComputeHash(saltedPassword);
               }
          }

          private byte[] HashPassword(byte[] salt, string password)
          {
               byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
               byte[] saltedPassword = salt.Concat(passwordBytes).ToArray();
               return HashPassword(saltedPassword);
          }
     }
}
