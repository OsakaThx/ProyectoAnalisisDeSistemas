using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PaginaBizu.Data;
using PaginaBizu.Models;

namespace PaginaBizu.Controllers
{
	[Authorize] // Solo usuarios autenticados pueden comentar
	public class ComentariosController : Controller
	{
		private readonly AppDbContext _context;
		private readonly UserManager<ApplicationUser> _userManager;

		public ComentariosController(AppDbContext context, UserManager<ApplicationUser> userManager)
		{
			_context = context;
			_userManager = userManager;
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Crear(int ProductoId, string Contenido, int Calificacion)
		{
			if (string.IsNullOrWhiteSpace(Contenido) || Calificacion < 1 || Calificacion > 5)
			{
				
				return RedirectToAction("Details", "Shop", new { id = ProductoId });
			}

			var usuario = await _userManager.GetUserAsync(User);

			var comentario = new Comentario
			{
				ProductoId = ProductoId,
				Contenido = Contenido,
				Calificacion = Calificacion,
				UsuarioId = usuario.Id,
				Fecha = DateTime.Now
			};

			_context.Comentarios.Add(comentario);
			await _context.SaveChangesAsync();

			return RedirectToAction("Details", "Shop", new { id = ProductoId });
		}
	}
}
