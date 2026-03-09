using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using OfficeDeskReservation.API.Data;
using OfficeDeskReservation.API.Dtos.Rooms;
using OfficeDeskReservation.API.Mappings;
using OfficeDeskReservation.API.Models;
using OfficeDeskReservation.API.Pagination;
using OfficeDeskReservation.API.Services.Implementations;

namespace OfficeDeskReservation.Tests.Services
{
    public class RoomServiceTests
    {
        private readonly IMapper _mapper;

        public RoomServiceTests()
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
        public async Task CreateRoomAsync_WhenValidRoom_ShouldSaveToDatabase()
        {
            AppDbContext context = GetDbContext();
            RoomService service = new RoomService(context, _mapper);
            RoomDto roomDto = new RoomDto { Name = "Room A" };

            RoomResponseDto? result = await service.CreateRoomAsync(roomDto);

            result.Should().NotBeNull();
            result.Name.Should().Be("Room A");

            Room? roomInDb = await context.Rooms.FirstOrDefaultAsync(r => r.Name == roomDto.Name);
            roomInDb.Should().NotBeNull();
            roomInDb.Name.Should().Be("Room A");
        }

        [Fact]
        public async Task CreateRoomAsync_WhenRoomNameIsNotUnique_ShouldHaveError()
        {
            AppDbContext context = GetDbContext();
            RoomService service = new RoomService(context, _mapper);

            context.Rooms.Add(new Room
            {
                Name = "Room A"
            });

            await context.SaveChangesAsync();

            RoomDto roomDto = new RoomDto
            {
                Name = "Room A"
            };

            Func<Task> act = async () => await service.CreateRoomAsync(roomDto);

            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("A room with that name already exists.");
        }

        [Fact]
        public async Task GetRoomsAsync_WhenRoomExists_ShouldReturnRoomResponseDto()
        {
            AppDbContext context = GetDbContext();
            RoomService service = new RoomService(context, _mapper);

            context.Rooms.AddRange(
                new Room { Name = "Room 1" },
                new Room { Name = "Room 2" },
                new Room { Name = "Room 3" }
            );

            await context.SaveChangesAsync();

            RoomQueryParameters parameters = new RoomQueryParameters { PageNumber = 2, PageSize = 2 };
            var result = await service.GetAllRoomsAsync(parameters);

            result.Should().NotBeNull();
            result.TotalCount.Should().Be(3);
            result.Items.Should().HaveCount(1);
            result.Items.First().Name.Should().Be("Room 3");
        }

        [Fact]
        public async Task GetRoomsAsync_WithSearchTerm_ShouldReturnFilteredRooms()
        {
            var context = GetDbContext();
            var service = new RoomService(context, _mapper);

            context.Rooms.AddRange(
                new Room { Name = "Alpha Room" },
                new Room { Name = "Beta Room" },
                new Room { Name = "Alpha Coference" }
            );

            await context.SaveChangesAsync();

            RoomQueryParameters parameters = new RoomQueryParameters
            {
                PageNumber = 1,
                PageSize = 10,
                SearchTerm = "Alpha"
            };

            var result = await service.GetAllRoomsAsync(parameters);

            result.Should().NotBeNull();
            result.TotalCount.Should().Be(2);
            result.Items.Should().HaveCount(2);
            result.Items.First().Name.Should().Be("Alpha Room");
            result.Items.Last().Name.Should().Be("Alpha Coference");
        }

        [Fact]
        public async Task GetRoomByIdAsync_WhenRoomExists_ShouldReturnRoomResponseDto()
        {
            AppDbContext context = GetDbContext();
            RoomService service = new RoomService(context, _mapper);

            Room newRoom = new Room { Name = "Room 1" };

            context.Add(newRoom);
            await context.SaveChangesAsync();

            RoomResponseDto? result = await service.GetRoomByIdAsync(newRoom.Id);

            result.Should().NotBeNull();
            result.Name.Should().Be("Room 1");
        }

        [Fact]
        public async Task GetRoomByIdAsync_WhenRoomDoesNotExist_ShouldReturnNull()
        {
            AppDbContext context = GetDbContext();
            RoomService service = new RoomService(context, _mapper);

            int testId = 1;
            RoomResponseDto? result = await service.GetRoomByIdAsync(testId);

            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateRoomAsync_WhenRoomWithSuchNameAlreadyExists_ShouldHaveError()
        {
            AppDbContext context = GetDbContext();
            RoomService service = new RoomService(context, _mapper);

            context.AddRange(
                new Room { Name = "Room A" }, 
                new Room { Name = "Room B" }
            );

            await context.SaveChangesAsync();

            RoomDto roomDto = new RoomDto
            {
                Name = "Room B"
            };

            int id = 1;

            Func<Task> act = async () => await service.UpdateRoomAsync(id, roomDto);

            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("A room with that name already exists.");
        }

        [Fact]
        public async Task UpdateRoomAsync_WhenRoomNameIsUnique_ShouldBeTrue()
        {
            AppDbContext context = GetDbContext();
            RoomService service = new RoomService(context, _mapper);

            context.AddRange(
                new Room { Name = "Room A" },
                new Room { Name = "Room B" }
            );

            await context.SaveChangesAsync();

            RoomDto roomDto = new RoomDto
            {
                Name = "Room C"
            };

            int id = 1;

            bool result = await service.UpdateRoomAsync(id, roomDto);

            result.Should().BeTrue();

            Room? updatedRoom = await context.Rooms.FirstOrDefaultAsync(r => r.Id == id);

            updatedRoom.Should().NotBeNull();
            updatedRoom.Name.Should().Be("Room C");
        }

        [Fact]
        public async Task UpdateRoomAsync_WhenRoomDoesNotExist_ShouldBeFalse()
        {
            AppDbContext context = GetDbContext();
            RoomService service = new RoomService(context, _mapper);

            RoomDto roomDto = new RoomDto
            {
                Name = "Room A"
            };

            bool result = await service.UpdateRoomAsync(1, roomDto);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteRoomAsync_WhenDoesNotExist_ShouldBeFalse()
        {
            AppDbContext context = GetDbContext();
            RoomService service = new RoomService(context, _mapper);

            bool result = await service.DeleteRoomAsync(1);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteRoomAsync_WhenContainsAnyDesk_ShouldHaveError()
        {
            AppDbContext context = GetDbContext();
            RoomService service = new RoomService(context, _mapper);

            context.Rooms.Add(new Room
            {
                Name = "Room A",
                Desks = new List<Desk>
                {
                    new Desk
                    {
                        DeskIdentifier = "Desk A1"
                    }
                }
            });

            await context.SaveChangesAsync();

            Func<Task> act = async () => await service.DeleteRoomAsync(1);

            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("This room cannot be deleted, as it contains desks.");
        }

        [Fact]
        public async Task DeleteRoomAsync_WhenExistAndDoesNotContainDesk_ShouldBeTrue()
        {
            AppDbContext context = GetDbContext();
            RoomService service = new RoomService(context, _mapper);

            context.Rooms.Add(new Room
            {
                Name = "Room A"
            });

            await context.SaveChangesAsync();

            bool result = await service.DeleteRoomAsync(1);

            result.Should().BeTrue();

            Room? deletedRoom = await context.Rooms.FirstOrDefaultAsync(r => r.Id == 1);
            deletedRoom.Should().BeNull();
        }
    }
}
