using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;

namespace PaginaBizu.Models
{
	public class Order 
	{
		public int Id { get; set; }

		public string UsuarioId { get; set; }

		public DateTime Fecha { get; set; }

		public decimal Total { get; set; }

		public string Estado { get; set; } = "Pendiente";

		public List<OrderDetail> OrderItems { get; set; } = new List<OrderDetail>();

	}

	public class  OrderDetail
	{
		public int Id { get; set; }
		public int OrderId { get; set; }
		public int ProductId { get; set; }
		public int Cantidad { get; set; }
		public decimal PrecioUnitario { get; set; }
		public Product Producto { get; set; }
		}



}
