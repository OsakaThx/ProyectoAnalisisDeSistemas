using Microsoft.AspNetCore.Mvc;
using PaginaBizu.Data;
using PaginaBizu.Helpers;
using PaginaBizu.Models;



namespace PaginaBizu.Controllers
{
	public class CartController : Controller
	{
		private readonly AppDbContext _context;

		public CartController(AppDbContext context)
		{
			_context = context;
		}

		// Mostrar carrito
		public IActionResult Index()
		{
			var cart = HttpContext.Session.GetObject<List<CartItem>>("Cart") ?? new List<CartItem>();
			return View(cart);

		}

		// Modelo para recibir el id desde JSON
		public class AddToCartModel
		{
			public int Id { get; set; }
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult AddToCart([FromBody] AddToCartModel data)
		{
			if (data == null) return BadRequest();

			var product = _context.Products.FirstOrDefault(p => p.Id == data.Id);
			if (product == null) return NotFound();

			var cart = HttpContext.Session.GetObject<List<CartItem>>("Cart") ?? new List<CartItem>();

			var existingItem = cart.FirstOrDefault(c => c.ProductId == product.Id);
			if (existingItem != null)
			{
				existingItem.Cantidad++;
			}
			else
			{
				cart.Add(new CartItem
				{
					ProductId = product.Id,
					NombreArticulo = product.NombreArticulo,
					Precio = product.Precio,
					Cantidad = 1
				});
			}

			HttpContext.Session.SetObject("Cart", cart);

			return Json(new { success = true, count = cart.Sum(c => c.Cantidad) });
		}


	}
}
