namespace CreditCardValidationAPI.Tests
{
    using Xunit;
    using CreditCardValidationAPI.Controllers;
    using CreditCardValidationAPI.Models;
    using Microsoft.AspNetCore.Mvc;

    public class CreditCardControllerTests
    {
        private readonly CreditCardController _controller;

        public CreditCardControllerTests()
        {
            _controller = new CreditCardController();
        }

        [Fact]
        public void ValidateCard_ValidVisaCard_ReturnsOk()
        {
            // Arrange
            var request = new CreditCardRequest
            {
                CardOwner = "John Doe",
                CardNumber = "4111111111111111",
                ExpiryDate = DateTime.UtcNow.AddYears(1),
                CVC = "123"
            };

            // Act
            var result = _controller.ValidateCard(request) as OkObjectResult;
            var response = result.Value as CreditCardResponse;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.True(response.IsValid);
            Assert.Equal("Visa", response.CardType);
            Assert.Empty(response.Errors);
        }

        [Fact]
        public void ValidateCard_ValidMasterCard_ReturnsOk()
        {
            // Arrange
            var request = new CreditCardRequest
            {
                CardOwner = "Alice Smith",
                CardNumber = "5555555555554444", // Valid MasterCard number
                ExpiryDate = DateTime.UtcNow.AddYears(1),
                CVC = "123"
            };

            // Act
            var result = _controller.ValidateCard(request) as OkObjectResult;
            var response = result.Value as CreditCardResponse;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.True(response.IsValid);
            Assert.Equal("MasterCard", response.CardType);
            Assert.Empty(response.Errors);
        }

        [Fact]
        public void ValidateCard_ValidAmexCard_ReturnsOk()
        {
            // Arrange
            var request = new CreditCardRequest
            {
                CardOwner = "Bob Johnson",
                CardNumber = "371449635398431", // Valid Amex number
                ExpiryDate = DateTime.UtcNow.AddYears(1),
                CVC = "1234" // Amex CVC is 4 digits
            };

            // Act
            var result = _controller.ValidateCard(request) as OkObjectResult;
            var response = result.Value as CreditCardResponse;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.True(response.IsValid);
            Assert.Equal("American Express", response.CardType);
            Assert.Empty(response.Errors);
        }

        [Fact]
        public void ValidateCard_ExpiredCard_ReturnsError()
        {
            // Arrange
            var request = new CreditCardRequest
            {
                CardOwner = "Expired Card",
                CardNumber = "4111111111111111",
                ExpiryDate = DateTime.UtcNow.AddYears(-1), // Past date
                CVC = "123"
            };

            // Act
            var result = _controller.ValidateCard(request) as BadRequestObjectResult;
            var response = result.Value as CreditCardResponse;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.False(response.IsValid);
            Assert.Contains("Card is expired.", response.Errors);
        }

        [Fact]
        public void ValidateCard_InvalidCVCForVisa_ReturnsError()
        {
            // Arrange
            var request = new CreditCardRequest
            {
                CardOwner = "John Doe",
                CardNumber = "4111111111111111",
                ExpiryDate = DateTime.UtcNow.AddYears(1),
                CVC = "12" // Invalid length
            };

            // Act
            var result = _controller.ValidateCard(request) as BadRequestObjectResult;
            var response = result.Value as CreditCardResponse;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.False(response.IsValid);
            Assert.Contains("Visa CVC must be 3 digits.", response.Errors);
        }

        [Fact]
        public void ValidateCard_InvalidCVCForAmex_ReturnsError()
        {
            // Arrange
            var request = new CreditCardRequest
            {
                CardOwner = "Jane Doe",
                CardNumber = "371449635398431",
                ExpiryDate = DateTime.UtcNow.AddYears(1),
                CVC = "123" // Should be 4 digits
            };

            // Act
            var result = _controller.ValidateCard(request) as BadRequestObjectResult;
            var response = result.Value as CreditCardResponse;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.False(response.IsValid);
            Assert.Contains("American Express CVC must be 4 digits.", response.Errors);
        }

        [Fact]
        public void ValidateCard_InvalidCVCForMasterCard_ReturnsError()
        {
            // Arrange
            var request = new CreditCardRequest
            {
                CardOwner = "John Doe",
                CardNumber = "5555555555554444",
                ExpiryDate = DateTime.UtcNow.AddYears(1),
                CVC = "1234" // Invalid length
            };

            // Act
            var result = _controller.ValidateCard(request) as BadRequestObjectResult;
            var response = result.Value as CreditCardResponse;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.False(response.IsValid);
            Assert.Contains("MasterCard CVC must be 3 digits.", response.Errors);
        }

        [Fact]
        public void ValidateCard_MissingCardOwner_ReturnsError()
        {
            // Arrange
            var request = new CreditCardRequest
            {
                // CardOwner is missing
                CardNumber = "4111111111111111",
                ExpiryDate = DateTime.UtcNow.AddYears(1),
                CVC = "123"
            };

            // Act
            var result = _controller.ValidateCard(request) as BadRequestObjectResult;
            var response = result.Value as CreditCardResponse;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.False(response.IsValid);
            Assert.Contains("Card owner name is required.", response.Errors);
        }

        [Fact]
        public void ValidateCard_CardOwnerWithDigits_ReturnsError()
        {
            // Arrange
            var request = new CreditCardRequest
            {
                CardOwner = "John123",
                CardNumber = "4111111111111111",
                ExpiryDate = DateTime.UtcNow.AddYears(1),
                CVC = "123"
            };

            // Act
            var result = _controller.ValidateCard(request) as BadRequestObjectResult;
            var response = result.Value as CreditCardResponse;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.False(response.IsValid);
            Assert.Contains("Card owner name should only contain letters.", response.Errors);
        }

        [Fact]
        public void ValidateCard_CardOwnerWithSpecialCharacters_ReturnsError()
        {
            // Arrange
            var request = new CreditCardRequest
            {
                CardOwner = "John!#¤",
                CardNumber = "4111111111111111",
                ExpiryDate = DateTime.UtcNow.AddYears(1),
                CVC = "123"
            };

            // Act
            var result = _controller.ValidateCard(request) as BadRequestObjectResult;
            var response = result.Value as CreditCardResponse;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.False(response.IsValid);
            Assert.Contains("Card owner name should only contain letters.", response.Errors);
        }

        [Fact]
        public void ValidateCard_CardNumberWithNonDigits_ReturnsError()
        {
            // Arrange
            var request = new CreditCardRequest
            {
                CardOwner = "John Doe",
                CardNumber = "4111-1111-1111-1111", // Contains hyphens
                ExpiryDate = DateTime.UtcNow.AddYears(1),
                CVC = "123"
            };

            // Act
            var result = _controller.ValidateCard(request) as OkObjectResult;
            var response = result.Value as CreditCardResponse;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.True(response.IsValid);
            Assert.Equal("Visa", response.CardType);
            Assert.Empty(response.Errors);
        }

        [Fact]
        public void ValidateCard_UnsupportedCardType_ReturnsError()
        {
            // Arrange
            var request = new CreditCardRequest
            {
                CardOwner = "John Doe",
                CardNumber = "6011111111111117", // Unknown card type
                ExpiryDate = DateTime.UtcNow.AddYears(1),
                CVC = "123"
            };

            // Act
            var result = _controller.ValidateCard(request) as BadRequestObjectResult;
            var response = result.Value as CreditCardResponse;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.False(response.IsValid);
            Assert.Contains("Card number is not valid for Visa, MasterCard, or American Express.", response.Errors);
        }
    }
}