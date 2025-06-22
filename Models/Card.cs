namespace MarsellaBank.Models
{
    public class Card
    {
        public int Id { get; set; }
        public required string Number { get; set; }  // Например, "1234 5678 9012 3456"
        public decimal Balance { get; set; }
        public int AccountId { get; set; }
        public Account? Account { get; set; }
    }
}