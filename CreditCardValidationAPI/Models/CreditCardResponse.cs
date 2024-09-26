namespace CreditCardValidationAPI.Models
{
    public class CreditCardResponse
    {
        public bool IsValid { get; set; }
        public string CardType { get; set; } // Visa, MasterCard, American Express
        public List<string> Errors { get; set; }
    }
}
