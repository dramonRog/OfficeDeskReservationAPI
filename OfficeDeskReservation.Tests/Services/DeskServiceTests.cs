using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using OfficeDeskReservation.API.Data;
using OfficeDeskReservation.API.Dtos.Desks;
using OfficeDeskReservation.API.Mappings;
using OfficeDeskReservation.API.Models;
using OfficeDeskReservation.API.Pagination;
using OfficeDeskReservation.API.Services.Implementations;

namespace OfficeDeskReservation.Tests.Services
{
    public class DeskServiceTests
    {
        private readonly IMapper _mapper;

        public DeskServiceTests()
        {
            var config = new MapperConfiguration(opt => opt.AddProfile<AppMappingProfile>(), NullLoggerFactory.Instance);
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
        public async Task GetDeskByIdAsync_WhenDeskExists_ShouldReturnDeskDto()
        {
            AppDbContext context = GetDbContext();
            DeskService service = new DeskService(context, _mapper);

            Desk newDesk = new Desk 
            { 
                DeskIdentifier = "A101", 
                Room = new Room { Name = "A" } 
            };
            context.Desks.Add(newDesk);

            await context.SaveChangesAsync();

            DeskResponseDto? result = await service.GetDeskByIdAsync(newDesk.Id);

            result.Should().NotBeNull();
            result.DeskIdentifier.Should().Be("A101");
        }

        [Fact]
        public async Task GetDeskByIdAsync_WhenDeskDoesNotExist_ShouldReturnNull()
        {
            AppDbContext context = GetDbContext();
            DeskService service = new DeskService(context, _mapper);

            DeskResponseDto? result = await service.GetDeskByIdAsync(1);

            result.Should().BeNull();
        }

        [Fact]
        public async Task CreateDeskAsync_WhenDeskIdentifierIsNotUnique_ShouldHaveError()
        {
            AppDbContext context = GetDbContext();
            DeskService service = new DeskService(context, _mapper);

            context.Desks.Add(new Desk
            {
                DeskIdentifier = "A101"
            });

            await context.SaveChangesAsync();

            DeskDto deskDto = new DeskDto
            {
                DeskIdentifier = "A101"
            };

            Func<Task> act = async () => await service.CreateDeskAsync(deskDto);

            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("The desk with that name already exists.");
        }

        [Fact]
        public async Task CreateDeskAsync_WhenRoomDoesNotExist_ShouldHaveError()
        {
            AppDbContext context = GetDbContext();
            DeskService service = new DeskService(context, _mapper);

            DeskDto deskDto = new DeskDto
            {
                DeskIdentifier = "A1",
                RoomId = 1
            };

            Func<Task> act = async () => await service.CreateDeskAsync(deskDto);

            await act.Should()
                .ThrowAsync<KeyNotFoundException>()
                .WithMessage("The specified room doesn't exist.");
        }

        [Fact]
        public async Task CreateDeskAsync_WhenValidData_ShouldCreateDeskAndReturnDto()
        {
            AppDbContext context = GetDbContext();
            DeskService service = new DeskService(context, _mapper);

            Room room = new Room { Name = "Conference Room" };
            context.Rooms.Add(room);
            await context.SaveChangesAsync();

            DeskDto deskDto = new DeskDto
            {
                DeskIdentifier = "B200",
                RoomId = room.Id
            };

            var result = await service.CreateDeskAsync(deskDto);

            result.Should().NotBeNull();
            result.DeskIdentifier.Should().Be("B200");

            var deskInDb = await context.Desks.FirstOrDefaultAsync(d => d.DeskIdentifier == "B200");
            deskInDb.Should().NotBeNull();
            deskInDb.RoomId.Should().Be(room.Id);
        }

        [Fact]
        public async Task UpdateDeskAsync_WhenDeskDoesNotExist_ShouldBeFalse()
        {
            AppDbContext context = GetDbContext();
            DeskService service = new DeskService(context, _mapper);

            DeskDto deskDto = new DeskDto
            {
                DeskIdentifier = "A1"
            };

            bool result = await service.UpdateDeskAsync(1, deskDto);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateDeskAsync_WhenDeskIdentifierIsNotUnique_ShouldHaveError()
        {
            AppDbContext context = GetDbContext();
            DeskService service = new DeskService(context, _mapper);

            context.Desks.AddRange(
                new Desk { DeskIdentifier = "A1" },
                new Desk { DeskIdentifier = "A2" }
            );

            await context.SaveChangesAsync();

            DeskDto deskDto = new DeskDto
            {
                DeskIdentifier = "A2"
            };

            Func<Task> act = async () => await service.UpdateDeskAsync(1, deskDto);

            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("The desk with that name already exists.");
        }

        [Fact]
        public async Task UpdateDeskAsync_WhenRoomDoesNotExist_ShouldHaveError()
        {
            AppDbContext context = GetDbContext();
            DeskService service = new DeskService(context, _mapper);

            context.Desks.Add(new Desk
            {
                DeskIdentifier = "A1"
            });

            await context.SaveChangesAsync();

            DeskDto deskDto = new DeskDto
            {
                DeskIdentifier = "A2",
                RoomId = 1
            };

            Func<Task> act = async () => await service.UpdateDeskAsync(1, deskDto);

            await act.Should()
                .ThrowAsync<KeyNotFoundException>()
                .WithMessage("The specified room doesn't exist.");
        }

        [Fact]
        public async Task UpdateDeskAsync_WhenValidData_ShouldUpdateDeskAndReturnTrue()
        {
            AppDbContext context = GetDbContext();
            DeskService service = new DeskService(context, _mapper);

            Room room = new Room { Name = "Office" };
            Desk desk = new Desk { DeskIdentifier = "OldId", Room = room };
            context.Rooms.Add(room);
            context.Desks.Add(desk);
            await context.SaveChangesAsync();

            DeskDto deskDto = new DeskDto
            {
                DeskIdentifier = "NewId",
                RoomId = room.Id
            };

            bool result = await service.UpdateDeskAsync(desk.Id, deskDto);

            result.Should().BeTrue();

            var updatedDesk = await context.Desks.FirstOrDefaultAsync(d => d.Id == desk.Id);
            updatedDesk.Should().NotBeNull();
            updatedDesk!.DeskIdentifier.Should().Be("NewId");
        }

        [Fact]
        public async Task DeleteDeskAsync_WhenDeskDoesNotExist_ShouldBeFalse()
        {
            AppDbContext context = GetDbContext();
            DeskService service = new DeskService(context, _mapper);

            bool result = await service.DeleteDeskAsync(1);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteDeskAsync_WhenContainsReservations_ShouldHaveError()
        {
            AppDbContext context = GetDbContext();
            DeskService service = new DeskService(context, _mapper);

            Desk desk = new Desk
            {
                DeskIdentifier = "A1",
                Reservations = new List<Reservation> { new Reservation() }
            };
            context.Desks.Add(desk);
            await context.SaveChangesAsync();

            Func<Task> act = async () => await service.DeleteDeskAsync(desk.Id);

            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("This desk can't be deleted, as it contains reservations");
        }

        [Fact]
        public async Task DeleteDeskAsync_WhenDoesNotContainReservations_ShouldBeTrue()
        {
            AppDbContext context = GetDbContext();
            DeskService service = new DeskService(context, _mapper);

            context.Desks.Add(new Desk
            {
                DeskIdentifier = "A1"
            });

            await context.SaveChangesAsync();

            bool result = await service.DeleteDeskAsync(1);

            result.Should().BeTrue();

            Desk? deletedDesk = await context.Desks.FirstOrDefaultAsync(d => d.Id == 1);
            deletedDesk.Should().BeNull();
        }

        [Fact]
        public async Task GetDesksAsync_WithPaginationAndRoomFilter_ShouldReturnFilteredData()
        {
            AppDbContext context = GetDbContext();
            DeskService service = new DeskService(context, _mapper);

            Room room1 = new Room { Name = "Room 1" };
            Room room2 = new Room { Name = "Room 2" };
            context.Rooms.AddRange(room1, room2);
            await context.SaveChangesAsync();

            context.Desks.AddRange(
                new Desk { DeskIdentifier = "A1", RoomId = room1.Id },
                new Desk { DeskIdentifier = "A2", RoomId = room1.Id },
                new Desk { DeskIdentifier = "B1", RoomId = room2.Id }
            );
            await context.SaveChangesAsync();

            DeskQueryParameters parameters = new DeskQueryParameters
            {
                PageNumber = 1,
                PageSize = 10,
                RoomId = room1.Id
            };

            var result = await service.GetDesksAsync(parameters);

            result.Should().NotBeNull();
            result.TotalCount.Should().Be(2); 
            result.Items.Should().HaveCount(2);
            result.Items.Should().OnlyContain(d => d.DeskIdentifier.StartsWith("A"));
        }
    }
}
