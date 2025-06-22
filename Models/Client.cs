using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MarsellaBank.Models
{
    public class Client
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Некорректный email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Пароль обязателен")]
        [MinLength(4, ErrorMessage = "Минимум 4 символа")]
        public string Password { get; set; } = string.Empty;

        // Навигационное свойство
        public ICollection<Account> Accounts { get; set; } = new List<Account>();
    }
}
