using MarsellaBank.Data;
using MarsellaBank.Models;
using MarsellaBank.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MarsellaBank.Controllers
{
    public class AuthController : Controller
    {
        private readonly AppDbContext _context;
        private const string AuthSessionKey = "Auth_UserEmail";

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Auth/Register
        public IActionResult Register()
        {
            return View(new ClientViewModel());
        }

        // POST: /Auth/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(ClientViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                if (await _context.Clients.AnyAsync(c => c.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "Этот email уже зарегистрирован");
                    return View(model);
                }

                var client = new Client
                {
                    Email = model.Email,
                    Password = model.Password // В реальном проекте нужно хеширование!
                };

                var account = new Account
                {
                    Client = client,
                    Cards = { new Card { Number = GenerateCardNumber(), Balance = 1000 } }
                };

                _context.Accounts.Add(account);
                await _context.SaveChangesAsync();

                HttpContext.Session.SetString(AuthSessionKey, client.Email);
                return RedirectToAction("Dashboard", "Account");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Ошибка при регистрации");
                return View(model);
            }
        }

        // GET: /Auth/Login
        public IActionResult Login()
        {
            return View(new ClientViewModel());
        }

        // POST: /Auth/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(ClientViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var client = await _context.Clients
                    .FirstOrDefaultAsync(c => c.Email == model.Email && c.Password == model.Password);

                if (client == null)
                {
                    ModelState.AddModelError("", "Неверный email или пароль");
                    return View(model);
                }

                HttpContext.Session.SetString(AuthSessionKey, client.Email);
                return RedirectToAction("Dashboard", "Account");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Ошибка при входе в систему");
                return View(model);
            }
        }

        // POST: /Auth/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Remove(AuthSessionKey);
            return RedirectToAction("Index", "Home");
        }

        private string GenerateCardNumber()
        {
            var rnd = new Random();
            return $"{rnd.Next(1000, 9999)} {rnd.Next(1000, 9999)} {rnd.Next(1000, 9999)} {rnd.Next(1000, 9999)}";
        }
    }
}