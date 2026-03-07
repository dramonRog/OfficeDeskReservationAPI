using FluentValidation;
using OfficeDeskReservation.API.Dtos.Reservations;

namespace OfficeDeskReservation.API.Validators
{
    public class ReservationDtoValidator : AbstractValidator<ReservationDto>
    {
        public ReservationDtoValidator()
        {
            RuleFor(r => r.DeskId)
                .GreaterThan(0).WithMessage("A valid Desk ID is required.");

            RuleFor(r => r.StartTime)
                .NotEmpty().WithMessage("Start time is required.")
                .GreaterThan(DateTime.Now).WithMessage("Start time must be in the future.");

            RuleFor(r => r.EndTime)
                .NotEmpty().WithMessage("End time is required.")
                .GreaterThan(r => r.StartTime).WithMessage("End time must be after the start time.");
        }
    }
}
