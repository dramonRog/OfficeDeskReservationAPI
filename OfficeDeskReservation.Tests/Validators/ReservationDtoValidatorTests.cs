using FluentAssertions;
using FluentValidation.Results;
using OfficeDeskReservation.API.Dtos.Reservations;
using OfficeDeskReservation.API.Validators;

namespace OfficeDeskReservation.Tests.Validators
{
    public class ReservationDtoValidatorTests
    {
        private readonly ReservationDtoValidator _validator;

        public ReservationDtoValidatorTests()
        {
            _validator = new ReservationDtoValidator();
        }

        [Fact]
        public void Validate_WhenStartTimeIsInThePast_ShouldHaveValidatorError()
        {
            ReservationDto testReservationDto = new ReservationDto
            {
                DeskId = 1,
                StartTime = DateTime.Now.AddDays(-1),
                EndTime = DateTime.Now.AddDays(1)
            };

            ValidationResult result = _validator.Validate(testReservationDto);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == "Start time must be in the future!");
        }

        [Fact]
        public void Validate_WhenStartTimeIsEmpty_ShouldHaveValidatorError()
        {
            ReservationDto testReservationDto = new ReservationDto
            {
                DeskId = 1,
                EndTime = DateTime.Now.AddDays(1)
            };

            ValidationResult result = _validator.Validate(testReservationDto);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == "Start time is required!");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_WhenDeskIdIsLessOrEqualThenZero_ShouldHaveValidatorError(int id)
        {
            ReservationDto testReservationDto = new ReservationDto
            {
                DeskId = id,
                StartTime = DateTime.Now.AddHours(4),
                EndTime = DateTime.Now.AddHours(6)
            };

            ValidationResult result = _validator.Validate(testReservationDto);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == "A valid Desk ID is required!");
        }

        [Fact]
        public void Validate_WhenEndTimeIsEmpty_ShouldHaveValidatorError()
        {
            ReservationDto testReservationDto = new ReservationDto
            {
                DeskId = 1,
                StartTime = DateTime.Now.AddHours(1)
            };

            ValidationResult result = _validator.Validate(testReservationDto);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == "End time is required!");
        }

        [Fact]
        public void Validate_WhenEndTimeIsBeforeStartTime_ShouldHaveValidatorError()
        {
            ReservationDto testReservationDto = new ReservationDto
            {
                DeskId = 1,
                StartTime = DateTime.Now.AddHours(15),
                EndTime = DateTime.Now.AddHours(10)
            };

            ValidationResult result = _validator.Validate(testReservationDto);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == "End time must be after the start time!");
        }

        [Fact]
        public void Validate_WhenEndTimeIsEqualToStartTime_ShouldHaveValidatorError()
        {
            DateTime time = DateTime.Now.AddHours(1);

            ReservationDto testReservationDto = new ReservationDto
            {
                DeskId = 1,
                StartTime = time,
                EndTime = time
            };

            ValidationResult result = _validator.Validate(testReservationDto);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == "End time must be after the start time!");
        }
    }
}
