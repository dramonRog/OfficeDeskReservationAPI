using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OfficeDeskReservation.API.Controllers;
using OfficeDeskReservation.API.Dtos.Users;
using OfficeDeskReservation.API.Pagination;
using OfficeDeskReservation.API.Services.Interfaces;
using System.Security.Claims;

namespace OfficeDeskReservation.Tests.Controllers
{
    public class UsersControllerTests
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly UsersController _controller;

        public UsersControllerTests()
        {
            _mockUserService = new Mock<IUserService>();
            _controller = new UsersController(_mockUserService.Object);
        }

        [Fact]
        public async Task GetUsersAsync_WithQueryParameters_ShouldReturnOkObjectWithListOfUserResponseDto()
        {
            UserQueryParameters inputParameters = new UserQueryParameters
            {
                PageNumber = 1,
                PageSize = 4
            };

            PagedResult<UserResponseDto> expectedResult = new PagedResult<UserResponseDto>
            {
                PageNumber = 1,
                PageSize = 4,
                TotalCount = 3,
                Items = new List<UserResponseDto>
                {
                    new UserResponseDto { Id = 1, FirstName = "John", LastName = "Doe", Email = "praca1@gmail.com" },
                    new UserResponseDto { Id = 2, FirstName = "Jan", LastName = "Kovalski", Email = "praca2@gmail.com" },
                    new UserResponseDto { Id = 3, FirstName = "Ja", LastName = "Dobrovolski", Email = "praca3@gmail.com" }
                }
            };

            _mockUserService
                .Setup(service => service.GetUsersAsync(inputParameters))
                .ReturnsAsync(expectedResult);

            ActionResult<PagedResult<UserResponseDto>> result = await _controller.GetUsersAsync(inputParameters);
            OkObjectResult okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(expectedResult);
            _mockUserService.Verify(service => service.GetUsersAsync(inputParameters), Times.Once);
        }

        [Fact]
        public async Task GetUserByIdAsync_WhenExists_ShouldReturnOkObjectWithUserResponseDto()
        {
            UserResponseDto expectedResult = new UserResponseDto
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "praca@gmail.com"
            };

            _mockUserService
                .Setup(service => service.GetUserByIdAsync(1))
                .ReturnsAsync(expectedResult);

            ActionResult<UserResponseDto> result = await _controller.GetUserByIdAsync(1);
            OkObjectResult okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(expectedResult);
            _mockUserService.Verify(service => service.GetUserByIdAsync(1), Times.Once);
        }

        [Fact]
        public async Task GetUserByIdAsync_WhenDoesNotExist_ShouldReturnNotFound()
        {
            _mockUserService
                .Setup(service => service.GetUserByIdAsync(1))
                .ReturnsAsync((UserResponseDto?)null);

            ActionResult<UserResponseDto> result = await _controller.GetUserByIdAsync(1);
            NotFoundResult notFoundResult = result.Result.Should().BeOfType<NotFoundResult>().Subject;
            _mockUserService.Verify(service => service.GetUserByIdAsync(1), Times.Once);
        }

        [Fact]
        public async Task PutUserAsync_WhenDataValid_ShouldReturnNoContent()
        {
            SetupUserContext(1);

            UserDto inputUserDto = new UserDto
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "praca@gmail.com"
            };

            _mockUserService
                .Setup(service => service.UpdateUserAsync(1, inputUserDto))
                .ReturnsAsync(true);

            IActionResult result = await _controller.PutUserAsync(1, inputUserDto);
            result.Should().BeOfType<NoContentResult>();
            _mockUserService.Verify(service => service.UpdateUserAsync(1, inputUserDto), Times.Once);
        }

        [Fact]
        public async Task PutUserAsync_WhenUserDoesNotExist_ShouldReturnNotFound()
        {
            SetupUserContext(1);

            UserDto inputUserDto = new UserDto
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "praca@gmail.com"
            };

            _mockUserService
                .Setup(service => service.UpdateUserAsync(1, inputUserDto))
                .ReturnsAsync(false);

            IActionResult result = await _controller.PutUserAsync(1, inputUserDto);
            result.Should().BeOfType<NotFoundResult>();
            _mockUserService.Verify(service => service.UpdateUserAsync(1, inputUserDto), Times.Once);
        }

        [Fact]
        public async Task RemoveUserByIdAsync_WhenUserDoesNotExist_ShouldReturnNotFound()
        {
            _mockUserService
                .Setup(service => service.DeleteUserAsync(1))
                .ReturnsAsync(false);

            IActionResult result = await _controller.RemoveUserByIdAsync(1);
            result.Should().BeOfType<NotFoundResult>();
            _mockUserService.Verify(service => service.DeleteUserAsync(1), Times.Once);
        }

        [Fact]
        public async Task RemoveUserByIdAsync_WhenUserExists_ShouldReturnNoContent()
        {
            _mockUserService
                .Setup(service => service.DeleteUserAsync(1))
                .ReturnsAsync(true);

            IActionResult result = await _controller.RemoveUserByIdAsync(1);
            result.Should().BeOfType<NoContentResult>();
            _mockUserService.Verify(service => service.DeleteUserAsync(1), Times.Once);
        }

        private void SetupUserContext(int userId)
        {
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var user = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }
    }
}
