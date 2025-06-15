using Microsoft.AspNetCore.Mvc;
using PaginaBizu.Data;

namespace PaginaBizu.Controllers
{
	public class ProductController : Controller
	{
		private readonly AppDbContext _context;

		public ProductController(AppDbContext context)
		{
			_context = context;
		}

		public IActionResult Index()
		{
			var productos = _context.Products.ToList(); // Asegúrate que esté en AppDbContext
			return View(productos);
		}
	}
}
