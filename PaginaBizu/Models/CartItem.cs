using System.ComponentModel.DataAnnotations;

namespace PaginaBizu.Models
{
	public class CartItem
	{
		[Required]
		[Range(1, int.MaxValue, ErrorMessage = "El ProductId debe ser un número positivo.")]
		public int ProductId { get; set; }

		[Required(ErrorMessage = "El nombre del artículo es obligatorio.")]
		[StringLength(100, ErrorMessage = "El nombre del artículo no puede exceder 100 caracteres.")]
		public string NombreArticulo { get; set; }

		[Required]
		[Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor que cero.")]
		public decimal Precio { get; set; }

		[Range(1, 100, ErrorMessage = "La cantidad debe estar entre 1 y 100.")]
		public int Cantidad { get; set; } = 1;
	}
}
