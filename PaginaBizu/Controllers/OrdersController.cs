using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaginaBizu.Data;
using PaginaBizu.Models;

namespace PaginaBizu.Controllers
{
	[Authorize(Roles = "Admin")]
	public class OrdersController : Controller
	{
		private readonly AppDbContext _context;

		public OrdersController(AppDbContext context)
		{
			_context = context;
		}

		// GET: Orders
		public IActionResult Index()
		{
			var pedidos = _context.Orders
				.Include(o => o.OrderItems)
					.ThenInclude(oi => oi.Producto) // Producto viene de OrderDetail
				.OrderByDescending(o => o.Fecha) // Opcional: ordena por fecha
				.ToList();

			return View(pedidos);
		}

		// GET: Orders/Details/5
		public IActionResult Details(int id)
		{
			var pedido = _context.Orders
	.Include(o => o.OrderItems)
		.ThenInclude(oi => oi.Producto) // Producto viene de OrderDetail
	.FirstOrDefault(o => o.Id == id);


			if (pedido == null)
			{
				return NotFound();
			}

			return View(pedido);
		}

		// GET: Orders/Edit/5
		public IActionResult Edit(int id)
		{
			var pedido = _context.Orders.Find(id);

			if (pedido == null)
			{
				return NotFound();
			}

			return View(pedido);
		}

		// POST: Orders/Edit/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Edit(int id, [Bind("Id,Estado")] Order order)
		{
			if (id != order.Id)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				var pedidoEnDb = _context.Orders.Find(id);
				if (pedidoEnDb == null)
					return NotFound();

				pedidoEnDb.Estado = order.Estado;
				_context.SaveChanges();

				return RedirectToAction(nameof(Index));
			}

			return View(order);
		}

		[Authorize(Roles = "Admin")]
		[Authorize(Roles = "Admin")]
		public IActionResult CrearPedidoPrueba()
		{
			var pedidoPrueba = new Order
			{
				UsuarioId = "usuarioPruebaId",
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
			_context.SaveChanges();

			return RedirectToAction("Index");
		}


	}
}
