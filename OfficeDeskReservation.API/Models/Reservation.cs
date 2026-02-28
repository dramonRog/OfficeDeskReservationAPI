namespace OfficeDeskReservation.API.Models
{
    public class Reservation
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        public int DeskId { get; set; }
        public Desk? Desk { get; set; }
    }
}
