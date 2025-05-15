using AttestationProject.Data;
using AttestationProject.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AttestationProject.Services
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _db;
        public ProductService(AppDbContext db) => _db = db;

        public async Task<IEnumerable<Product>> GetAllAsync()
            => await _db.Products.AsNoTracking().ToListAsync();

        public async Task<Product?> GetByIdAsync(int id)
            => await _db.Products.FindAsync(id);

        public async Task<Product> CreateAsync(Product p)
        {
            _db.Products.Add(p);
            await _db.SaveChangesAsync();
            return p;
        }

        public async Task<bool> UpdateAsync(int id, Product p)
        {
            var exist = await _db.Products.FindAsync(id);
            if (exist == null) return false;
            exist.Name = p.Name;
            exist.Description = p.Description;
            exist.Price = p.Price;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var exist = await _db.Products.FindAsync(id);
            if (exist == null) return false;
            _db.Products.Remove(exist);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
