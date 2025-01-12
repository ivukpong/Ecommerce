using Ecommerce.Core.Models;
using FluentValidation;

namespace Ecommerce.Infrastructure.Validators
{
     public class RegisterViewModelValidator : AbstractValidator<RegisterViewModel>
     {
          public RegisterViewModelValidator()
          {
               RuleFor(x => x.Email)
                   .NotEmpty().WithMessage("Email is required.")
                   .EmailAddress().WithMessage("Invalid email address.");

               RuleFor(x => x.Username)
                   .NotEmpty().WithMessage("Username is required.")
                   .MinimumLength(6).WithMessage("Username must be at least 6 characters long.")
                   .MaximumLength(100).WithMessage("Username cannot exceed 100 characters.");

               RuleFor(x => x.Password)
                   .NotEmpty().WithMessage("Password is required.")
                   .MinimumLength(6).WithMessage("Password must be at least 6 characters long.")
                   .MaximumLength(100).WithMessage("Password cannot exceed 100 characters.");

               RuleFor(x => x.ConfirmPassword)
                   .NotEmpty().WithMessage("Confirm password is required.")
                   .Equal(x => x.Password).WithMessage("The password and confirmation password do not match.");
          }
     }
}
