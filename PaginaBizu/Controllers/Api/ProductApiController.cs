using Microsoft.AspNetCore.Mvc;
using PaginaBizu.Data;
using PaginaBizu.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace PaginaBizu.Controllers.Api
{
	[ApiController]
	[Route("api/[controller]")]
	public class ProductApiController : ControllerBase
	{
		private readonly AppDbContext _context;

		public ProductApiController(AppDbContext context)
		{
			_context = context;
		}

		// GET: api/ProductApi
		[HttpGet]
		public async Task<IActionResult> Get()
		{
			var productos = await _context.Products.ToListAsync();
			return Ok(productos);
		}
	}
}
