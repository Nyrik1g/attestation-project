using AttestationProject.Models;
using AttestationProject.Models.Dto;
using AttestationProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AttestationProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductApiController : ControllerBase
    {
        private readonly IProductService _srv;
        public ProductApiController(IProductService srv) => _srv = srv;

        [HttpGet, AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetAll()
        {
            var list = await _srv.GetAllAsync();
            return list.Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price
            }).ToList();
        }

        [HttpGet("{id:int}"), AllowAnonymous]
        public async Task<ActionResult<ProductDto>> Get(int id)
        {
            var p = await _srv.GetByIdAsync(id);
            if (p == null) return NotFound();
            return new ProductDto { Id = p.Id, Name = p.Name, Description = p.Description, Price = p.Price };
        }

        [HttpPost, Authorize(Roles = "Admin,User")]
        public async Task<ActionResult<ProductDto>> Create(ProductDto dto)
        {
            var p = new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price
            };
            var created = await _srv.CreateAsync(p);
            dto.Id = created.Id;
            return CreatedAtAction(nameof(Get), new { id = dto.Id }, dto);
        }

        [HttpPut("{id:int}"), Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> Update(int id, ProductDto dto)
        {
            if (id != dto.Id) return BadRequest();
            var ok = await _srv.UpdateAsync(id, new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price
            });
            if (!ok) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id:int}"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _srv.DeleteAsync(id);
            if (!ok) return NotFound();
            return NoContent();
        }
    }
}
