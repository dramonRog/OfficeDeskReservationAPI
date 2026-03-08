using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using OfficeDeskReservation.API.Dtos.Users;
using OfficeDeskReservation.API.Validators;

namespace OfficeDeskReservation.Tests.Validators
{
    public class UserDtoValidatorTests
    {
        private readonly UserDtoValidator _validator;

        public UserDtoValidatorTests()
        {
            _validator = new UserDtoValidator();
        }

        [Fact]
        public void Validate_WhenFirstNameIsEmpty_ShouldHaveValidatorError()
        {
            UserDto testUserDto = new UserDto
            {
                FirstName = "",
                LastName = "Darvinsky",
                Email = "robota@gmail.com"
            };

            ValidationResult result = _validator.Validate(testUserDto);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == "First name is required!");
        }

        [Fact]
        public void Validate_WhenFirstNameLengthIsLessThanTwo_ShouldHaveValidatorError()
        {
            UserDto testUserDto = new UserDto
            {
                FirstName = "a",
                LastName = "Darvinsky",
                Email = "robota@gmail.com"
            };

            ValidationResult result = _validator.Validate(testUserDto);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == "First name must contain at least 2 characters!");
        }

        [Fact]
        public void Validate_WhenLastNameIsEmpty_ShouldHaveValidatorError()
        {
            UserDto testUserDto = new UserDto
            {
                FirstName = "Ja",
                LastName = "",
                Email = "robota@gmail.com"
            };

            ValidationResult result = _validator.Validate(testUserDto);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == "Last name is required!");
        }

        [Fact]
        public void Validate_WhenLastNameLengthIsLessThanTwo_ShouldHaveValidatorError()
        {
            UserDto testUserDto = new UserDto
            {
                FirstName = "Ja",
                LastName = "D",
                Email = "robota@gmail.com"
            };

            ValidationResult result = _validator.Validate(testUserDto);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == "Last name must contain at least 2 characters!");
        }

        [Fact]
        public void Validate_WhenEmailAddressIsEmpty_ShouldHaveValidatorError()
        {
            UserDto testUserDto = new UserDto
            {
                FirstName = "Ja",
                LastName = "Darvinsky",
                Email = ""
            };

            ValidationResult result = _validator.Validate(testUserDto);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == "Email address is required!");
        }

        [Theory]
        [InlineData("plainaddress")]
        [InlineData("@domain.com")]
        [InlineData("username@")]
        [InlineData("user@domain@test.com")]
        [InlineData("user name@domain.com")]
        public void Validate_WhenEmailAddressIsInvalid_ShouldHaveValidatorError(string invalidEmail)
        {
            UserDto testUserDto = new UserDto
            {
                FirstName = "Ja",
                LastName = "Darvinsky",
                Email = invalidEmail
            };

            ValidationResult result = _validator.Validate(testUserDto);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == "Email address is invalid!");
        }

        [Fact]
        public void Validate_WhenEmailAddressLengthIsEqualToOneHundred_ShouldNotHaveValidatorError()
        {
            UserDto testUserDto = new UserDto
            {
                FirstName = "Ja",
                LastName = "Darvinsky",
                Email = new string('a', 90) + "@gmail.com"
            };

            ValidationResult result = _validator.Validate(testUserDto);

            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Validate_WhenEmailAddressLengthIsMoreThanOneHundred_ShouldHaveValidatorError()
        {
            UserDto testUserDto = new UserDto
            {
                FirstName = "Ja",
                LastName = "Darvinsky",
                Email = new string('a', 91) + "@gmail.com"
            };

            ValidationResult result = _validator.Validate(testUserDto);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == "Email address must not exceed 100 characters!");
        }
    }
}
