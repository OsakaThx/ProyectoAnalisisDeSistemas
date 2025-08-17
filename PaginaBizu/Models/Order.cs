using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaginaBizu.Models
{
	public class Order 
	{
		public int Id { get; set; }

		public string UsuarioId { get; set; }

		public DateTime Fecha { get; set; }

		[Precision(18, 2)]
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
		[Precision(18, 2)]
		public decimal PrecioUnitario { get; set; }
		public Product Producto { get; set; }
		}



}
