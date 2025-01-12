using Ecommerce.Core.Models;
using FluentValidation;

namespace Ecommerce.Core.Validators
{
     public class LoginViewModelValidator : AbstractValidator<LoginViewModel>
     {
          public LoginViewModelValidator()
          {
               RuleFor(x => x.Email)
                   .NotEmpty().WithMessage("Email is required.")  // Ensures email is not empty
                   .EmailAddress().WithMessage("Invalid email address.");  // Validates proper email format

               RuleFor(x => x.Password)
                   .NotEmpty().WithMessage("Password is required.")  // Ensures password is not empty
                   .MinimumLength(6).WithMessage("Password must be at least 6 characters long.");  // Optional, but can enforce a minimum length

               RuleFor(x => x.RememberMe)
                   .NotNull(); // Ensure the RememberMe field is not null (default is false, but can be customized)
          }
     }
}
