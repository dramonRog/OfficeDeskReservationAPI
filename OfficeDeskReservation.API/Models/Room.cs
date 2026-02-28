namespace OfficeDeskReservation.API.Models
{
    public class Room
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public ICollection<Desk> Desks { get; set; } = new List<Desk>();
    }
}
