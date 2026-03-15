
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OfficeDeskReservation.API.Controllers;
using OfficeDeskReservation.API.Dtos.Auth;
using OfficeDeskReservation.API.Dtos.Users;
using OfficeDeskReservation.API.Models;
using OfficeDeskReservation.API.Services.Interfaces;

namespace OfficeDeskReservation.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _mockAuthService = new Mock<IAuthService>();
            _controller = new AuthController(_mockAuthService.Object);
        }

        [Fact]
        public async Task RegisterAsync_WhenValidRequest_ShouldReturnOkWithUserResponse()
        {
            RegisterDto registerDto = new RegisterDto
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "praca@gmail.com",
                Password = "Password123!"
            };

            UserResponseDto expectedResponse = new UserResponseDto
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "praca@gmail.com",
                Role = Role.User
            };

            _mockAuthService
                .Setup(service => service.RegisterAsync(registerDto))
                .ReturnsAsync(expectedResponse);

            ActionResult<UserResponseDto> result = await _controller.RegisterAsync(registerDto);

            OkObjectResult okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(expectedResponse);
            _mockAuthService.Verify(service => service.RegisterAsync(registerDto), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_WhenValidRequest_SouldReturnOkWithToken()
        {
            LoginDto loginDto = new LoginDto
            {
                Email = "praca@gmail.com",
                Password = "Password123!"
            };

            string expectedToken = "eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9JhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9";

            _mockAuthService
                .Setup(service => service.LoginAsync(loginDto))
                .ReturnsAsync(expectedToken);

            ActionResult<string> result = await _controller.LoginAsync(loginDto);

            OkObjectResult okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(new { Token = expectedToken });
            _mockAuthService.Verify(service => service.LoginAsync(loginDto), Times.Once);
        }
    }
}
