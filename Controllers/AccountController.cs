using MarsellaBank.Data;
using MarsellaBank.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MarsellaBank.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            AppDbContext context,
            ILogger<AccountController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: /Account/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var email = HttpContext.Session.GetString("Auth_UserEmail");
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login", "Auth");
            }


            var client = await _context.Clients
                .Include(c => c.Accounts)
                    .ThenInclude(a => a.Cards)
                .Include(c => c.Accounts)
                    .ThenInclude(a => a.Transactions)
                        .ThenInclude(t => t.FromCard)
                .Include(c => c.Accounts)
                    .ThenInclude(a => a.Transactions)
                        .ThenInclude(t => t.ToCard)
                .FirstOrDefaultAsync(c => c.Email == email);

            if (client == null)
            {
                return NotFound();
            }

            return View(client);
        }

        // GET: /Account/Profile
        public async Task<IActionResult> Profile()
        {
            var email = HttpContext.Session.GetString("Auth_UserEmail");
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login", "Auth");
            }

            var client = await _context.Clients
                .FirstOrDefaultAsync(c => c.Email == email);

            if (client == null)
            {
                return NotFound();
            }

            return View(client);
        }

        // POST: /Account/Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(Client model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var email = HttpContext.Session.GetString("Auth_UserEmail");
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login", "Auth");
            }

            var client = await _context.Clients
                .FirstOrDefaultAsync(c => c.Email == email);

            if (client == null)
            {
                return NotFound();
            }

            // Обновляем только разрешенные поля
            client.Email = model.Email;
            client.Password = model.Password; // В реальном проекте нужно хеширование!

            _context.Update(client);
            await _context.SaveChangesAsync();

            // Обновляем email в сессии, если он изменился
            HttpContext.Session.SetString("Auth_UserEmail", client.Email);

            return RedirectToAction(nameof(Profile));
        }
    }
}