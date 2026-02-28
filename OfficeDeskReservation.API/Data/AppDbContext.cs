using Microsoft.EntityFrameworkCore;
using OfficeDeskReservation.API.Models;

namespace OfficeDeskReservation.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Desk> Desks { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
    }
}
