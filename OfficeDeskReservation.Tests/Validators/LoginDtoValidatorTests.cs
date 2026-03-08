using FluentAssertions;
using FluentValidation.Results;
using OfficeDeskReservation.API.Dtos.Auth;
using OfficeDeskReservation.API.Validators;

namespace OfficeDeskReservation.Tests.Validators
{
    public class LoginDtoValidatorTests
    {
        private readonly LoginDtoValidator _validator;

        public LoginDtoValidatorTests()
        {
            _validator = new LoginDtoValidator();
        }

        [Fact]
        public void Validate_WhenPasswordIsEmpty_ShouldHaveValidatorError()
        {
            LoginDto testLoginDto = new LoginDto
            {
                Email = "robota@gmail.com",
                Password = ""
            };

            ValidationResult result = _validator.Validate(testLoginDto);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == "Password is required.");
        }

        [Fact]
        public void Validate_WhenEmailAddressIsEmpty_ShouldHaveValidatorError()
        {
            LoginDto testLoginDto = new LoginDto
            {
                Email = "",
                Password = "12345678"
            };

            ValidationResult result = _validator.Validate(testLoginDto);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == "Email is required.");
        }

        [Theory]
        [InlineData("plainaddress")]
        [InlineData("@domain.com")]
        [InlineData("username@")]
        [InlineData("user@domain@test.com")]
        [InlineData("user name@domain.com")]
        public void Validate_WhenEmailAddressIsInvalid_ShouldHaveValidatorError(string invalidEmail)
        {
            LoginDto testLoginDto = new LoginDto
            {
                Email = invalidEmail,
                Password = "12345678"
            };

            ValidationResult result = _validator.Validate(testLoginDto);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == "Email address is invalid.");
        }

        [Fact]
        public void Validate_WhenEmailAddressLengthIsEqualToOneHundred_ShouldNotHaveValidatorError()
        {
            LoginDto testLoginDto = new LoginDto
            {
                Email = new string('a', 90) + "@gmail.com",
                Password = "123456789"
            };

            ValidationResult result = _validator.Validate(testLoginDto);

            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Validate_WhenEmailAddressLengthIsMoreThanOneHundred_ShouldHaveValidatorError()
        {
            LoginDto testLoginDto = new LoginDto
            {
                Email = new string('a', 91) + "@gmail.com",
                Password = "12345678"
            };

            ValidationResult result = _validator.Validate(testLoginDto);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == "Email must not exceed 100 characters.");
        }
    }
}
