using Microsoft.AspNetCore.Mvc;
using PaginaBizu.Data;
using PaginaBizu.Helpers;
using PaginaBizu.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;



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

		[HttpGet]
		public IActionResult CartCount()
		{
			var cart = HttpContext.Session.GetObject<List<CartItem>>("Cart") ?? new List<CartItem>();
			int totalItems = cart.Sum(c => c.Cantidad);
			return Json(totalItems);
		}

		[HttpPost]
		public async Task<IActionResult> CreateTestOrder()
		{
			try
			{
				// Get the first available user ID from the database
				var existingUserId = await _context.Users
					.Select(u => u.Id)
					.FirstOrDefaultAsync();

				if (string.IsNullOrEmpty(existingUserId))
				{
					return Json(new { success = false, message = "No users found in the database. Please create a user first." });
				}

				// Verify that product with ID 1 exists
				var productExists = await _context.Products.AnyAsync(p => p.Id == 1);
				if (!productExists)
				{
					return Json(new { success = false, message = "Product with ID 1 not found. Please ensure products are seeded in the database." });
				}

				var pedidoPrueba = new Order
				{
					UsuarioId = existingUserId, // Use the existing user ID
					Fecha = DateTime.Now,
					Estado = "Pendiente",
					Total = 100,
					OrderItems = new List<OrderDetail>
					{
						new OrderDetail
						{
							ProductId = 1,
							Cantidad = 2,
							PrecioUnitario = 50
						}
					}
				};

				_context.Orders.Add(pedidoPrueba);
				await _context.SaveChangesAsync();

				return Json(new { success = true, message = "Pedido de prueba creado exitosamente", orderId = pedidoPrueba.Id });
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = "Error al crear el pedido: " + ex.Message });
			}
		}
	}
}
