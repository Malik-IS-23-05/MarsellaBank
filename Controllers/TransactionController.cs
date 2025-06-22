using MarsellaBank.Data;
using MarsellaBank.Models;
using MarsellaBank.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace MarsellaBank.Controllers
{
    public class TransactionController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<TransactionController> _logger;

        public TransactionController(AppDbContext context, ILogger<TransactionController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: /Transaction/Transfer
 // В начале файла добавить

// Измененный метод Transfer()
public async Task<IActionResult> Transfer()
    {
        var email = HttpContext.Session.GetString("Auth_UserEmail");
        if (string.IsNullOrEmpty(email))
        {
            return RedirectToAction("Login", "Auth");
        }

        try
        {
            var userCards = await GetUserCardsAsync(email);
            if (userCards == null || !userCards.Any())
            {
                TempData["WarningMessage"] = "У вас нет привязанных карт для перевода";
                return RedirectToAction("Dashboard", "Account");
            }

            ViewBag.UserCards = userCards.AsEnumerable(); // Явное преобразование
            return View(new TransferViewModel());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении карт пользователя");
            TempData["ErrorMessage"] = "Ошибка при загрузке данных";
            return RedirectToAction("Dashboard", "Account");
        }
    }

    // POST: /Transaction/Transfer
    [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Transfer(TransferViewModel model)
        {
            var email = HttpContext.Session.GetString("Auth_UserEmail");
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                // Всегда загружаем карты для повторного отображения формы
                ViewBag.UserCards = await GetUserCardsAsync(email);

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // Удаляем пробелы из номера карты для сравнения
                var cleanToCardNumber = model.ToCardNumber?.Replace(" ", "");

                // Проверяем, что карты разные
                if (model.FromCardNumber?.Replace(" ", "") == cleanToCardNumber)
                {
                    ModelState.AddModelError("", "Нельзя переводить на ту же карту");
                    return View(model);
                }

                // Получаем карту отправителя с блокировкой для предотвращения гонки
                var fromCard = await _context.Cards
                    .Include(c => c.Account)
                        .ThenInclude(a => a.Client)
                    .FirstOrDefaultAsync(c => c.Number == model.FromCardNumber &&
                                           c.Account.Client.Email == email);

                if (fromCard == null)
                {
                    ModelState.AddModelError("", "Карта отправителя не найдена или вам не принадлежит");
                    return View(model);
                }

                // Получаем карту получателя
                var toCard = await _context.Cards
                    .FirstOrDefaultAsync(c => c.Number.Replace(" ", "") == cleanToCardNumber);

                if (toCard == null)
                {
                    ModelState.AddModelError("ToCardNumber", "Карта получателя не найдена");
                    return View(model);
                }

                // Проверяем баланс с учетом возможной параллельной операции
                if (fromCard.Balance < model.Amount)
                {
                    ModelState.AddModelError("Amount", "Недостаточно средств на карте");
                    return View(model);
                }

                // Начинаем транзакцию БД
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // Создаем запись о транзакции
                        var newTransaction = new Transaction
                        {
                            Amount = model.Amount,
                            Date = DateTime.UtcNow,
                            FromCardId = fromCard.Id,
                            ToCardId = toCard.Id,
                            AccountId = fromCard.AccountId
                        };

                        // Обновляем балансы
                        fromCard.Balance -= model.Amount;
                        toCard.Balance += model.Amount;

                        _context.Transactions.Add(newTransaction);
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        TempData["SuccessMessage"] = $"Перевод на сумму {model.Amount:C} успешно выполнен";
                        return RedirectToAction("Dashboard", "Account");
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogError(ex, "Ошибка при выполнении перевода");
                        ModelState.AddModelError("", "Ошибка при выполнении перевода");
                        return View(model);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Системная ошибка при обработке перевода");
                TempData["ErrorMessage"] = "Системная ошибка. Попробуйте позже.";
                return RedirectToAction("Dashboard", "Account");
            }
        }

        private async Task<List<Card>> GetUserCardsAsync(string email)
        {
            return await _context.Cards
                .Include(c => c.Account)
                    .ThenInclude(a => a.Client)
                .Where(c => c.Account.Client.Email == email)
                .OrderBy(c => c.Number)
                .ToListAsync();
        }
    }
}