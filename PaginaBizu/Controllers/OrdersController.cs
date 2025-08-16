using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaginaBizu.Data;
using PaginaBizu.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

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
		public async Task<IActionResult> Edit(int id, [Bind("Id,Estado")] Order order)
		{
			try
			{
				if (id != order.Id)
				{
					if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
						return Json(new { success = false, message = "ID de pedido no válido" });
					return NotFound();
				}

				if (string.IsNullOrEmpty(order.Estado))
				{
					ModelState.AddModelError("Estado", "El estado es requerido");
					if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
						return Json(new { success = false, message = "El estado es requerido" });
					return View(order);
				}

				var pedidoEnDb = await _context.Orders.FindAsync(id);
				if (pedidoEnDb == null)
				{
					if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
						return Json(new { success = false, message = "Pedido no encontrado" });
					return NotFound();
				}

				// Actualizar solo el estado
				pedidoEnDb.Estado = order.Estado;
				
				// Marcar solo el campo Estado como modificado
				_context.Entry(pedidoEnDb).Property(x => x.Estado).IsModified = true;
				
				await _context.SaveChangesAsync();
				
				var successMessage = $"Estado del pedido actualizado correctamente a: {order.Estado}";
				
				if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
				{
					return Json(new { 
						success = true, 
						message = successMessage,
						newStatus = order.Estado
					});
				}
				
				TempData["Success"] = successMessage;
				return RedirectToAction(nameof(Index));
			}
			catch (Exception ex)
			{
				// Log the error
				Console.WriteLine($"Error al actualizar el pedido: {ex.Message}");
				
				if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
				{
					return Json(new { 
						success = false, 
						message = "Ocurrió un error al actualizar el pedido. Por favor, inténtalo de nuevo."
					});
				}
				
				ModelState.AddModelError("", "Ocurrió un error al actualizar el pedido. Por favor, inténtalo de nuevo.");
				return View(order);
			}
		}

		// GET: Orders/Delete/5
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var order = await _context.Orders
				.Include(o => o.OrderItems)
				.ThenInclude(oi => oi.Producto)
				.FirstOrDefaultAsync(m => m.Id == id);

			if (order == null)
			{
				return NotFound();
			}

			return View(order);
		}

		// POST: Orders/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			var order = await _context.Orders.FindAsync(id);
			if (order == null)
			{
				return NotFound();
			}

			// Eliminar los OrderItems primero
			var orderItems = _context.OrderDetails.Where(od => od.OrderId == id);
			_context.OrderDetails.RemoveRange(orderItems);

			// Luego eliminar el pedido
			_context.Orders.Remove(order);
			await _context.SaveChangesAsync();

			TempData["Success"] = "Pedido eliminado correctamente.";
			return RedirectToAction(nameof(Index));
		}

		private bool OrderExists(int id)
		{
			return _context.Orders.Any(e => e.Id == id);
		}

		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> CrearPedidoPrueba()
		{
			try
			{
				// Get the first available user ID from the database
				var existingUserId = await _context.Users
					.OrderBy(u => u.Id)
					.Select(u => u.Id)
					.FirstOrDefaultAsync();

				if (string.IsNullOrEmpty(existingUserId))
				{
					TempData["Error"] = "No users found in the database. Please create a user first.";
					return RedirectToAction("Index");
				}

				// Verify that product with ID 1 exists
				var productExists = await _context.Products.AnyAsync(p => p.Id == 1);
				if (!productExists)
				{
					TempData["Error"] = "Product with ID 1 not found. Please ensure products are seeded in the database.";
					return RedirectToAction("Index");
				}

				var pedidoPrueba = new Order
				{
					UsuarioId = existingUserId,
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

				TempData["Success"] = "Test order created successfully!";
				return RedirectToAction("Index");
			}
			catch (Exception ex)
			{
				TempData["Error"] = $"Error creating test order: {ex.Message}";
				return RedirectToAction("Index");
			}
		}


	}
}
