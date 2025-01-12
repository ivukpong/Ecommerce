using Ecommerce.Core.Models;
using FluentValidation;

namespace Ecommerce.Core.Validators
{
     public class ProductValidator : AbstractValidator<Product>
     {
          public ProductValidator()
          {
               RuleFor(p => p.Name)
                   .NotEmpty().WithMessage("Product name is required.")
                   .Length(1, 100).WithMessage("Product name cannot exceed 100 characters.");

               RuleFor(p => p.Description)
                   .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");

               RuleFor(p => p.Price)
                   .GreaterThan(0).WithMessage("Price must be a positive value.");

               RuleFor(p => p.ImageUrl)
                   .Must(url => Uri.IsWellFormedUriString(url, UriKind.Absolute)).When(p => !string.IsNullOrEmpty(p.ImageUrl))
                   .WithMessage("Invalid URL format.");
          }
     }
}
