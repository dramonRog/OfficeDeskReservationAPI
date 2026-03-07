using OfficeDeskReservation.API.Dtos.Reservations;
using OfficeDeskReservation.API.Models;

namespace OfficeDeskReservation.API.Dtos.Users
{
    public class UserResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public Role Role { get; set; }
        public List<ReservationResponseDto> Reservations { get; set; } = new();
    }
}
