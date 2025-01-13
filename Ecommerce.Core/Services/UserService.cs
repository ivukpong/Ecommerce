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
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Authentication;

namespace Ecommerce.Core.Services
{
     public class UserService : IUserService
     {
          private readonly IUsersRepository _usersRepository;
          private readonly IConfiguration _configuration;
          private readonly IValidator<LoginViewModel> _loginValidator;
          private readonly IValidator<RegisterViewModel> _registerValidator;

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
          }

          public async Task<LoginResponseDTO> Login(LoginViewModel model)
          {
               // Validate the login model
               var loginValidationResult = await _loginValidator.ValidateAsync(model);
               if (!loginValidationResult.IsValid)
               {
                    throw new ArgumentException(string.Join(", ", loginValidationResult.Errors.Select(e => e.ErrorMessage)));
               }

               if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
               {
                    throw new ArgumentNullException("Email and Password cannot be null");
               }

               // Retrieve user credentials
               var existingUser = await _usersRepository.GetUserCredentials(model.Email);
               if (existingUser == null)
               {
                    throw new Exception("Invalid login credentials");
               }

               // Verify password
               byte[] salt = Convert.FromBase64String(existingUser.Salt);
               byte[] passwordBytes = Encoding.UTF8.GetBytes(model.Password);
               byte[] saltedPassword = new byte[salt.Length + passwordBytes.Length];
               Buffer.BlockCopy(salt, 0, saltedPassword, 0, salt.Length);
               Buffer.BlockCopy(passwordBytes, 0, saltedPassword, salt.Length, passwordBytes.Length);

               byte[] enteredHash = HashPassword(saltedPassword);

               if (!enteredHash.SequenceEqual(existingUser.PasswordHash))
               {
                    throw new Exception("Invalid login credentials");
               }

               // Generate JWT token
               var token = GenerateJwtToken(existingUser).Result;

               return new LoginResponseDTO
               {
                    Token = token,
                    User = existingUser
               };
          }


          public async Task Register(RegisterViewModel model)
          {
               var registerValidationResult = await _registerValidator.ValidateAsync(model);
               if (!registerValidationResult.IsValid)
               {
                    throw new ArgumentException(string.Join(", ", registerValidationResult.Errors.Select(e => e.ErrorMessage)));
               }

               if (model.Email == null || model.Password == null || model.Username == null)
               {
                    throw new ArgumentNullException("Email, Password, and Username cannot be null");
               }

               var existingUser = await _usersRepository.GetUserCredentials(model.Email);
               if (existingUser != null)
               {
                    throw new Exception("User already exists");
               }

               byte[] salt = GenerateSalt(16); // 16 bytes (128 bits) salt

               byte[] passwordBytes = Encoding.UTF8.GetBytes(model.Password);
               byte[] saltedPassword = new byte[salt.Length + passwordBytes.Length];
               Buffer.BlockCopy(salt, 0, saltedPassword, 0, salt.Length);
               Buffer.BlockCopy(passwordBytes, 0, saltedPassword, salt.Length, passwordBytes.Length);

               byte[] hashedPassword = HashPassword(saltedPassword);

               string convertedSalt = Convert.ToBase64String(salt);

               User user = new User
               {
                    CreatedAt = DateTime.Now,
                    Email = model.Email,
                    PasswordHash = hashedPassword,
                    Salt = convertedSalt,
                    Username = model.Username,
                    Role = "User" // Assign role here
               };

               await _usersRepository.AddNewUser(user);
          }

          public async Task<User> GetUser(string email)
          {
               return await _usersRepository.GetUserCredentials(email);
          }
          public Task<string> GenerateJwtToken(User user)
          {
               var claims = new[]
               {
                  new Claim(ClaimTypes.Name, user.Username),
                  new Claim(ClaimTypes.Email, user.Email),
                  new Claim(ClaimTypes.Role, user.Role) // Add role claim
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

               return Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));
          }

          private byte[] GenerateSalt(int length)
          {
               var jwtSecretKey = _configuration["JwtSecretKey"];
               if (string.IsNullOrEmpty(jwtSecretKey))
               {
                    throw new Exception("JWT Secret Key is not configured.");
               }
               var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey));
               SecureRandom random = new SecureRandom();
               byte[] salt = new byte[length];
               random.NextBytes(salt);
               return salt;
          }

          private byte[] HashPassword(byte[] saltedPassword)
          {
               using (SHA256 sha256 = SHA256.Create())
               {
                    return sha256.ComputeHash(saltedPassword);
               }
          }

          
     }
}
