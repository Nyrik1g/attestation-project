using Microsoft.AspNetCore.Mvc;
using AttestationProject.Models;
using Microsoft.AspNetCore.Authorization;
using AttestationProject.Data;
using Microsoft.EntityFrameworkCore;

namespace AttestationProject.Controllers
{
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;

        public ProductController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Product
        public async Task<IActionResult> Index(decimal? minPrice, decimal? maxPrice)
        {
            var filtered = _context.Products.AsQueryable();

            if (minPrice.HasValue)
                filtered = filtered.Where(p => p.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                filtered = filtered.Where(p => p.Price <= maxPrice.Value);

            ViewData["minPrice"] = minPrice;
            ViewData["maxPrice"] = maxPrice;

            return View(await filtered.ToListAsync());
        }

        // GET: /Product/Create
        [Authorize]
        public IActionResult Create() => View();

        // POST: /Product/Create
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create(Product product, IFormFile imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                    var savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

                    using (var stream = new FileStream(savePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    product.ImagePath = "/images/" + fileName;
                }

                product.CreatedBy = User.Identity?.Name ?? "guest";
                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            return View(product);
        }

        // GET: /Product/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            if (product.CreatedBy != User.Identity?.Name && !User.IsInRole("Admin"))
                return Forbid();

            return View(product);
        }

        // POST: /Product/Edit/5
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Edit(int id, Product updatedProduct, IFormFile imageFile)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            if (product.CreatedBy != User.Identity?.Name && !User.IsInRole("Admin"))
                return Forbid();

            if (ModelState.IsValid)
            {
                product.Name = updatedProduct.Name;
                product.Description = updatedProduct.Description;
                product.Price = updatedProduct.Price;
                product.Category = updatedProduct.Category;
                product.Subcategory = updatedProduct.Subcategory;

                if (imageFile != null && imageFile.Length > 0)
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

                    using var stream = new FileStream(path, FileMode.Create);
                    await imageFile.CopyToAsync(stream);

                    product.ImagePath = "/images/" + fileName;
                }

                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(updatedProduct);
        }

        // GET: /Product/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            if (product.CreatedBy != User.Identity?.Name && !User.IsInRole("Admin"))
                return Forbid();

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // GET: /Product/MyProducts
        [Authorize]
        public async Task<IActionResult> MyProducts()
        {
            var username = User.Identity?.Name;
            var myProducts = await _context.Products
                .Where(p => p.CreatedBy == username)
                .ToListAsync();

            return View("Index", myProducts);
        }
    }
}
