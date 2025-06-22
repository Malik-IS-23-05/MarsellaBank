using System.ComponentModel.DataAnnotations;

namespace MarsellaBank.Models.ViewModels
{
    public class ClientViewModel
    {
        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Некорректный формат email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Пароль обязателен")]
        [MinLength(4, ErrorMessage = "Минимум 4 символа")]
        public string Password { get; set; } = string.Empty;
    }
}