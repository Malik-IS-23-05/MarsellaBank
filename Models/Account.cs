

namespace MarsellaBank.Models
{
    public class Account
    {
        public int Id { get; set; }

        // Внешний ключ
        public int ClientId { get; set; }

        // Навигационные свойства
        public Client Client { get; set; }
        public List<Card> Cards { get; set; } = new List<Card>();
        public List<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}