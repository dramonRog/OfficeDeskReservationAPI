namespace OfficeDeskReservation.API.Models
{
    public class Desk
    {
        public int Id { get; set; }
        public string DeskIdentifier { get; set; } = string.Empty;
        public int RoomId { get; set; }
        public Room? Room { get; set; }
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
