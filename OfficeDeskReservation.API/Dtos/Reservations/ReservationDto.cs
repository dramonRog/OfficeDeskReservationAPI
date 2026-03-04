namespace OfficeDeskReservation.API.Dtos.Reservations
{
    public class ReservationDto
    {
        public int DeskId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
