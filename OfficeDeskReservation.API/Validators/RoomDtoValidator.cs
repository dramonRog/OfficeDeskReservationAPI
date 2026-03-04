using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OfficeDeskReservation.API.Data;
using OfficeDeskReservation.API.Dtos.Rooms;

namespace OfficeDeskReservation.API.Validators
{
    public class RoomDtoValidator : AbstractValidator<RoomDto>
    {
        private readonly AppDbContext _context;
        public RoomDtoValidator(AppDbContext context)
        {
            _context = context;

            RuleFor(r => r.Name)
                .NotEmpty().WithMessage("Room name can't be empty!")
                .Length(3, 30).WithMessage("Room name must have at least 3 characters and no more than 30")
                .MustAsync(async (name, token) => !await _context.Rooms.AnyAsync(r => r.Name == name, token)).WithMessage("A room with that name already exists.");
        }
    }
}
