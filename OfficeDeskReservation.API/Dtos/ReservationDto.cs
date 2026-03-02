namespace OfficeDeskReservation.API.Dtos
{
    public class ReservationDto
    {
        public int UserId { get; set; }
        public int DeskId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
