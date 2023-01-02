using DietApp.Data;
using DietApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace DietApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = _context.Users.Find(id);
            if (user != null)
            {
                string date = DateTime.Now.ToString("dd/MM/yyyy");
                var existDC = _context.DailyConsumption.FirstOrDefault(d => d.Date.Equals(date) && d.User.Id.Equals(id));
                if (existDC == null)
                {
                    DailyConsumption dailyConsumption = new DailyConsumption()
                    {
                        UserId = id,
                        Date = date,
                        CaloricBalance = -user.BMR,
                    };
                    _context.DailyConsumption.Add(dailyConsumption);
                    await _context.SaveChangesAsync();
                }
            }
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}