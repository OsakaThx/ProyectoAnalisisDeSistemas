using System.ComponentModel.DataAnnotations;

namespace PaginaBizu.Models
{
	public class Comentario
	{
		public int Id { get; set; }

		[Required]
		public int ProductoId { get; set; }

		public Product Producto { get; set; }

		public string UsuarioId { get; set; }

		public ApplicationUser Usuario { get; set; }

		[Required(ErrorMessage = "El comentario no puede estar vacío.")]
		[StringLength(500)]
		public string Contenido { get; set; }

		[Range(1, 5)]
		public int Calificacion { get; set; }

		public DateTime Fecha { get; set; } = DateTime.Now;
	}
}


