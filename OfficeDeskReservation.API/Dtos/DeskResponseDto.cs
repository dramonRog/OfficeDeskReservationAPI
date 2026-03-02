namespace OfficeDeskReservation.API.Dtos
{
    public class DeskResponseDto
    {
        public int Id { get; set; }
        public string DeskIdentifier { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
    }
}
