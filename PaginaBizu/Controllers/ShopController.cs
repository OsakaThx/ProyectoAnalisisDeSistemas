using Microsoft.AspNetCore.Mvc;
using PaginaBizu.Data;

namespace PaginaBizu.Controllers
{
	public class ShopController : Controller
	{
		private readonly AppDbContext _context;

		public ShopController(AppDbContext context)
		{
			_context = context;
		}

		// Muestra todos los productos
		public IActionResult Index()
		{
			var productos = _context.Products.ToList();
			return View(productos);
		}

		// Muestra los detalles de un producto
		public IActionResult Details(int id)
		{
			var producto = _context.Products.FirstOrDefault(p => p.Id == id);

			if (producto == null)
			{
				return NotFound();
			}

			return View(producto);
		}
	}
}
