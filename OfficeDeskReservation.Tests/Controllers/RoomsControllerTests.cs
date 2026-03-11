using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OfficeDeskReservation.API.Controllers;
using OfficeDeskReservation.API.Dtos.Rooms;
using OfficeDeskReservation.API.Pagination;
using OfficeDeskReservation.API.Services.Interfaces;

namespace OfficeDeskReservation.Tests.Controllers
{
    public class RoomsControllerTests
    {
        private readonly Mock<IRoomService> _mockRoomService;
        private readonly RoomsController _controller;

        public RoomsControllerTests()
        {
            _mockRoomService = new Mock<IRoomService>();
            _controller = new RoomsController(_mockRoomService.Object);
        }

        [Fact]
        public async Task GetRoomsAsync_WithParameters_ShouldOkWithReturnListOfRoomResponseDto()
        {
            RoomQueryParameters parameters = new RoomQueryParameters
            {
                PageNumber = 2,
                PageSize = 4
            };

            PagedResult<RoomResponseDto> expectedResult = new PagedResult<RoomResponseDto>
            {
                PageNumber = 2,
                PageSize = 4,
                TotalCount = 2,
                Items = new List<RoomResponseDto>
                {
                    new RoomResponseDto { Id = 1, Name = "Room A" },
                    new RoomResponseDto { Id = 2, Name = "Room B" }
                }
            };

            _mockRoomService
                .Setup(service => service.GetAllRoomsAsync(parameters))
                .ReturnsAsync(expectedResult);

            ActionResult<PagedResult<RoomResponseDto>> result = await _controller.GetRoomsAsync(parameters);
            OkObjectResult okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(expectedResult);
            _mockRoomService.Verify(service => service.GetAllRoomsAsync(parameters), Times.Once);
        }

        [Fact]
        public async Task GetRoomByIdAsync_WhenRoomExists_ShouldReturnOkWithRoomResponseDto()
        {
            RoomResponseDto expectedResult = new RoomResponseDto
            {
                Id = 1,
                Name = "Room A"
            };

            _mockRoomService
                .Setup(service => service.GetRoomByIdAsync(1))
                .ReturnsAsync(expectedResult);

            ActionResult<RoomResponseDto> result = await _controller.GetRoomByIdAsync(1);
            OkObjectResult okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(expectedResult);
            _mockRoomService.Verify(service => service.GetRoomByIdAsync(1), Times.Once);
        }

        [Fact]
        public async Task GetRoomByIdAsync_WhenRoomDoesNotExist_ShouldReturnNotFound()
        {
            _mockRoomService
                .Setup(service => service.GetRoomByIdAsync(1))
                .ReturnsAsync((RoomResponseDto?)null);

            ActionResult<RoomResponseDto> result = await _controller.GetRoomByIdAsync(1);
            result.Result.Should().BeOfType<NotFoundResult>();
            _mockRoomService.Verify(service => service.GetRoomByIdAsync(1), Times.Once);
        }

        [Fact]
        public async Task PostRoomAsync_WithValidData_ShouldReturnCreatedAtActionWithData()
        {
            RoomDto roomDto = new RoomDto { Name = "Room A" };
            RoomResponseDto expectedResult = new RoomResponseDto { Id = 1, Name = "Room A" };

            _mockRoomService
                .Setup(service => service.CreateRoomAsync(roomDto))
                .ReturnsAsync(expectedResult);

            ActionResult<RoomResponseDto> result = await _controller.PostRoomAsync(roomDto);
            CreatedAtActionResult createdAtActionResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;

            createdAtActionResult.ActionName.Should().Be("GetRoomById");
            createdAtActionResult.Value.Should().BeEquivalentTo(expectedResult);
            createdAtActionResult.RouteValues.Should().ContainKey("id").WhoseValue.Should().Be(expectedResult.Id);
            _mockRoomService.Verify(service => service.CreateRoomAsync(roomDto), Times.Once);
        }

        [Fact]
        public async Task DeleteByIdAsync_WhenRoomExists_ShouldReturnNoContent()
        {
            _mockRoomService
                .Setup(service => service.DeleteRoomAsync(1))
                .ReturnsAsync(true);

            IActionResult result = await _controller.DeleteByIdAsync(1);
            result.Should().BeOfType<NoContentResult>();
            _mockRoomService.Verify(service => service.DeleteRoomAsync(1), Times.Once);
        }

        [Fact]
        public async Task DeleteByIdAsync_WhenRoomDoesNotExist_ShouldReturnNotFound()
        {
            _mockRoomService
                .Setup(service => service.DeleteRoomAsync(1))
                .ReturnsAsync(false);

            IActionResult result = await _controller.DeleteByIdAsync(1);
            result.Should().BeOfType<NotFoundObjectResult>();
            _mockRoomService.Verify(service => service.DeleteRoomAsync(1), Times.Once);
        }

        [Fact]
        public async Task PutRoomAsync_WhenRoomExists_ShouldReturnNoContent()
        {
            RoomDto inputRoomDto = new RoomDto
            {
                Name = "Room A"
            };

            _mockRoomService
                .Setup(service => service.UpdateRoomAsync(1, inputRoomDto))
                .ReturnsAsync(true);

            IActionResult result = await _controller.PutRoomAsync(1, inputRoomDto);
            result.Should().BeOfType<NoContentResult>();
            _mockRoomService.Verify(service => service.UpdateRoomAsync(1, inputRoomDto), Times.Once);
        }

        [Fact]
        public async Task PutRoomAsync_WhenRoomDoesNotExist_ShouldReturnNotFound()
        {
            RoomDto inputRoomDto = new RoomDto
            {
                Name = "Room A"
            };

            _mockRoomService
                .Setup(service => service.UpdateRoomAsync(1, inputRoomDto))
                .ReturnsAsync(false);

            IActionResult result = await _controller.PutRoomAsync(1, inputRoomDto);
            result.Should().BeOfType<NotFoundObjectResult>();
            _mockRoomService.Verify(service => service.UpdateRoomAsync(1, inputRoomDto), Times.Once);
        }
    }
}
