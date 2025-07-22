using Microsoft.AspNetCore.Mvc;
using PaginaBizu.Data;
using PaginaBizu.Models;
using System.Linq;

namespace PaginaBizu.Controllers.Api
{
	[ApiController]
	[Route("api/[controller]")]
	public class ShopApiController : ControllerBase
	{
		private readonly AppDbContext _context;

		public ShopApiController(AppDbContext context)
		{
			_context = context;
		}

		// GET: api/ShopApi
		[HttpGet]
		public IActionResult GetProducts()
		{
			var productos = _context.Products.ToList();
			return Ok(productos);
		}

		// GET: api/ShopApi/5
		[HttpGet("{id}")]
		public IActionResult GetProductDetails(int id)
		{
			var producto = _context.Products.FirstOrDefault(p => p.Id == id);
			if (producto == null)
				return NotFound();

			var comentarios = _context.Comentarios
				.Where(c => c.ProductoId == id)
				.OrderByDescending(c => c.Fecha)
				.ToList();

			var result = new
			{
				Producto = producto,
				Comentarios = comentarios
			};

			return Ok(result);
		}

		// POST: api/ShopApi/AgregarComentario
		[HttpPost("AgregarComentario")]
		public IActionResult AgregarComentario([FromBody] ComentarioDto comentarioDto)
		{
			if (string.IsNullOrWhiteSpace(comentarioDto.Texto))
				return BadRequest("El comentario no puede estar vacío.");

			var comentario = new Comentario
			{
				Contenido = comentarioDto.Texto,
				Fecha = System.DateTime.Now,
				ProductoId = comentarioDto.ProductoId,
				// Asignar UsuarioId si usas identidad
			};

			_context.Comentarios.Add(comentario);
			_context.SaveChanges();

			return Ok(new { mensaje = "Comentario agregado correctamente" });
		}
	}

	// DTO para recibir datos en el POST
	public class ComentarioDto
	{
		public string Texto { get; set; }
		public int ProductoId { get; set; }
	}
}
