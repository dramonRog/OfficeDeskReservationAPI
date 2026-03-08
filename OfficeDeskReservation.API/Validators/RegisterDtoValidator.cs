using FluentValidation;
using OfficeDeskReservation.API.Dtos.Auth;

namespace OfficeDeskReservation.API.Validators
{
    public class RegisterDtoValidator : AbstractValidator<RegisterDto>
    {
        public RegisterDtoValidator()
        {
            RuleFor(r => r.FirstName)
                .NotEmpty().WithMessage("First name is required.")
                .MinimumLength(2).WithMessage("First name must contain at least 2 characters.");

            RuleFor(r => r.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .MinimumLength(2).WithMessage("Last name must contain at least 2 characters");

            RuleFor(r => r.Email)
                .NotEmpty().WithMessage("Email is required.")
                .Matches(@"^[^@\s]+@[^@\s]+\.[^@\s]+$").WithMessage("Email address is invalid.")
                .MaximumLength(100).WithMessage("Email must not exceed 100 characters.");

            RuleFor(r => r.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(8).WithMessage("Password must contain at least 8 characters.")
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                .Matches("[0-9]").WithMessage("Password must contain at least one number.")
                .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character");
        }
    }
}
