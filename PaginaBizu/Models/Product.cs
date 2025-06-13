namespace PaginaBizu.Models
{
	using System;
	using System.ComponentModel.DataAnnotations;

	public class Product
	{
		public int Id { get; set; }

		[Required(ErrorMessage = "El nombre del artículo es obligatorio.")]
		[StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres.")]
		public string NombreArticulo { get; set; }

		[Required(ErrorMessage = "La cantidad disponible es obligatoria.")]
		[Range(0, int.MaxValue, ErrorMessage = "La cantidad debe ser 0 o mayor.")]
		public int CantidadDisponible { get; set; }

		[Required(ErrorMessage = "El precio es obligatorio.")]
		[Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a cero.")]
		public decimal Precio { get; set; }

		[StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres.")]
		public string Descripcion { get; set; }

		
		public string ImagenUrl { get; set; }

		[StringLength(50, ErrorMessage = "La categoría no puede exceder 50 caracteres.")]
		public string Categoria { get; set; }

		public DateTime FechaCreacion { get; set; } = DateTime.Now;
	}


}
