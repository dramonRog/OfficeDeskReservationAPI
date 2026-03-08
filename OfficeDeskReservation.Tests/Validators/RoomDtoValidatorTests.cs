using FluentAssertions;
using FluentValidation.Results;
using OfficeDeskReservation.API.Dtos.Rooms;
using OfficeDeskReservation.API.Validators;

namespace OfficeDeskReservation.Tests.Validators
{
    public class RoomDtoValidatorTests
    {
        private readonly RoomDtoValidator _validator;

        public RoomDtoValidatorTests()
        {
            _validator = new RoomDtoValidator();
        }

        [Fact]
        public void Validate_WhenRoomNameIsEmpty_ShouldHaveValidatorError()
        {
            RoomDto testRoomDto = new RoomDto
            {
                Name = ""
            };

            ValidationResult result = _validator.Validate(testRoomDto);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == "Room name can't be empty!");
        }

        [Theory]
        [InlineData("A")]
        [InlineData("AB")]
        [InlineData("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")]
        public void Validate_WhenRoomNameLengthIsOutOfRange_ShouldHaveValidatorError(string invalidRoomName)
        {
            RoomDto testRoomDto = new RoomDto
            {
                Name = invalidRoomName
            };

            ValidationResult result = _validator.Validate(testRoomDto);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == "Room name must have at least 3 characters and no more than 30");
        }

        [Theory]
        [InlineData("ABC")]
        [InlineData("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")]
        public void Validate_WhenRoomNameLengthIsInRangeFromThreeToThirty_ShouldNotHaveValidatorError(string roomName)
        {
            RoomDto testRoomDto = new RoomDto
            {
                Name = roomName
            };

            ValidationResult result = _validator.Validate(testRoomDto);

            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }
    }
}
