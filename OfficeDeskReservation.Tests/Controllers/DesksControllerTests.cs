using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using OfficeDeskReservation.API.Controllers;
using OfficeDeskReservation.API.Dtos.Desks;
using OfficeDeskReservation.API.Pagination;
using OfficeDeskReservation.API.Services.Implementations;
using OfficeDeskReservation.API.Services.Interfaces;

namespace OfficeDeskReservation.Tests.Controllers
{
    public class DesksControllerTests
    {
        private readonly Mock<IDeskService> _mockDeskService;
        private readonly DesksController _controller;

        public DesksControllerTests()
        {
            _mockDeskService = new Mock<IDeskService>();
            _controller = new DesksController(_mockDeskService.Object);
        }

        [Fact]
        public async Task GetDesksAsync_WithParameters_ShouldReturnOkWithListOfDesks()
        {
            DeskQueryParameters parameters = new DeskQueryParameters
            {
                PageNumber = 1,
                PageSize = 4
            };

            PagedResult<DeskResponseDto> expectedResult = new PagedResult<DeskResponseDto>
            {
                PageNumber = 1,
                PageSize = 3,
                TotalCount = 2,
                Items = new List<DeskResponseDto>
                {
                    new DeskResponseDto { Id = 1, DeskIdentifier = "A1" },
                    new DeskResponseDto { Id = 2, DeskIdentifier = "A2" }
                }
            };

            _mockDeskService
                .Setup(service => service.GetDesksAsync(parameters))
                .ReturnsAsync(expectedResult);

            ActionResult<PagedResult<DeskResponseDto>> result = await _controller.GetDesksAsync(parameters);
            OkObjectResult okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(expectedResult);
            _mockDeskService.Verify(service => service.GetDesksAsync(parameters), Times.Once);
        }

        [Fact]
        public async Task GetDeskByIdAsync_WhenDeskExists_ShouldReturnOkWithDeskResponseDto()
        {
            DeskResponseDto expectedResult = new DeskResponseDto
            {
                Id = 1,
                DeskIdentifier = "A1"
            };

            _mockDeskService
                .Setup(service => service.GetDeskByIdAsync(1))
                .ReturnsAsync(expectedResult);

            ActionResult<DeskResponseDto> result = await _controller.GetDeskByIdAsync(1);
            OkObjectResult okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(expectedResult);
            _mockDeskService.Verify(service => service.GetDeskByIdAsync(1), Times.Once);
        }

        [Fact]
        public async Task GetDeskByIdAsync_WhenDeskDoesNotExist_ShouldReturnNotFound()
        {
            _mockDeskService
                .Setup(service => service.GetDeskByIdAsync(1))
                .ReturnsAsync((DeskResponseDto)null);

            ActionResult<DeskResponseDto> result = await _controller.GetDeskByIdAsync(1);
            NotFoundResult okResult = result.Result.Should().BeOfType<NotFoundResult>().Subject;
            _mockDeskService.Verify(service => service.GetDeskByIdAsync(1), Times.Once);
        }

        [Fact]
        public async Task PostDeskAsync_WhenValidData_ShouldReturnCreatedAtAction()
        {
            DeskDto inputDeskDto = new DeskDto
            {
                DeskIdentifier = "A1"
            };

            DeskResponseDto expectedResult = new DeskResponseDto
            {
                Id = 1,
                DeskIdentifier = "A1",
            };

            _mockDeskService
                .Setup(service => service.CreateDeskAsync(inputDeskDto))
                .ReturnsAsync(expectedResult);

            ActionResult<DeskResponseDto> result = await _controller.PostDeskAsync(inputDeskDto);
            
            CreatedAtActionResult createdAtActionResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdAtActionResult.ActionName.Should().Be("GetDeskById");
            createdAtActionResult.Value.Should().BeEquivalentTo(expectedResult);
            createdAtActionResult.RouteValues.Should().ContainKey("id").WhoseValue.Should().Be(expectedResult.Id);
            _mockDeskService.Verify(service => service.CreateDeskAsync(inputDeskDto), Times.Once);
        }

        [Fact]
        public async Task PutDeskAsync_WithValidData_ShouldReturnNoContent()
        {
            DeskDto inputDeskDto = new DeskDto
            {
                DeskIdentifier = "A1"
            };

            _mockDeskService
                .Setup(service => service.UpdateDeskAsync(1, inputDeskDto))
                .ReturnsAsync(true);

            IActionResult result = await _controller.PutDeskAsync(1, inputDeskDto);
            result.Should().BeOfType<NoContentResult>();
            _mockDeskService.Verify(service => service.UpdateDeskAsync(1, inputDeskDto), Times.Once);
        }

        [Fact]
        public async Task PutDeskAsync_WhenDeskDoesNotExist_ShouldReturnNoContent()
        {
            DeskDto inputDeskDto = new DeskDto
            {
                DeskIdentifier = "A1"
            };

            _mockDeskService
                .Setup(service => service.UpdateDeskAsync(1, inputDeskDto))
                .ReturnsAsync(false);

            IActionResult result = await _controller.PutDeskAsync(1, inputDeskDto);
            result.Should().BeOfType<NotFoundResult>();
            _mockDeskService.Verify(service => service.UpdateDeskAsync(1, inputDeskDto), Times.Once);
        }
    }
}
