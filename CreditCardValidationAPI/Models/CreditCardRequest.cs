namespace CreditCardValidationAPI.Models
{
    public class CreditCardRequest
    {
        public string CardOwner { get; set; }
        public string CardNumber { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string CVC { get; set; }
    }
}
