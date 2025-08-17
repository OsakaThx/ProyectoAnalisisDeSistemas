using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PaginaBizu.Models
{
    public class Factura
    {
        public int Id { get; set; }
        public int? OrdenId { get; set; }
        public string NumeroFactura { get; set; } = string.Empty;
        public string NombreCliente { get; set; } = string.Empty;
        public string ClienteNombre { get => NombreCliente; set => NombreCliente = value; }
        public string ClienteEmail { get; set; } = string.Empty;
        public DateTime FechaEmision { get; set; } = DateTime.Now;
        
        [Precision(18, 2)]
        public decimal Subtotal { get; set; }
        
        [Precision(18, 2)]
        public decimal Impuesto { get; set; }
        
        [Precision(18, 2)]
        public decimal Impuestos { get => Impuesto; set => Impuesto = value; }
        
        [Precision(18, 2)]
        public decimal Total { get; set; }
        
        public string Estado { get; set; } = "Pendiente";
        public string EstadoPago { get => Estado; set => Estado = value; }
        public string MetodoPago { get; set; } = "Tarjeta de crédito";
        public DateTime? FechaPago { get; set; }
        public string? DireccionEnvio { get; set; }
        
        // Navigation properties
        public virtual Order? Orden { get; set; }
        public virtual ICollection<DetalleFactura> Detalles { get; set; } = new List<DetalleFactura>();
        
        // Alias for OrderId to match view expectations
        public int OrderId { get => OrdenId ?? 0; set => OrdenId = value; }
    }

    public class DetalleFactura
    {
        public int Id { get; set; }
        public int FacturaId { get; set; }
        public int ProductoId { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        
        [Precision(18, 2)]
        public decimal PrecioUnitario { get; set; }
        
        [Precision(18, 2)]
        public decimal Total { get; set; }
        
        [Precision(18, 2)]
        public decimal Subtotal { get => Total; set => Total = value; }
        
        // Navigation properties
        public virtual Factura Factura { get; set; } = null!;
        public virtual Product? Producto { get; set; }
    }
}
