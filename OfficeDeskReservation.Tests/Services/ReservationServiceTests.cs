using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using OfficeDeskReservation.API.Data;
using OfficeDeskReservation.API.Dtos.Reservations;
using OfficeDeskReservation.API.Mappings;
using OfficeDeskReservation.API.Models;
using OfficeDeskReservation.API.Pagination;
using OfficeDeskReservation.API.Services.Implementations;

namespace OfficeDeskReservation.Tests.Services
{
    public class ReservationServiceTests
    {
        private readonly IMapper _mapper;

        public ReservationServiceTests()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<AppMappingProfile>(), NullLoggerFactory.Instance);
            _mapper = config.CreateMapper();
        }

        private AppDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        public async Task GetReservationByIdAsync_WhenExists_ShouldReturnReservationResponseDto()
        {
            AppDbContext context = GetDbContext();
            ReservationService service = new ReservationService(context, _mapper);

            context.Reservations.Add(new Reservation
            {
                StartTime = DateTime.Now.AddHours(1),
                EndTime = DateTime.Now.AddHours(3),
                User = new User { FirstName = "John", LastName = "Doe" },
                Desk = new Desk { DeskIdentifier = "A1", Room = new Room { Name = "Room A" } }
            });

            await context.SaveChangesAsync();

            ReservationResponseDto? result = await service.GetReservationByIdAsync(1);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task GetReservationByIdAsync_WhenDoesNotExist_ShouldBeNull()
        {
            AppDbContext context = GetDbContext();
            ReservationService service = new ReservationService(context, _mapper);

            context.Reservations.Add(new Reservation
            {
                StartTime = DateTime.Now.AddHours(1),
                EndTime = DateTime.Now.AddHours(3),
                User = new User { FirstName = "John", LastName = "Doe" },
                Desk = new Desk { DeskIdentifier = "A1", Room = new Room { Name = "Room A" } }
            });

            await context.SaveChangesAsync();

            ReservationResponseDto? result = await service.GetReservationByIdAsync(2);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetAllReservationsAsync_WithFilters_ShouldReturnFilteredData()
        {
            AppDbContext context = GetDbContext();
            ReservationService service = new ReservationService(context, _mapper);

            var user1 = new User { FirstName = "John", Email = "1@test.com" };
            var user2 = new User { FirstName = "Jane", Email = "2@test.com" };

            var room1 = new Room { Name = "Room 1" };
            var desk1 = new Desk { DeskIdentifier = "A1", Room = room1 };
            var desk2 = new Desk { DeskIdentifier = "A2", Room = room1 };

            context.Users.AddRange(user1, user2);
            context.Rooms.Add(room1);
            context.Desks.AddRange(desk1, desk2);
            await context.SaveChangesAsync();

            context.Reservations.AddRange(
                new Reservation { User = user1, Desk = desk1, StartTime = DateTime.Now, EndTime = DateTime.Now.AddHours(1) },
                new Reservation { User = user1, Desk = desk2, StartTime = DateTime.Now, EndTime = DateTime.Now.AddHours(1) },
                new Reservation { User = user2, Desk = desk1, StartTime = DateTime.Now.AddHours(2), EndTime = DateTime.Now.AddHours(3) }
            );
            await context.SaveChangesAsync();

            var parameters = new ReservationQueryParameters
            {
                PageNumber = 1,
                PageSize = 10,
                UserId = user1.Id
            };

            var result = await service.GetAllReservationsAsync(parameters);

            result.Should().NotBeNull();
            result.TotalCount.Should().Be(2);
        }

        [Fact]
        public async Task CreateReservationAsync_WhenUserDoesNotExist_ShouldThrowException()
        {
            AppDbContext context = GetDbContext();
            ReservationService service = new ReservationService(context, _mapper);

            var dto = new ReservationDto { DeskId = 1, StartTime = DateTime.Now, EndTime = DateTime.Now.AddHours(1) };

            Func<Task> act = async () => await service.CreateReservationAsync(dto, 1);

            await act.Should()
                .ThrowAsync<KeyNotFoundException>()
                .WithMessage("User from token not found in database.");
        }

        [Fact]
        public async Task CreateReservationAsync_WhenDeskDoesNotExist_ShouldThrowException()
        {
            AppDbContext context = GetDbContext();
            ReservationService service = new ReservationService(context, _mapper);

            var user = new User { FirstName = "John", Email = "test@test.com" };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var dto = new ReservationDto { DeskId = 999, StartTime = DateTime.Now, EndTime = DateTime.Now.AddHours(1) };

            Func<Task> act = async () => await service.CreateReservationAsync(dto, user.Id);

            await act.Should()
                .ThrowAsync<KeyNotFoundException>()
                .WithMessage("No desk with such ID.");
        }

        [Fact]
        public async Task CreateReservationAsync_WhenTimeOverlaps_ShouldThrowException()
        {
            AppDbContext context = GetDbContext();
            ReservationService service = new ReservationService(context, _mapper);

            var user = new User { FirstName = "John", Email = "test@test.com" };
            var room = new Room { Name = "Room" };
            var desk = new Desk { DeskIdentifier = "A1", Room = room };

            DateTime baseTime = DateTime.Now;

            context.Users.Add(user);
            context.Desks.Add(desk);
            
            context.Reservations.Add(new Reservation
            {
                User = user,
                Desk = desk,
                StartTime = baseTime,
                EndTime = baseTime.AddHours(2)
            });
            await context.SaveChangesAsync();

            var dto = new ReservationDto
            {
                DeskId = desk.Id,
                StartTime = baseTime.AddHours(1), 
                EndTime = baseTime.AddHours(3)   
            };

            Func<Task> act = async () => await service.CreateReservationAsync(dto, user.Id);

            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Desk is already taken for that period.");
        }

        [Fact]
        public async Task CreateReservationAsync_WhenValidData_ShouldCreateAndReturnDto()
        {
            AppDbContext context = GetDbContext();
            ReservationService service = new ReservationService(context, _mapper);

            var user = new User { FirstName = "John", Email = "test@test.com" };
            var room = new Room { Name = "Room" };
            var desk = new Desk { DeskIdentifier = "A1", Room = room };

            context.Users.Add(user);
            context.Desks.Add(desk);
            await context.SaveChangesAsync();

            var dto = new ReservationDto
            {
                DeskId = desk.Id,
                StartTime = DateTime.Now,
                EndTime = DateTime.Now.AddHours(2)
            };

            var result = await service.CreateReservationAsync(dto, user.Id);

            result.Should().NotBeNull();
            var savedReservation = await context.Reservations.FirstOrDefaultAsync();
            savedReservation.Should().NotBeNull();
            savedReservation!.UserId.Should().Be(user.Id);
            savedReservation.DeskId.Should().Be(desk.Id);
        }

        [Fact]
        public async Task UpdateReservationAsync_WhenReservationDoesNotExist_ShouldReturnFalse()
        {
            AppDbContext context = GetDbContext();
            ReservationService service = new ReservationService(context, _mapper);

            var dto = new ReservationDto { DeskId = 1, StartTime = DateTime.Now, EndTime = DateTime.Now.AddHours(1) };
            bool result = await service.UpdateReservationAsync(1, dto);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateReservationAsync_WhenTimeOverlapsWithAnother_ShouldThrowException()
        {
            AppDbContext context = GetDbContext();
            ReservationService service = new ReservationService(context, _mapper);

            var user = new User { FirstName = "John", Email = "test@test.com" };
            var room = new Room { Name = "Room" };
            var desk = new Desk { DeskIdentifier = "A1", Room = room };

            DateTime baseTime = DateTime.Now;

            context.Users.Add(user);
            context.Desks.Add(desk);

            var res1 = new Reservation { User = user, Desk = desk, StartTime = baseTime, EndTime = baseTime.AddHours(2) };
            var res2 = new Reservation { User = user, Desk = desk, StartTime = baseTime.AddHours(4), EndTime = baseTime.AddHours(6) };

            context.Reservations.AddRange(res1, res2);
            await context.SaveChangesAsync();

            var dto = new ReservationDto
            {
                DeskId = desk.Id,
                StartTime = baseTime.AddHours(1),
                EndTime = baseTime.AddHours(3)
            };

            Func<Task> act = async () => await service.UpdateReservationAsync(res2.Id, dto);

            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Desk is already taken for the selected period.");
        }

        [Fact]
        public async Task UpdateReservationAsync_WhenValidData_ShouldUpdateAndReturnTrue()
        {
            AppDbContext context = GetDbContext();
            ReservationService service = new ReservationService(context, _mapper);

            var user = new User { FirstName = "John", Email = "test@test.com" };
            var room = new Room { Name = "Room" };
            var desk = new Desk { DeskIdentifier = "A1", Room = room };

            var res = new Reservation { User = user, Desk = desk, StartTime = DateTime.Now, EndTime = DateTime.Now.AddHours(1) };

            context.Users.Add(user);
            context.Desks.Add(desk);
            context.Reservations.Add(res);
            await context.SaveChangesAsync();

            DateTime newStart = DateTime.Now.AddDays(1);
            DateTime newEnd = DateTime.Now.AddDays(1).AddHours(2);

            var dto = new ReservationDto { DeskId = desk.Id, StartTime = newStart, EndTime = newEnd };

            bool result = await service.UpdateReservationAsync(res.Id, dto);

            result.Should().BeTrue();
            var updatedRes = await context.Reservations.FirstAsync();
            updatedRes.StartTime.Should().Be(newStart);
            updatedRes.EndTime.Should().Be(newEnd);
        }

        [Fact]
        public async Task DeleteReservationAsync_WhenReservationExists_ShouldReturnTrueAndRemove()
        {
            AppDbContext context = GetDbContext();
            ReservationService service = new ReservationService(context, _mapper);

            var res = new Reservation { StartTime = DateTime.Now, EndTime = DateTime.Now.AddHours(1) };
            context.Reservations.Add(res);
            await context.SaveChangesAsync();

            bool result = await service.DeleteReservationAsync(res.Id);

            result.Should().BeTrue();
            var checkRes = await context.Reservations.FirstOrDefaultAsync(r => r.Id == res.Id);
            checkRes.Should().BeNull();
        }
    }
}
