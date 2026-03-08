using FluentAssertions;
using FluentValidation.Results;
using OfficeDeskReservation.API.Dtos.Auth;
using OfficeDeskReservation.API.Validators;

namespace OfficeDeskReservation.Tests.Validators
{
    public class RegisterDtoValidatorTests
    {
        private readonly RegisterDtoValidator _validator;

        public RegisterDtoValidatorTests()
        {
            _validator = new RegisterDtoValidator();
        }

        [Fact]
        public void Validate_WhenFirstNameIsEmpty_ShouldHaveValidatorError()
        {
            RegisterDto testRegisterDto = new RegisterDto
            {
                FirstName = "",
                LastName = "Dobrovolsky",
                Email = "praca@gmail.com",
                Password = "12345678_aA"
            };

            ValidationResult result = _validator.Validate(testRegisterDto);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == "First name is required!");
        }

        [Fact]
        public void Validate_WhenFirstNameLengthIsLessThanTwo_ShouldHaveValidatorError()
        {
            RegisterDto testRegisterDto = new RegisterDto
            {
                FirstName = "a",
                LastName = "Dobrovolsky",
                Email = "praca@gmail.com",
                Password = "12345678_aA"
            };

            ValidationResult result = _validator.Validate(testRegisterDto);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == "First name must contain at least 2 characters!");
        }

        [Theory]
        [InlineData("Ja")]
        [InlineData("Jan")]
        public void Validate_WhenFirstNameLengthIsGreaterOrEqualThanTwo_ShouldNotHaveValidatorError(string name)
        {
            RegisterDto testRegisterDto = new RegisterDto
            {
                FirstName = name,
                LastName = "Dobrovolsky",
                Email = "praca@gmail.com",
                Password = "12345678_aA"
            };

            ValidationResult result = _validator.Validate(testRegisterDto);

            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Validate_WhenLastNameIsEmpty_ShouldHaveValidatorError()
        {
            RegisterDto testRegisterDto = new RegisterDto
            {
                FirstName = "Ja",
                LastName = "",
                Email = "praca@gmail.com",
                Password = "12345678_aA"
            };

            ValidationResult result = _validator.Validate(testRegisterDto);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == "Last name is required!");
        }

        [Fact]
        public void Validate_WhenLastNameLengthIsLessThanTwo_ShouldHaveValidatorError()
        {
            RegisterDto testRegisterDto = new RegisterDto
            {
                FirstName = "Ja",
                LastName = "A",
                Email = "praca@gmail.com",
                Password = "12345678_aA"
            };

            ValidationResult result = _validator.Validate(testRegisterDto);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == "Last name must contain at least 2 characters!");
        }

        [Theory]
        [InlineData("Do")]
        [InlineData("Dob")]
        public void Validate_WhenLastNameLengthIsGreaterOrEqualThanTwo_ShouldNotHaveValidatorError(string lastName)
        {
            RegisterDto testRegisterDto = new RegisterDto
            {
                FirstName = "Ja",
                LastName = lastName,
                Email = "praca@gmail.com",
                Password = "12345678_aA"
            };

            ValidationResult result = _validator.Validate(testRegisterDto);

            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Validate_WhenEmailAddressIsEmpty_ShouldHaveValidatorError()
        {
            RegisterDto testRegisterDto = new RegisterDto
            {
                FirstName = "Ja",
                LastName = "Dobrovolsky",
                Email = "",
                Password = "12345678_aA"
            };

            ValidationResult result = _validator.Validate(testRegisterDto);

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
            RegisterDto testRegisterDto = new RegisterDto
            {
                Email = invalidEmail,
                Password = "12345678"
            };

            ValidationResult result = _validator.Validate(testRegisterDto);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == "Email address is invalid!");
        }

        [Fact]
        public void Validate_WhenEmailAddressLengthIsEqualToOneHundred_ShouldNotHaveValidatorError()
        {
            RegisterDto testRegisterDto = new RegisterDto
            {
                FirstName = "Ja",
                LastName = "Dobrovolsky",
                Email = new string('a', 90) + "@gmail.com",
                Password = "12345678_aA"
            };

            ValidationResult result = _validator.Validate(testRegisterDto);

            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Validate_WhenEmailAddressLengthIsMoreThanOneHundred_ShouldHaveValidatorError()
        {
            RegisterDto testRegisterDto = new RegisterDto
            {
                FirstName = "Ja",
                LastName = "Dobrovolsky",
                Email = new string('a', 91) + "@gmail.com",
                Password = "12345678_aA"
            };

            ValidationResult result = _validator.Validate(testRegisterDto);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == "Email address must not exceed 100 characters!");
        }

        [Fact]
        public void Validate_WhenPasswordIsEmpty_ShouldHaveValidatorError()
        {
            RegisterDto testRegisterDto = new RegisterDto
            {
                FirstName = "Ja",
                LastName = "Dobrovolsky",
                Email = "praca@gmail.com",
                Password = ""
            };

            ValidationResult result = _validator.Validate(testRegisterDto);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == "Password is required!");
        }

        [Fact]
        public void Validate_WhenPasswordLengthIsLessThanEight_ShouldHaveValidatorError()
        {
            RegisterDto testRegisterDto = new RegisterDto
            {
                FirstName = "Ja",
                LastName = "Dobrovolsky",
                Email = "praca@gmail.com",
                Password = "12aA_67"
            };

            ValidationResult result = _validator.Validate(testRegisterDto);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == "Password must contain at least 8 characters!");
        }

        [Fact]
        public void Validate_WhenPasswordDoesNotContainTheNumber_ShouldHaveValidatorError()
        {
            RegisterDto testRegisterDto = new RegisterDto
            {
                FirstName = "Ja",
                LastName = "Dobrovolsky",
                Email = "praca@gmail.com",
                Password = "abcdefgH_"
            };

            ValidationResult result = _validator.Validate(testRegisterDto);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == "Password must contain at least one number!");
        }

        [Fact]
        public void Validate_WhenPasswordDoesNotContainTheLowercaseCharacter_ShouldHaveValidatorError()
        {
            RegisterDto testRegisterDto = new RegisterDto
            {
                FirstName = "Ja",
                LastName = "Dobrovolsky",
                Email = "praca@gmail.com",
                Password = "ABCDEFGH_0"
            };

            ValidationResult result = _validator.Validate(testRegisterDto);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == "Password must contain at least one lowercase letter!");
        }

        [Fact]
        public void Validate_WhenPasswordDoesNotContainTheUppercaseCharacter_ShouldHaveValidatorError()
        {
            RegisterDto testRegisterDto = new RegisterDto
            {
                FirstName = "Ja",
                LastName = "Dobrovolsky",
                Email = "praca@gmail.com",
                Password = "abcdefgh_0"
            };

            ValidationResult result = _validator.Validate(testRegisterDto);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == "Password must contain at least one uppercase letter!");
        }

        [Fact]
        public void Validate_WhenPasswordDoesNotContainTheSpecialCharacter_ShouldHaveValidatorError()
        {
            RegisterDto testRegisterDto = new RegisterDto
            {
                FirstName = "Ja",
                LastName = "Dobrovolsky",
                Email = "praca@gmail.com",
                Password = "abcdefgH0"
            };

            ValidationResult result = _validator.Validate(testRegisterDto);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == "Password must contain at least one special character!");
        }
    }
}
