using DietApp.Data;
using DietApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
            dailyConsumption.Reverse();
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
                    .Include(d => d.Meals).ThenInclude(m => m.Product)
                    .FirstOrDefaultAsync(d => d.DailyConsumptionId == id);
            if(dailyConsumption == null)
            {
                return NotFound();
            }
            double totalKcal = 0;
            double totalFat = 0;
            double totalCarbo = 0;
            double totalProtein = 0;
            foreach (var item in dailyConsumption.Meals)
            {
                totalKcal += item.TotalKcal;
                totalFat += item.TotalFat;
                totalCarbo += item.TotalCarbo;
                totalProtein += item.TotalProtein;
            }
            totalKcal = Math.Round(totalKcal,2);
            totalFat = Math.Round(totalFat,2);
            totalCarbo = Math.Round(totalCarbo,2);
            totalProtein = Math.Round(totalProtein,2);
            ViewBag.TotalKcal = totalKcal;
            ViewBag.TotalFat = totalFat;
            ViewBag.TotalCarbo = totalCarbo;
            ViewBag.TotalProtein = totalProtein;
            return View(dailyConsumption);
        }

        public IActionResult AddMeal(int? dailyId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (dailyId == null || userId == null)
            {
                return NotFound();
            }
            ViewBag.UserId = userId;
            ViewBag.DailyConsumptionId = (int)dailyId;
            ViewBag.Products = new SelectList(_context.Products, "ProductId", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMeal([Bind("MealId,UserId,ProductId,DailyConsumptionId,Weight")] Meal meal)
        {
            if (ModelState.IsValid)
            {
                var product = _context.Products.Find(meal.ProductId);
                meal.TotalKcal = Math.Round(product.KcalPer100*meal.Weight/100,2);
                meal.TotalFat = Math.Round(product.FatPer100 * meal.Weight / 100, 2);
                meal.TotalCarbo = Math.Round(product.CarboPer100 * meal.Weight / 100, 2);
                meal.TotalProtein = Math.Round(product.ProteinPer100 * meal.Weight / 100, 2);
                _context.Add(meal);
                var dailyConsumption = _context.DailyConsumption.Find(meal.DailyConsumptionId);
                if(dailyConsumption != null)
                {
                    dailyConsumption.CaloricBalance += meal.TotalKcal;
                    dailyConsumption.CaloricBalance = Math.Round(dailyConsumption.CaloricBalance, 2);
                    _context.Update(dailyConsumption);
                }
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(DailyConsumptionList));
            }
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return NotFound();
            }
            ViewBag.UserId = userId;
            ViewBag.DailyConsumptionId = meal.DailyConsumptionId;
            ViewBag.Products = new SelectList(_context.Products, "ProductId", "Name");
            return View(meal);
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMeal(int mealId, int dailyConsumptionId)
        {
            if (_context.Meals == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Meals'  is null.");
            }
            var meal = await _context.Meals.FindAsync(mealId);
            if (meal != null)
            {
                var dailyConsumption = _context.DailyConsumption.Find(dailyConsumptionId);
                dailyConsumption.CaloricBalance -= meal.TotalKcal;
                dailyConsumption.CaloricBalance = Math.Round(dailyConsumption.CaloricBalance, 2);
                _context.Meals.Remove(meal);
                _context.Update(dailyConsumption);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("DailyConsumption",new { id = dailyConsumptionId });
        }

        private bool UserExists(string id)
        {
            return _context.Users.Any(e => e.Id.Equals(id));
        }
    }
}
