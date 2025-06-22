using MarsellaBank.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MarsellaBank.Controllers
{
    public class HistoryController : Controller
    {
        private readonly AppDbContext _context;

        public HistoryController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Transaction/History
        public async Task<IActionResult> History()
        {
            var email = HttpContext.Session.GetString("Auth_UserEmail");
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Login", "Auth");

            var transactions = await _context.Transactions
                .Include(t => t.FromCard)
                .Include(t => t.ToCard)
                .Include(t => t.Account)
                    .ThenInclude(a => a.Client)
                .Where(t => t.Account.Client.Email == email)
                .OrderByDescending(t => t.Date)
                .ToListAsync();

            return View(transactions);
        }
    }
}