using Microsoft.AspNetCore.Mvc;
using PaginaBizu.Data;
using PaginaBizu.Models;

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

			var comentarios = _context.Comentarios
				.Where(c => c.ProductoId == id)
				.OrderByDescending(c => c.Fecha)
				.ToList();

			var viewModel = new DetalleProductoViewModel
			{
				Producto = producto,
				Comentarios = comentarios
			};

			return View(viewModel);
		}
		[HttpPost]
		public IActionResult AgregarComentario(string Texto, int ProductoId)
		{
			if (string.IsNullOrWhiteSpace(Texto))
			{
				TempData["Error"] = "El comentario no puede estar vacío.";
				return RedirectToAction("Details", "Shop", new { id = ProductoId }); // Controlador explícito
			}

			var comentario = new Comentario
			{
				Contenido = Texto,
				Fecha = DateTime.Now,
				ProductoId = ProductoId,
				// Aquí asigna el usuario si usas identidad
			};

			_context.Comentarios.Add(comentario);
			_context.SaveChanges();

			// Redirige explícitamente a Shop/Details
			return RedirectToAction("Details", "Shop", new { id = ProductoId });
		}



	}
}
