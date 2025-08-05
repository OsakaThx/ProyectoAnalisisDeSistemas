using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaginaBizu.Data;
using PaginaBizu.Models;
using System.Linq;
using System.Threading.Tasks;

namespace PaginaBizu.Controllers.Api
{
	// Solo accesible para Admin, igual que el original
	[Authorize(Roles = "Admin")]
	[ApiController]
	[Route("api/[controller]")]
	public class ProductsApiController : ControllerBase
	{
		private readonly AppDbContext _context;

		public ProductsApiController(AppDbContext context)
		{
			_context = context;
		}

		// GET: api/ProductsApi
		[HttpGet]
		public async Task<IActionResult> GetProducts()
		{
			var productos = await _context.Products.ToListAsync();
			return Ok(productos);
		}

		// GET: api/ProductsApi/5
		[HttpGet("{id}")]
		public async Task<IActionResult> GetProduct(int id)
		{
			var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
			if (product == null)
				return NotFound();

			return Ok(product);
		}

		// POST: api/ProductsApi
		[HttpPost]
		public async Task<IActionResult> CreateProduct([FromBody] Product product)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			_context.Products.Add(product);
			await _context.SaveChangesAsync();

			return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
		}

		// PUT: api/ProductsApi/5
		[HttpPut("{id}")]
		public async Task<IActionResult> UpdateProduct(int id, [FromBody] Product product)
		{
			if (id != product.Id)
				return BadRequest("El ID del producto no coincide");

			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			_context.Entry(product).State = EntityState.Modified;

			try
			{
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!ProductExists(id))
					return NotFound();
				else
					throw;
			}

			return NoContent();
		}

		// DELETE: api/ProductsApi/5
		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteProduct(int id)
		{
			var product = await _context.Products.FindAsync(id);
			if (product == null)
				return NotFound();

			_context.Products.Remove(product);
			await _context.SaveChangesAsync();

			return NoContent();
		}

		private bool ProductExists(int id)
		{
			return _context.Products.Any(e => e.Id == id);
		}
	}
}
