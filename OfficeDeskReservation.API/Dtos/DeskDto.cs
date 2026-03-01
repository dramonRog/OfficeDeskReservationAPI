namespace OfficeDeskReservation.API.Dtos
{
    public class DeskDto
    {
        public int Id { get; set; }
        public string DeskIdentifier { get; set; } = string.Empty;
        public int RoomId { get; set; }
    }
}
