using OfficeDeskReservation.API.Dtos.Reservations;

namespace OfficeDeskReservation.API.Dtos.Desks
{
    public class DeskResponseDto
    {
        public int Id { get; set; }
        public string DeskIdentifier { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
        public List<ReservationResponseDto> Reservations { get; set; } = new();
    }
}
