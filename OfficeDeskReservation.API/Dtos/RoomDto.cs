using OfficeDeskReservation.API.Models;

namespace OfficeDeskReservation.API.Dtos
{
    public class RoomDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<DeskDto> Desks { get; set; } = new();
    }
}
