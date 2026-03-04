using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OfficeDeskReservation.API.Data;
using OfficeDeskReservation.API.Dtos.Users;

namespace OfficeDeskReservation.API.Validators
{
    public class UserDtoValidator : AbstractValidator<UserDto>
    {
        public UserDtoValidator(AppDbContext context)
        {
            RuleFor(u => u.FirstName)
                .NotEmpty().WithMessage("First name is required.");

            RuleFor(u => u.LastName)
                .NotEmpty().WithMessage("Last name is required.");

            RuleFor(u => u.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Email address is invalid.")
                .MaximumLength(100).WithMessage("Email must not exceed 100 characters.");
        }
    }
}
