using MarsellaBank.Models;
using System.ComponentModel.DataAnnotations;

namespace MarsellaBank.Models
{
    public class Transaction
    {
        public int Id { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime Date { get; set; } = DateTime.Now;

        // Отправитель
        public int FromCardId { get; set; }
        public Card FromCard { get; set; }

        // Получатель
        public int ToCardId { get; set; }
        public Card ToCard { get; set; }

        // Счет, к которому привязана транзакция
        public int AccountId { get; set; }
        public Account Account { get; set; }
    }
}