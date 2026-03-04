using FluentValidation;
using OfficeDeskReservation.API.Dtos.Auth;

namespace OfficeDeskReservation.API.Validators
{
    public class LoginDtoValidator : AbstractValidator<LoginDto>
    {
        public LoginDtoValidator()
        {
            RuleFor(l => l.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Email is invalid.")
                .MaximumLength(100).WithMessage("Email must not exceed 100 characters.");

            RuleFor(l => l.Password)
                .NotEmpty().WithMessage("Password is required.");
        }
    }
}
