using FluentValidation;
using OfficeDeskReservation.API.Dtos.Rooms;

namespace OfficeDeskReservation.API.Validators
{
    public class RoomDtoValidator : AbstractValidator<RoomDto>
    {
        public RoomDtoValidator()
        {
            RuleFor(r => r.Name)
                .NotEmpty().WithMessage("Room name can't be empty!")
                .Length(3, 30).WithMessage("Room name must have at least 3 characters and no more than 30!");
        }
    }
}
