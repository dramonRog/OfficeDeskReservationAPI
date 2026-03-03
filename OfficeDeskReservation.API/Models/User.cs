using System.ComponentModel.DataAnnotations;

namespace OfficeDeskReservation.API.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public Role Role { get; set; } = Role.User;
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
