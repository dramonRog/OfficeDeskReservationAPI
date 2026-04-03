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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Reservations)
                .WithOne(r => r.User)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Desk>()
                .HasMany(d => d.Reservations)
                .WithOne(r => r.Desk)
                .HasForeignKey(r => r.DeskId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Room>()
                .HasMany(r => r.Desks)
                .WithOne(d => d.Room)
                .HasForeignKey(d => d.RoomId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Room>().HasData(
                new Room { Id = 1, Name = "Open Space A" },
                new Room { Id = 2, Name = "Meeting Room 1" }
            );

            modelBuilder.Entity<Desk>().HasData(
                new Desk { Id = 1, DeskIdentifier = "A-101", RoomId = 1 },
                new Desk { Id = 2, DeskIdentifier = "A-102", RoomId = 1 },
                new Desk { Id = 3, DeskIdentifier = "M-01", RoomId = 2 }
            );

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    FirstName = "Roman",
                    LastName = "Buchynskyi",
                    Email = "roman.buchynskyi2006@gmail.com",
                    PasswordHash = "$2a$11$KYxIeRootPrk.RLtbBRBle5TRHsZG9l4zJ5Q2krvF16REoqjvGQHS", // Admin123!
                    Role = Role.Admin
                }
            );
        }
    }
}
