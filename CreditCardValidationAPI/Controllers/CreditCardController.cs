using CreditCardValidationAPI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace CreditCardValidationAPI.Controllers
{
 
        [ApiController]
        [Route("api/[controller]")]
        public class CreditCardController : ControllerBase
        {
            [HttpPost("validate")]
            public IActionResult ValidateCard([FromBody] CreditCardRequest request)
            {
                var response = new CreditCardResponse
                {
                    Errors = new List<string>()
                };

                // Validates that all fields are provided
                ValidateFields(request, response.Errors);

                // If there are errors at this point, return early
                if (response.Errors.Any())
                {
                    response.IsValid = false;
                    return BadRequest(response);
                }

                // Validate Card Owner
                ValidateCardOwner(request.CardOwner, response.Errors);

                // Validate Expiry Date
                ValidateExpiryDate(request.ExpiryDate, response.Errors);

                // Validate Card Number and get Card Type
                var cardType = GetCardType(request.CardNumber, response.Errors);

                if (cardType != null)
                {
                    response.CardType = cardType;
                    // Step 5: Validate CVC
                    ValidateCVC(request.CVC, cardType, response.Errors);
                }

                // Set IsValid based on whether there are errors
                response.IsValid = !response.Errors.Any();

                if (response.IsValid)
                {
                    return Ok(response);
                }
                else
                {
                    return BadRequest(response);
                }
            }

            // Validation Methods

            private void ValidateFields(CreditCardRequest request, List<string> errors)
            {
                if (string.IsNullOrWhiteSpace(request.CardOwner))
                    errors.Add("Card owner name is required.");

                if (string.IsNullOrWhiteSpace(request.CardNumber))
                    errors.Add("Card number is required.");

                if (request.ExpiryDate == default)
                    errors.Add("Expiry date is required.");

                if (string.IsNullOrWhiteSpace(request.CVC))
                    errors.Add("CVC is required.");
            }

            private void ValidateCardOwner(string cardOwner, List<string> errors)
            {
                string pattern = @"[^a-zA-Z\s]"; // matches any character that is not a letter or space

                if (Regex.IsMatch(cardOwner, pattern))
                {
                    errors.Add("Card owner name should only contain letters.");
                }

                // Check for patterns resembling credit card numbers or CVC numbers
                var digitCount = cardOwner.Count(char.IsDigit);

                if (digitCount == 13 || digitCount == 15 || digitCount == 16 )
                {
                    errors.Add("Card owner name appears to contain sensitive information.");
                }
                else if (digitCount == 3 || digitCount == 4)
                {
                    errors.Add("Card owner name appears to contain sensitive information.");
                }
            }

            private void ValidateExpiryDate(DateTime expiryDate, List<string> errors)
            {
                var currentDate = DateTime.UtcNow;

                // Assuming the expiry date is provided as the last day of the expiry month
                var lastDayOfExpiryMonth = new DateTime(expiryDate.Year, expiryDate.Month, DateTime.DaysInMonth(expiryDate.Year, expiryDate.Month));

                if (lastDayOfExpiryMonth < currentDate)
                {
                    errors.Add("Card is expired.");
                }
            }

            private string GetCardType(string cardNumber, List<string> errors)
            {
                // Remove any spaces or hyphens
                cardNumber = cardNumber.Replace(" ", "").Replace("-", "");

                if (!cardNumber.All(char.IsDigit))
                {
                    errors.Add("Card number must contain only digits.");
                    return null;
                }

                // Visa: Starts with 4, length 13 or 16
                if (cardNumber.StartsWith("4") && (cardNumber.Length == 13 || cardNumber.Length == 16))
                {
                    return "Visa";
                }
                // MasterCard: Starts with 51-55 or 2221-2720, length 16
                else if ((IsMasterCardPrefix(cardNumber)) && cardNumber.Length == 16)
                {
                    return "MasterCard";
                }
                // American Express: Starts with 34 or 37, length 15
                else if ((cardNumber.StartsWith("34") || cardNumber.StartsWith("37")) && cardNumber.Length == 15)
                {
                    return "American Express";
                }
                else
                {
                    errors.Add("Card number is not valid for Visa, MasterCard, or American Express.");
                    return null;
                }
            }

            private bool IsMasterCardPrefix(string cardNumber)
            {
                // Check for prefixes 51-55
                if (cardNumber.Length >= 2)
                    {
                    var prefix2 = cardNumber.Substring(0, 2);
                    if (int.TryParse(prefix2, out int prefixInt2) && prefixInt2 >= 51 && prefixInt2 <= 55)
                    {
                        return true;
                    }
                }

                // Check for prefixes 2221-2720
                if (cardNumber.Length >= 4)
                {
                    var prefix4 = cardNumber.Substring(0, 4);
                    if (int.TryParse(prefix4, out int prefixInt4) && prefixInt4 >= 2221 && prefixInt4 <= 2720)
                    {
                        return true;
                    }
                }

                return false;
            }

            private void ValidateCVC(string cvc, string cardType, List<string> errors)
            {
                if (!cvc.All(char.IsDigit))
                {
                    errors.Add("CVC must contain only digits.");
                    return;
                }

                if (cardType == "American Express")
                {
                    if (cvc.Length != 4)
                        errors.Add("American Express CVC must be 4 digits.");
                }
                else if (cardType == "Visa" || cardType == "MasterCard")
                {
                    if (cvc.Length != 3)
                        errors.Add($"{cardType} CVC must be 3 digits.");
                }
                else
                {
                    errors.Add("Unknown card type for CVC validation.");
                }
            }
        }
}
