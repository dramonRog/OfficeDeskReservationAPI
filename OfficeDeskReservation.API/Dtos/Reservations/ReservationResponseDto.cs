namespace OfficeDeskReservation.API.Dtos.Reservations
{
    public class ReservationResponseDto
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string DeskName { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
