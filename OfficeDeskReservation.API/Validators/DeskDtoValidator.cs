using FluentValidation;
using OfficeDeskReservation.API.Dtos.Desks;

namespace OfficeDeskReservation.API.Validators
{
    public class DeskDtoValidator : AbstractValidator<DeskDto>
    {
        public DeskDtoValidator()
        {
            RuleFor(d => d.DeskIdentifier)
                .NotEmpty().WithMessage("Desk identifier is required.")
                .Length(1, 20).WithMessage("Desk identifier must be between 1 and 20 characters.");

            RuleFor(d => d.RoomId)
                .GreaterThan(0).WithMessage("A valid Room ID is required.");
        }
    }
}
