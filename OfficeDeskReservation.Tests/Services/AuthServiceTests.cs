using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using OfficeDeskReservation.API.Data;
using OfficeDeskReservation.API.Dtos.Auth;
using OfficeDeskReservation.API.Dtos.Users;
using OfficeDeskReservation.API.Mappings;
using OfficeDeskReservation.API.Models;
using OfficeDeskReservation.API.Services.Implementations;

namespace OfficeDeskReservation.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;

        public AuthServiceTests()
        {
            var config = new MapperConfiguration(opt => opt.AddProfile<AppMappingProfile>(), NullLoggerFactory.Instance);
            _mapper = config.CreateMapper();

            var inMemorySettings = new Dictionary<string, string?> {
                {"Jwt:Key", "SuperSecretKeyThatIsAtLeast64BytesLongForHmacSha512Signature!@#$"},
                {"Jwt:Issuer", "TestIssuer"},
                {"Jwt:Audience", "TestAudience"}
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
        }

        private AppDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        public async Task RegisterAsync_WhenEmailIsNotUnique_ShouldHaveError()
        {
            AppDbContext context = GetDbContext();
            AuthService service = new AuthService(context, _mapper, _configuration);

            context.Users.Add(new User
            {
                FirstName = "Dohn",
                LastName = "Dobrovolsky",
                Email = "praca@gmail.com",
                PasswordHash = "hash"
            });

            await context.SaveChangesAsync();

            RegisterDto registerDto = new RegisterDto
            {
                FirstName = "User",
                LastName = "User",
                Email = "praca@gmail.com",
                Password = "Password_1"
            };

            Func<Task> act = async () => await service.RegisterAsync(registerDto);

            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("User with this email already exists.");
        }

        [Fact]
        public async Task RegisterAsync_WhenValidData_ShouldCreateUserAndReturnUserResponseDto()
        {
            AppDbContext context = GetDbContext();
            AuthService service = new AuthService(context, _mapper, _configuration);

            RegisterDto registerDto = new RegisterDto
            {
                FirstName = "Jan",
                LastName = "Dobrovolsky",
                Email = "praca@gmail.com",
                Password = "Password12_3"
            };

            UserResponseDto result = await service.RegisterAsync(registerDto);

            result.Should().NotBeNull();
            result.Email.Should().Be("praca@gmail.com");
            result.Name.Should().Be("Jan Dobrovolsky");

            User? createdUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "praca@gmail.com");

            createdUser.Should().NotBeNull();
            createdUser.PasswordHash.Should().NotBe("Password12_3");
            BCrypt.Net.BCrypt.Verify("Password12_3", createdUser.PasswordHash).Should().BeTrue();
        }

        [Fact]
        public async Task LoginAsync_WhenEmailDoesNotExist_ShouldHaveError()
        {
            AppDbContext context = GetDbContext();
            AuthService service = new AuthService(context, _mapper, _configuration);

            context.Users.Add(new User
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "prac@gmai.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!")
            });

            await context.SaveChangesAsync();

            LoginDto loginDto = new LoginDto
            {
                Email = "praca@gmail.com",
                Password = "Password123!"
            };

            Func<Task> act = async () => await service.LoginAsync(loginDto);

            await act.Should()
                .ThrowAsync<KeyNotFoundException>()
                .WithMessage("Invalid email or password.");
        }

        [Fact]
        public async Task LoginAsync_WhenPasswordIsInvalid_ShouldHaveError()
        {
            AppDbContext context = GetDbContext();
            AuthService service = new AuthService(context, _mapper, _configuration);

            context.Users.Add(new User
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "praca@gmail.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!")
            });

            await context.SaveChangesAsync();

            LoginDto loginDto = new LoginDto
            {
                Email = "praca@gmail.com",
                Password = "Password123_"
            };

            Func<Task> act = async () => await service.LoginAsync(loginDto);

            await act.Should()
                .ThrowAsync<KeyNotFoundException>()
                .WithMessage("Invalid email or password.");
        }

        [Fact]
        public async Task LoginAsync_WhenValidCredentials_ShouldReturnJwtToken()
        {
            AppDbContext context = GetDbContext();
            AuthService service = new AuthService(context, _mapper, _configuration);

            context.Users.Add(new User
            {
                FirstName = "Jan",
                LastName = "Dobrovolsky",
                Email = "praca@gmail.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!")
            });

            await context.SaveChangesAsync();

            LoginDto loginDto = new LoginDto
            {
                Email = "praca@gmail.com",
                Password = "Password123!"
            };

            string token = await service.LoginAsync(loginDto);
            token.Should().NotBeNullOrWhiteSpace();
            token.Should().StartWith("eyJ");    
        }
    }
}
