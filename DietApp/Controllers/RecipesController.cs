using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DietApp.Data;
using DietApp.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using NuGet.ContentModel;
using static System.Net.WebRequestMethods;
using System.Reflection.Metadata;
using Microsoft.Extensions.Hosting;
using System.Globalization;
using NuGet.Packaging;

namespace DietApp.Controllers
{
    public class RecipesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public RecipesController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        // GET: Recipes
        public async Task<IActionResult> Index(string search, string currentFilter, int? pageNumber, string category)
        {
            ViewBag.Categories = _context.Categories;
            if (search != null)
            {
                pageNumber = 1;
            }
            else
            {
                search = currentFilter;
            }
            ViewData["CurrentFilter"] = search;
            ViewData["CurrentCategory"] = category;
            var recipes = await _context.Recipes.Include(r => r.User).Include(r => r.Category).ToListAsync();
            if (!String.IsNullOrEmpty(search))
            {
                recipes = recipes.Where(r => r.Title.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            if (!String.IsNullOrEmpty(category))
            {
                var cat = _context.Categories.FirstOrDefault(c => c.Name.Equals(category));
                recipes = recipes.Where(r => r.Category.Any(i => i.CategoryId == cat.CategoryId) == true).ToList();
            }

            int pageSize = 10;
            return View(PaginatedList<Recipe>.Create(recipes, pageNumber ?? 1, pageSize));
        }

        // GET: Recipes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Recipes == null)
            {
                return NotFound();
            }
            ViewBag.RecipeId = id;
            var recipe = await _context.Recipes
                .Include(r => r.User)
                .Include(r => r.Comments)
                    .ThenInclude(c => c.User)
                .Include(r => r.Ingredients)
                    .ThenInclude(i => i.Product)
                .Include(r => r.Category)
                .FirstOrDefaultAsync(m => m.RecipeId == id);
            if (recipe == null)
            {
                return NotFound();
            }
            ViewBag.CommentsCount = recipe.Comments.Count();
            if(ViewBag.CommentsCount == 0)
            {
                ViewBag.Rating = 0;
            }
            else
            {
                double rating = 0.0;
                foreach (var comment in recipe.Comments)
                {
                    rating += comment.Rating;
                }
                NumberFormatInfo nfi = new()
                {
                    NumberDecimalSeparator = "."
                };
                ViewBag.Rating = (Math.Round(rating / ViewBag.CommentsCount, 1)).ToString(nfi); ;
            }
            return View(recipe);
        }

        // GET: Recipes/Create
        [Authorize]
        public IActionResult Create()
        {
            ViewData["Products"] = new SelectList(_context.Products, "ProductId", "Name");
            ViewData["Categories"] = new SelectList(_context.Categories, "CategoryId", "Name");
            return View();
        }

        // POST: Recipes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RecipeId,Title,Image,Description,DifficultyLevel,PreparationTime,Portions,Ingredients")] Recipe recipe, int[] categoriesId)
        {
            if (ModelState.IsValid)
            {
                if (recipe.Image != null)
                {
                    string wwwRootPath = _hostEnvironment.WebRootPath;
                    string fileName = Path.GetFileNameWithoutExtension(recipe.Image.FileName);
                    string extension = Path.GetExtension(recipe.Image.FileName);
                    fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;
                    recipe.ImageName = fileName;
                    string path = Path.Combine(wwwRootPath + "/images/", fileName);
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(wwwRootPath + "/images/");
                    }
                    path = Path.Combine(wwwRootPath + "/images/", fileName);
                    using var fileStream = new FileStream(path, FileMode.Create);
                    await recipe.Image.CopyToAsync(fileStream);
                }
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                recipe.UserId = userId;
                double totalKcal = 0;
                double totalFat = 0;
                double totalCarbo = 0;
                double totalProtein = 0;
                int totalWeight = 0;
                foreach (var item in recipe.Ingredients)
                {
                    totalKcal += _context.Products.Find(item.ProductId).KcalPer100 * item.Weight/100;
                    totalFat += _context.Products.Find(item.ProductId).FatPer100 * item.Weight / 100;
                    totalCarbo += _context.Products.Find(item.ProductId).CarboPer100 * item.Weight / 100;
                    totalProtein += _context.Products.Find(item.ProductId).ProteinPer100 * item.Weight / 100;
                    totalWeight += item.Weight;
                    using (var transaction = _context.Database.BeginTransaction())
                    {
                        _context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.RecipeIngredients ON;");
                        _context.Add(item);
                        transaction.Commit();
                    }
                }
                List<Category> categories = new(); 
                foreach (var item in categoriesId)
                {
                    Category category = _context.Categories.Find(item);
                    categories.Add(category);
                }
                recipe.Category = categories;
                recipe.TotalKcal = Math.Round(totalKcal, 2);
                recipe.TotalFat = Math.Round(totalFat, 2);
                recipe.TotalCarbo = Math.Round(totalCarbo, 2);
                recipe.TotalProtein = Math.Round(totalProtein, 2);
                recipe.TotalWeight = totalWeight;                
                _context.Add(recipe);
                await _context.SaveChangesAsync();

                Product product = new();
                product.Name = recipe.Title;
                product.KcalPer100 = Math.Round(recipe.TotalKcal / recipe.TotalWeight * 100, 2);
                product.FatPer100 = Math.Round(recipe.TotalFat / recipe.TotalWeight * 100, 2);
                product.CarboPer100 = Math.Round(recipe.TotalCarbo / recipe.TotalWeight * 100, 2);
                product.ProteinPer100 = Math.Round(recipe.TotalProtein / recipe.TotalWeight * 100, 2);
                product.Image = recipe.Image;
                product.ImageName = recipe.ImageName;
                product.RecipeId = recipe.RecipeId;
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Products"] = new SelectList(_context.Products, "ProductId", "Name");
            ViewData["Categories"] = new SelectList(_context.Categories, "CategoryId", "Name");
            return View(recipe);
        }

        // GET: Recipes/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Recipes == null)
            {
                return NotFound();
            }

            var recipe = await _context.Recipes.FindAsync(id);
            if (recipe == null)
            {
                return NotFound();
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "UserName", recipe.UserId);
            return View(recipe);
        }

        // POST: Recipes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("RecipeId,UserId,Title,Description,DifficultyLevel,PreparationTime,Portions,Ingredients")] Recipe recipe)
        {
            if (id != recipe.RecipeId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(recipe);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RecipeExists(recipe.RecipeId))
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
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "UserName", recipe.UserId);
            return View(recipe);
        }

        // GET: Recipes/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Recipes == null)
            {
                return NotFound();
            }

            var recipe = await _context.Recipes
                .Include(r => r.User)
                .FirstOrDefaultAsync(m => m.RecipeId == id);
            if (recipe == null)
            {
                return NotFound();
            }

            return View(recipe);
        }

        // POST: Recipes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Recipes == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Recipes'  is null.");
            }
            var recipe = await _context.Recipes.FindAsync(id);
            if (recipe != null)
            {
                if (recipe.ImageName != null)
                {
                    var imagePath = Path.Combine(_hostEnvironment.WebRootPath, "images", recipe.ImageName);
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }
                var product = await _context.Products.FirstOrDefaultAsync(p => p.RecipeId == id);
                if(product != null)
                {
                    _context.Products.Remove(product);
                }
                _context.Recipes.Remove(recipe);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        public IActionResult AddComment()
        {
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment([Bind("CommentId,RecipeId,Content,Rating")] Comment comment)
        {
            var recipe = await _context.Recipes
                .Include(r => r.User)
                .Include(r => r.Comments)
                .Include(r => r.Ingredients)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(m => m.RecipeId == comment.RecipeId);
            ViewBag.RecipeId = comment.RecipeId;
            if (ModelState.IsValid)
            {
                comment.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                comment.Date = DateTime.Now;
                _context.Add(comment);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Details", new { id = comment.RecipeId });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteComment(int commentId)
        {
            Console.WriteLine("--------------------------------");
            Console.WriteLine(commentId);
            if (_context.Comments == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Comments'  is null.");
            }
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null)
            {
                return NotFound();
            }
            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new  { id = comment.RecipeId });
        }

        private bool RecipeExists(int id)
        {
          return _context.Recipes.Any(e => e.RecipeId == id);
        }
    }
}
