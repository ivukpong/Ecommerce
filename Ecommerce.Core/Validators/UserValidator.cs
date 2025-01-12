using Ecommerce.Core.Models;
using FluentValidation;

namespace Ecommerce.Core.Validators
{
     public class UserValidator : AbstractValidator<User>
     {
          public UserValidator()
          {
               RuleFor(x => x.Username)
                   .NotEmpty().WithMessage("Username is required.")
                   .Length(1, 50).WithMessage("Username cannot exceed 50 characters.");

               RuleFor(x => x.Email)
                   .NotEmpty().WithMessage("Email is required.")
                   .EmailAddress().WithMessage("Invalid email address.")
                   .Length(1, 100).WithMessage("Email cannot exceed 100 characters.");
               RuleFor(x => x.PasswordHash)
                   .NotEmpty().WithMessage("Password hash is required.");

               RuleFor(x => x.Salt)
                   .NotEmpty().WithMessage("Salt is required.")
                   .Length(1, 20).WithMessage("Salt cannot exceed 20 characters.");

               RuleFor(x => x.CreatedAt)
                   .NotEmpty().WithMessage("Created at timestamp is required.");
          }
     }
}
