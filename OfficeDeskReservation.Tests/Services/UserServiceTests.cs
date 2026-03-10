using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using OfficeDeskReservation.API.Data;
using OfficeDeskReservation.API.Dtos.Users;
using OfficeDeskReservation.API.Mappings;
using OfficeDeskReservation.API.Models;
using OfficeDeskReservation.API.Pagination;
using OfficeDeskReservation.API.Services.Implementations;

namespace OfficeDeskReservation.Tests.Services
{
    public class UserServiceTests
    {
        private readonly IMapper _mapper;

        public UserServiceTests()
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
        public async Task GetUsersAsync_WithPagination_ShouldReturnCorrectPage()
        {
            AppDbContext context = GetDbContext();
            UserService service = new UserService(context, _mapper);

            context.Users.AddRange(
                new User { FirstName = "Ivan", LastName = "Ivanov", Email = "praca1@gmail.com" },
                new User { FirstName = "John", LastName = "Doe", Email = "praca2@gmail.com" },
                new User { FirstName = "Jan", LastName = "Dobrovolsky", Email = "praca3@gmail.com" }
            );

            await context.SaveChangesAsync();

            UserQueryParameters parameters = new UserQueryParameters
            {
                PageNumber = 2,
                PageSize = 2
            };

            var result = await service.GetUsersAsync(parameters);

            result.Should().NotBeNull();
            result.TotalCount.Should().Be(3);
            result.Items.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetUsersAsync_WithFilters_ShouldReturnFilteredData()
        {
            AppDbContext context = GetDbContext();
            UserService service = new UserService(context, _mapper);

            context.Users.AddRange(
                new User { FirstName = "Anna", LastName = "Smith", Email = "anna@test.com" },
                new User { FirstName = "Anton", LastName = "Doe", Email = "anton@test.com" },
                new User { FirstName = "John", LastName = "Smith", Email = "john@test.com" }
            );

            await context.SaveChangesAsync();

            UserQueryParameters parameters = new UserQueryParameters
            {
                PageNumber = 1,
                PageSize = 10,
                FirstNameTerm = "An",
                LastNameTerm = "Sm"
            };

            var result = await service.GetUsersAsync(parameters);

            result.Should().NotBeNull();
            result.TotalCount.Should().Be(1);
            result.Items.First().Name.Should().Be("Anna Smith");
        }

        [Fact]
        public async Task GetUserByIdAsync_WhenUserExists_ShouldReturnUserResponseDto()
        {
            AppDbContext context = GetDbContext();
            UserService service = new UserService(context, _mapper);

            context.Users.Add(new User
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "praca@gmail.com",
            });

            await context.SaveChangesAsync();

            UserResponseDto? result = await service.GetUserByIdAsync(1);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task GetUserByIdAsync_WhenUserDoesNotExist_ShouldReturnNull()
        {
            AppDbContext context = GetDbContext();
            UserService service = new UserService(context, _mapper);

            UserResponseDto? result = await service.GetUserByIdAsync(1);

            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateUserAsync_WhenUserDoesNotExist_ShouldBeFalse()
        {
            AppDbContext context = GetDbContext();
            UserService service = new UserService(context, _mapper);

            UserDto userDto = new UserDto
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "praca@gmail.com"
            };

            bool result = await service.UpdateUserAsync(1, userDto);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateUserAsync_WhenEmailIsNotUnique_ShouldHaveError()
        {
            AppDbContext context = GetDbContext();
            UserService service = new UserService(context, _mapper);

            context.AddRange(
                new User
                {
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "praca@gmail.com"
                },

                new User
                {
                    Email = "praca1@gmail.com"
                }
            );

            await context.SaveChangesAsync();

            UserDto userDto = new UserDto
            {
                FirstName = "Jan",
                LastName = "Dobrovolsky",
                Email = "praca@gmail.com"
            };

            Func<Task> act = async () => await service.UpdateUserAsync(2, userDto);

            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Another user with this email already exists.");
        }

        [Fact]
        public async Task UpdateUserAsync_WhenUserNameAndSurnameAlreadyExist_ShouldHaveError()
        {
            AppDbContext context = GetDbContext();
            UserService service = new UserService(context, _mapper);

            context.Users.AddRange(
                new User
                {
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "praca1@gmai.com"
                },

                new User
                {
                    FirstName = "Jan",
                    LastName = "Dobrovolsky",
                    Email = "praca@gmail.com"
                }
            );

            await context.SaveChangesAsync();

            UserDto userDto = new UserDto
            {
                FirstName = "Jan",
                LastName = "Dobrovolsky"
            };

            Func<Task> act = async () => await service.UpdateUserAsync(1, userDto);

            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("The user with such name and surname already exists.");
        }

        [Fact]
        public async Task UpdateUserAsync_WhenDataAreValid_ShouldUpdateDataAndReturnTrue()
        {
            AppDbContext context = GetDbContext();
            UserService service = new UserService(context, _mapper);

            context.Users.Add(new User
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "praca@gmail.com"
            });

            await context.SaveChangesAsync();

            UserDto userDto = new UserDto
            {
                FirstName = "Jan",
                LastName = "Dobrovolsky",
                Email = "praca@gmail.com"
            };

            bool result = await service.UpdateUserAsync(1, userDto);

            result.Should().BeTrue();

            User? updatedUser = await context.Users.FirstAsync();

            updatedUser.Should().NotBeNull();
            updatedUser.FirstName.Should().Be("Jan");
            updatedUser.LastName.Should().Be("Dobrovolsky");
        }

        [Fact]
        public async Task DeleteUserAsync_WhenUserDoesNotExist_ShouldBeFalse()
        {
            AppDbContext context = GetDbContext();
            UserService service = new UserService(context, _mapper);

            bool result = await service.DeleteUserAsync(1);
            result.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteUserAsync_WhenUserExists_ShouldDeleteAndBeTrue()
        {
            AppDbContext context = GetDbContext();
            UserService service = new UserService(context, _mapper);

            context.Users.Add(new User
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "praca@gmail.com"
            });

            await context.SaveChangesAsync();

            bool result = await service.DeleteUserAsync(1);

            result.Should().BeTrue();

            User? deletedUser = await context.Users.FirstOrDefaultAsync();

            deletedUser.Should().BeNull();
        }

        [Fact]
        public async Task DeleteUserAsync_WhenUserHasReservations_ShouldDeleteUserAndReservations()
        {
            AppDbContext context = GetDbContext();
            UserService service = new UserService(context, _mapper);

            User user = new User
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john@test.com",
                Reservations = new List<Reservation>
                {
                    new Reservation { StartTime = DateTime.Now }
                }
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            bool result = await service.DeleteUserAsync(user.Id);

            result.Should().BeTrue();

            User? deletedUser = await context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
            deletedUser.Should().BeNull();

            int reservationsCountAfter = await context.Reservations.CountAsync();
            reservationsCountAfter.Should().Be(0);
        }
    }
}
