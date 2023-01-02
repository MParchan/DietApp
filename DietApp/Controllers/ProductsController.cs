using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DietApp.Data;
using DietApp.Models;
using System.Drawing;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.SqlClient;
using System.Security.Claims;

namespace DietApp.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public ProductsController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        // GET: Products
        public async Task<IActionResult> Index(string order, string search, string currentFilter, int? pageNumber, int? toFavorite)
        {
            ViewBag.Favorites = new List<Product>();
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if(id != null)
            {
                var user = await _context.Users.Include(u => u.FavoriteProducts).FirstOrDefaultAsync(u => u.Id.Equals(id));
                ViewBag.Favorites = user.FavoriteProducts;
            }
            ViewData["Order"] = order;
            ViewData["NameSortParm"] = String.IsNullOrEmpty(order) ? "name_desc" : "";
            ViewData["KcalSortParm"] = order == "kcal" ? "kcal_desc" : "kcal";
            ViewData["FatSortParm"] = order == "fat" ? "fat_desc" : "fat";
            ViewData["CarboSortParm"] = order == "carbo" ? "carbo_desc" : "carbo";
            ViewData["ProteinSortParm"] = order == "protein" ? "protein_desc" : "protein";

            if (search != null)
            {
                pageNumber = 1;
            }
            else
            {
                search = currentFilter;
            }
            ViewData["CurrentFilter"] = search;

            var products = await _context.Products.ToListAsync();
            if (!String.IsNullOrEmpty(search))
            {
                products = products.Where(p => p.Name.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            switch (order)
            {
                case "kcal_desc":
                    products = products.OrderByDescending(s => s.KcalPer100).ToList();
                    break;
                case "kcal":
                    products = products.OrderBy(s => s.KcalPer100).ToList();
                    break;
                case "fat_desc":
                    products = products.OrderByDescending(s => s.FatPer100).ToList();
                    break;
                case "fat":
                    products = products.OrderBy(s => s.FatPer100).ToList();
                    break;
                case "carbo_desc":
                    products = products.OrderByDescending(s => s.CarboPer100).ToList();
                    break;
                case "carbo":
                    products = products.OrderBy(s => s.CarboPer100).ToList();
                    break;
                case "protein_desc":
                    products = products.OrderByDescending(s => s.ProteinPer100).ToList();
                    break;
                case "protein":
                    products = products.OrderBy(s => s.ProteinPer100).ToList();
                    break;
                case "name_desc":
                    products = products.OrderByDescending(s => s.Name).ToList();
                    break;
                default:
                    products = products.OrderBy(s => s.Name).ToList();
                    break;
            }
            int pageSize = 10;
            return View(PaginatedList<Product>.Create(products, pageNumber ?? 1, pageSize));
        }

        [Authorize]
        public async Task<IActionResult> FavoriteProducts()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.Users.Include(u => u.FavoriteProducts).FirstOrDefaultAsync(u => u.Id.Equals(id));
            var products = user.FavoriteProducts.ToList();
            return View(products);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> RemoveFavorite(int? id, bool? indexPage)
        {
            if (id == null)
            {
                return NotFound();
            }
            var product = _context.Products.Find(id);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.Users.Include(u => u.FavoriteProducts).FirstOrDefaultAsync(u => u.Id.Equals(userId));
            user.FavoriteProducts.Remove(product);
            _context.Update(user);
            await _context.SaveChangesAsync();
            if(indexPage == true)
            {
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction(nameof(FavoriteProducts));
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddFavorite(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var product = _context.Products.Find(id);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.Users.Include(u => u.FavoriteProducts).FirstOrDefaultAsync(u => u.Id.Equals(userId));
            user.FavoriteProducts.Add(product);
            _context.Update(user);
            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(Index));
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [Authorize]
        // GET: Products/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductId,Name,KcalPer100,FatPer100,CarboPer100,ProteinPer100,Image")] Product product)
        {
            if (ModelState.IsValid)
            {
                if (product.Image != null)
                {
                    string wwwRootPath = _hostEnvironment.WebRootPath;
                    string fileName = Path.GetFileNameWithoutExtension(product.Image.FileName);
                    string extension = Path.GetExtension(product.Image.FileName);
                    fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;
                    product.ImageName = fileName;
                    string path = Path.Combine(wwwRootPath + "/images/", fileName);
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(wwwRootPath + "/images/");
                    }
                    path = Path.Combine(wwwRootPath + "/images/", fileName);
                    using var fileStream = new FileStream(path, FileMode.Create);
                    await product.Image.CopyToAsync(fileStream);
                }
                product.KcalPer100 = product.FatPer100 * 9 + product.CarboPer100 * 4 + product.ProteinPer100 * 4;
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }



        // GET: Products/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductId,Name,KcalPer100,FatPer100,CarboPer100,ProteinPer100")] Product product)
        {
            if (id != product.ProductId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.ProductId))
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
            return View(product);
        }

        // GET: Products/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Products == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Products'  is null.");
            }
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                if (product.ImageName != null)
                {
                    var imagePath = Path.Combine(_hostEnvironment.WebRootPath, "images", product.ImageName);
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }
                _context.Products.Remove(product);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
          return _context.Products.Any(e => e.ProductId == id);
        }

        public IActionResult Comparison()
        {
            ViewBag.Products = new SelectList(_context.Products, "ProductId", "Name");
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ComparisonResult(int productId1, int productId2)
        {
            var product1 = await _context.Products.FindAsync(productId1);
            var product2 = await _context.Products.FindAsync(productId2);
            List<Product> products = new()
            {
                product1, 
                product2
            };
            return View(products);
        }

    }
}
