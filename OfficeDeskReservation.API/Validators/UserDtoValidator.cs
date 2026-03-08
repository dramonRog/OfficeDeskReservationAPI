using FluentValidation;
using OfficeDeskReservation.API.Dtos.Users;

namespace OfficeDeskReservation.API.Validators
{
    public class UserDtoValidator : AbstractValidator<UserDto>
    {
        public UserDtoValidator()
        {
            RuleFor(u => u.FirstName)
                .NotEmpty().WithMessage("First name is required!")
                .MinimumLength(2).WithMessage("First name must contain at least 2 characters!");

            RuleFor(u => u.LastName)
                .NotEmpty().WithMessage("Last name is required!")
                .MinimumLength(2).WithMessage("Last name must contain at least 2 characters!");

            RuleFor(u => u.Email)
                .NotEmpty().WithMessage("Email address is required!")
                .Matches(@"^[^@\s]+@[^@\s]+\.[^@\s]+$").WithMessage("Email address is invalid!")
                .MaximumLength(100).WithMessage("Email address must not exceed 100 characters!");
        }
    }
}
