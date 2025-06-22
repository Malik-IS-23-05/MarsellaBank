using System.ComponentModel.DataAnnotations;

namespace MarsellaBank.Models.ViewModels
{
    public class TransferViewModel
    {
        [Required(ErrorMessage = "Выберите карту списания")]
        [Display(Name = "С вашей карты")]
        public string FromCardNumber { get; set; }

        [Required(ErrorMessage = "Введите номер карты получателя")]
        [Display(Name = "На карту")]
        public string ToCardNumber { get; set; } // Убрали RegularExpression

        [Required(ErrorMessage = "Введите сумму перевода")]
        [Range(0.01, 1000000, ErrorMessage = "Сумма должна быть от {1} до {2}")]
        [Display(Name = "Сумма")]
        public decimal Amount { get; set; }
    }
}