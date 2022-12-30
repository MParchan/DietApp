using DietApp.Data;
using DietApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.Security.Claims;

namespace DietApp.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = _context.Users.Find(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        public async Task<IActionResult> DailyConsumptionList()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = _context.Users.Find(id);
            if (user == null)
            {
                return NotFound();
            }
            var dailyConsumption = await _context.DailyConsumption.Where(d => d.UserId.Equals(id)).ToListAsync();
            return View(dailyConsumption);
        }

        public async Task<IActionResult> DailyConsumption(int? id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = _context.Users.Find(userId);
            if (user == null || id == null)
            {
                return NotFound();
            }
            var dailyConsumption = await _context.DailyConsumption.Include(d => d.User)
                    .Include(d => d.Meals).FirstOrDefaultAsync(d => d.DailyConsumptionId == id);
            if(dailyConsumption == null)
            {
                return NotFound();
            }
            return View(dailyConsumption);
        }

        public IActionResult AddMeal(int? dailyId)
        {
            if(dailyId == null)
            {
                return NotFound();
            }
            ViewBag.DailyConsumptionId = dailyId;
            return View();
        }

        public IActionResult Calculate()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = _context.Users.Find(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Calculate([Bind("Id,Age,Height,Weight,Gender,Activity")] ApplicationUser user)
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!id.Equals(user.Id))
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var u = _context.Users.Find(id);
                u.Age = user.Age;
                u.Height = user.Height;
                u.Weight = user.Weight;
                u.Gender = user.Gender;
                u.Activity = user.Activity;
                double BMR;
                double BMI;
                if (user.Height > 0)
                {
                    BMI = user.Weight / Math.Pow((double)user.Height / 100, 2);
                    if (user.Gender == Gender.Male)
                    {
                        BMR = (66 + (13.7 * user.Weight) + (5 * user.Height) - (6.8 * user.Age)) * ((double)user.Activity) / 10;
                    }
                    else
                    {
                        BMR = (655 + (9.6 * user.Weight) + (1.8 * user.Height) - (4.7 * user.Age)) * ((double)user.Activity) / 10;
                    }
                }
                else
                {
                    BMR = 0;
                    BMI = 0;
                }
                u.BMR = Math.Round(BMR, 2);
                u.BMI = Math.Round(BMI, 2);
                try
                {
                    _context.Update(u);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        private bool UserExists(string id)
        {
            return _context.Users.Any(e => e.Id.Equals(id));
        }
    }
}
