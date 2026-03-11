using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OfficeDeskReservation.API.Controllers;
using OfficeDeskReservation.API.Dtos.Reservations;
using OfficeDeskReservation.API.Pagination;
using OfficeDeskReservation.API.Services.Interfaces;
using System.Security.Claims;

namespace OfficeDeskReservation.Tests.Controllers
{
    public class ReservationsControllerTests
    {
        private readonly Mock<IReservationService> _mockReservationService;
        private readonly ReservationsController _controller;

        public ReservationsControllerTests()
        {
            _mockReservationService = new Mock<IReservationService>();
            _controller = new ReservationsController(_mockReservationService.Object);
        }

        [Fact]
        public async Task GetAllReservationsAsync_WithParameters_ShouldReturnOkWithListOfReservationResponseDto()
        {
            ReservationQueryParameters parameters = new ReservationQueryParameters
            {
                PageNumber = 2,
                PageSize = 4
            };

            PagedResult<ReservationResponseDto> expectedResult = new PagedResult<ReservationResponseDto>
            {
                PageNumber = 2,
                PageSize = 4,
                TotalCount = 2,
                Items = new List<ReservationResponseDto>
                {
                    new ReservationResponseDto { Id = 1, DeskName = "A1", RoomName = "Room A" },
                    new ReservationResponseDto { Id = 2, DeskName = "A2", RoomName = "Room A" }
                }
            };

            _mockReservationService
                .Setup(service => service.GetAllReservationsAsync(parameters))
                .ReturnsAsync(expectedResult);

            ActionResult<PagedResult<ReservationResponseDto>> result = await _controller.GetAllReservationsAsync(parameters);
            OkObjectResult okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(expectedResult);
            _mockReservationService.Verify(service => service.GetAllReservationsAsync(parameters), Times.Once);
        }

        [Fact]
        public async Task GetReservationByIdAsync_WhenReservationExists_ShouldReturnOkWithReservationResponseDto()
        {
            ReservationResponseDto expectedResult = new ReservationResponseDto
            {
                Id = 1,
                DeskName = "A1",
                RoomName = "Room A",
                StartTime = DateTime.Now.AddHours(1),
                EndTime = DateTime.Now.AddHours(3)
            };

            _mockReservationService
                .Setup(service => service.GetReservationByIdAsync(1))
                .ReturnsAsync(expectedResult);

            ActionResult<ReservationResponseDto> result = await _controller.GetReservationByIdAsync(1);
            OkObjectResult okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            _mockReservationService.Verify(service => service.GetReservationByIdAsync(1), Times.Once);
        }

        [Fact]
        public async Task GetReservationByIdAsync_WhenReservationDoesNotExist_ShouldReturnNotFound()
        {
            _mockReservationService
                .Setup(service => service.GetReservationByIdAsync(1))
                .ReturnsAsync((ReservationResponseDto?)null);

            ActionResult<ReservationResponseDto> result = await _controller.GetReservationByIdAsync(1);
            result.Result.Should().BeOfType<NotFoundResult>();
            _mockReservationService.Verify(service => service.GetReservationByIdAsync(1), Times.Once);
        }

        [Fact]
        public async Task PostReservationAsync_WithValidData_ShouldReturnCreatedAtActionWithData()
        {
            ReservationDto inputReservationDto = new ReservationDto
            {
                StartTime = DateTime.Now.AddHours(2),
                EndTime = DateTime.Now.AddHours(4),
                DeskId = 1
            };

            ReservationResponseDto expectedResult = new ReservationResponseDto
            {
                Id = 1,
                StartTime = inputReservationDto.StartTime,
                EndTime = inputReservationDto.EndTime,
                UserId = 1
            };

            _mockReservationService
                .Setup(service => service.CreateReservationAsync(inputReservationDto, 1))
                .ReturnsAsync(expectedResult);

            List<Claim> claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "1") };
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth")) }
            };


            ActionResult<ReservationResponseDto> result = await _controller.PostReservationAsync(inputReservationDto);

            CreatedAtActionResult createdAtActionResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdAtActionResult.ActionName.Should().Be("GetReservationById");
            createdAtActionResult.Value.Should().BeEquivalentTo(expectedResult);
            createdAtActionResult.RouteValues.Should().ContainKey("id").WhoseValue.Should().Be(expectedResult.Id);
            _mockReservationService.Verify(service => service.CreateReservationAsync(inputReservationDto, 1), Times.Once);
        }

        [Fact]
        public async Task PutReservationAsync_WhenReservationDoesNotExist_ShouldReturnNotFound()
        {
            ReservationDto reservationDto = new ReservationDto
            {
                StartTime = DateTime.Now.AddHours(1),
                EndTime = DateTime.Now.AddHours(3)
            };

            _mockReservationService
                .Setup(service => service.UpdateReservationAsync(1, reservationDto))
                .ReturnsAsync(false);

            IActionResult result = await _controller.PutReservationAsync(1, reservationDto);
            result.Should().BeOfType<NotFoundResult>();

            _mockReservationService.Verify(service => service.GetReservationByIdAsync(1), Times.Once);
            _mockReservationService.Verify(service => service.UpdateReservationAsync(1, reservationDto), Times.Never);
        }

        [Fact]
        public async Task PutReservationAsync_WhenItIsUser_ShouldReturnForbid()
        {
            ReservationDto reservationDto = new ReservationDto
            {
                StartTime = DateTime.Now.AddHours(1),
                EndTime = DateTime.Now.AddHours(3)
            };

            ReservationResponseDto reservationResponseDto = new ReservationResponseDto 
            { 
                Id = 1, 
                UserId = 1 
            };

            _mockReservationService
                .Setup(service => service.GetReservationByIdAsync(1))
                .ReturnsAsync(reservationResponseDto);

            List<Claim> claims = new List<Claim> 
            { 
                new Claim(ClaimTypes.NameIdentifier, "2"), 
                new Claim(ClaimTypes.Role, API.Models.Role.User.ToString()) 
            };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims)) }
            };

            IActionResult result = await _controller.PutReservationAsync(1, reservationDto);

            result.Should().BeOfType<ForbidResult>();
            _mockReservationService.Verify(service => service.GetReservationByIdAsync(1), Times.Once);
            _mockReservationService.Verify(service => service.UpdateReservationAsync(1, reservationDto), Times.Never);
        }

        [Fact]
        public async Task PutReservationAsync_WhenReservationExists_ShouldReturnNoContent()
        {
            ReservationDto reservationDto = new ReservationDto
            {
                StartTime = DateTime.Now.AddHours(1),
                EndTime = DateTime.Now.AddHours(3)
            };

            ReservationResponseDto reservationResponseDto = new ReservationResponseDto 
            { 
                Id = 1, 
                UserId = 1 
            };

            _mockReservationService
                .Setup(service => service.GetReservationByIdAsync(1))
                .ReturnsAsync(reservationResponseDto);

            _mockReservationService
                .Setup(service => service.UpdateReservationAsync(1, reservationDto))
                .ReturnsAsync(true);

            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Role, API.Models.Role.Manager.ToString())
            };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims)) }
            };

            IActionResult result = await _controller.PutReservationAsync(1, reservationDto);

            result.Should().BeOfType<NoContentResult>();
            _mockReservationService.Verify(service => service.GetReservationByIdAsync(1), Times.Once);
            _mockReservationService.Verify(service => service.UpdateReservationAsync(1, reservationDto), Times.Once);
        }

        [Fact]
        public async Task RemoveReservationAsync_WhenReservationDoesNotExist_ShouldReturnNotFound()
        {
            _mockReservationService
                .Setup(service => service.DeleteReservationAsync(1))
                .ReturnsAsync(false);

            IActionResult result = await _controller.RemoveReservationAsync(1);
            result.Should().BeOfType<NotFoundResult>();

            _mockReservationService.Verify(service => service.GetReservationByIdAsync(1), Times.Once);
            _mockReservationService.Verify(service => service.DeleteReservationAsync(1), Times.Never);
        }

        [Fact]
        public async Task RemoveReservationAsync_WhenIsUser_ShouldReturnForbid()
        {
            ReservationResponseDto reservationResponseDto = new ReservationResponseDto
            {
                Id = 1,
                DeskName = "A1",
                RoomName = "Room A",
                StartTime = DateTime.Now.AddHours(1),
                EndTime = DateTime.Now.AddHours(3),
                UserId = 1
            };

            _mockReservationService
                .Setup(service => service.GetReservationByIdAsync(1))
                .ReturnsAsync(reservationResponseDto);

            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "2"),
                new Claim(ClaimTypes.Role, API.Models.Role.User.ToString())
            };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims)) }
            };

            IActionResult result = await _controller.RemoveReservationAsync(1);

            result.Should().BeOfType<ForbidResult>();
            _mockReservationService.Verify(service => service.GetReservationByIdAsync(1), Times.Once);
            _mockReservationService.Verify(service => service.DeleteReservationAsync(1), Times.Never);
        }

        [Fact]
        public async Task RemoveReservationAsync_WithValidData_ShouldReturnNoContent()
        {
            ReservationResponseDto reservationResponseDto = new ReservationResponseDto
            {
                Id = 1,
                DeskName = "A1",
                UserId = 1,
                StartTime = DateTime.Now.AddHours(1),
                EndTime = DateTime.Now.AddHours(3)
            };

            _mockReservationService
                .Setup(service => service.GetReservationByIdAsync(1))
                .ReturnsAsync(reservationResponseDto);

            _mockReservationService
                .Setup(service => service.DeleteReservationAsync(1))
                .ReturnsAsync(true);

            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Role, API.Models.Role.Manager.ToString())
            };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims)) }
            };

            IActionResult result = await _controller.RemoveReservationAsync(1);

            result.Should().BeOfType<NoContentResult>();
            _mockReservationService.Verify(service => service.DeleteReservationAsync(1), Times.Once);
            _mockReservationService.Verify(service => service.GetReservationByIdAsync(1), Times.Once);
        }
    }
}
