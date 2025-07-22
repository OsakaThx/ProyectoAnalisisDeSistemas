using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PaginaBizu.Data;
using PaginaBizu.Models;

namespace PaginaBizu.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	[Authorize]
	public class ComentariosControllerApi : ControllerBase
	{
		private readonly AppDbContext _context;
		private readonly UserManager<ApplicationUser> _userManager;

		public ComentariosControllerApi(AppDbContext context, UserManager<ApplicationUser> userManager)
		{
			_context = context;
			_userManager = userManager;
		}

		[HttpPost]
		public async Task<IActionResult> Crear([FromBody] ComentarioCreateDto dto)
		{
			if (string.IsNullOrWhiteSpace(dto.Contenido) || dto.Calificacion < 1 || dto.Calificacion > 5)
			{
				return BadRequest("Contenido vacío o calificación inválida");
			}

			var usuario = await _userManager.GetUserAsync(User);
			if (usuario == null)
				return Unauthorized();

			var comentario = new Comentario
			{
				ProductoId = dto.ProductoId,
				Contenido = dto.Contenido,
				Calificacion = dto.Calificacion,
				UsuarioId = usuario.Id,
				Fecha = DateTime.Now
			};

			_context.Comentarios.Add(comentario);
			await _context.SaveChangesAsync();

			return Ok(new { mensaje = "Comentario creado correctamente" });
		}
	}

	// DTO para crear comentario
	public class ComentarioCreateDto
	{
		public int ProductoId { get; set; }
		public string Contenido { get; set; }
		public int Calificacion { get; set; }
	}
}
