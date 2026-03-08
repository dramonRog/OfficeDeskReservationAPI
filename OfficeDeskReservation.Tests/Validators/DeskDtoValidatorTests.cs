using FluentAssertions;
using FluentValidation.Results;
using OfficeDeskReservation.API.Dtos.Desks;
using OfficeDeskReservation.API.Validators;

namespace OfficeDeskReservation.Tests.Validators
{
    public class DeskDtoValidatorTests
    {
        private readonly DeskDtoValidator _validator;

        public DeskDtoValidatorTests()
        {
            _validator = new DeskDtoValidator();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_WhenRoomIdIsLessOrEqualThanZero_ShouldHaveValidatorError(int invalidId)
        {
            DeskDto testDeskDto = new DeskDto
            {
                RoomId = invalidId,
                DeskIdentifier = "ABCD"
            };

            ValidationResult result = _validator.Validate(testDeskDto);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == "A valid Room ID is required!");
        }

        [Fact]
        public void Validate_WhenRoomIdIsGreaterThanZero_ShouldNotHaveValidatorError()
        {
            DeskDto testDeskDto = new DeskDto
            {
                RoomId = 1,
                DeskIdentifier = "ABCD"
            };

            ValidationResult result = _validator.Validate(testDeskDto);

            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Validate_WhenDeskIdentifierIsEmpty_ShouldHaveValidatorError()
        {
            DeskDto testDeskDto = new DeskDto
            {
                RoomId = 1,
                DeskIdentifier = ""
            };

            ValidationResult result = _validator.Validate(testDeskDto);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == "Desk identifier is required!");
        }

        [Theory]
        [InlineData("A")]
        [InlineData("AAAAAAAAAAAAAAAAAAAA")]
        public void Validate_WhenDeskIdentifierLengthIsInRangeFromOneToTwenty_ShouldNotHaveValidatorError(string deskIdentifier)
        {
            DeskDto testDeskDto = new DeskDto
            {
                RoomId = 1,
                DeskIdentifier = deskIdentifier
            };

            ValidationResult result = _validator.Validate(testDeskDto);

            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Validate_WhenDeskIdentifierLengthIsMoreThanTwenty_ShouldHaveValidatorError()
        {
            DeskDto testDeskDto = new DeskDto
            {
                RoomId = 1,
                DeskIdentifier = new string('A', 21)
            };

            ValidationResult result = _validator.Validate(testDeskDto);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == "Desk identifier must be between 1 and 20 characters!");
        }
    }
}
