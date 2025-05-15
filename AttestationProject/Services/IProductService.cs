using AttestationProject.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AttestationProject.Services
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetAllAsync();
        Task<Product?> GetByIdAsync(int id);
        Task<Product> CreateAsync(Product p);
        Task<bool> UpdateAsync(int id, Product p);
        Task<bool> DeleteAsync(int id);
    }
}
