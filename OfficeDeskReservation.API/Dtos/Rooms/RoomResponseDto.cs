using OfficeDeskReservation.API.Dtos.Desks;

namespace OfficeDeskReservation.API.Dtos.Rooms
{
    public class RoomResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<DeskResponseDto> Desks { get; set; } = new();
    }
}
