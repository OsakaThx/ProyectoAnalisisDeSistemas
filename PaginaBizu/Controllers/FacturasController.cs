using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaginaBizu.Data;
using PaginaBizu.Models;

namespace PaginaBizu.Controllers
{
    [Authorize]
    public class FacturasController : Controller
    {
        private readonly AppDbContext _context;

        public FacturasController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("factura/{id}")]
        public async Task<IActionResult> Index(int id)
        {
            var factura = await _context.Facturas
                .Include(f => f.Detalles)
                .Include(f => f.Orden)
                    .ThenInclude(o => o.OrderItems)
                    .ThenInclude(oi => oi.Producto)
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Id == id);

            if (factura == null)
            {
                return NotFound();
            }

            // Map to the view model if needed
            var viewModel = new Factura
            {
                Id = factura.Id,
                NumeroFactura = factura.Id.ToString().PadLeft(6, '0'),
                FechaEmision = factura.FechaEmision,
                ClienteNombre = factura.ClienteNombre ?? string.Empty,
                ClienteEmail = factura.ClienteEmail ?? string.Empty,
                Subtotal = factura.Subtotal,
                Impuesto = factura.Impuestos,
                Total = factura.Total,
                Estado = factura.EstadoPago ?? "Pendiente",
                OrdenId = factura.OrdenId,
                Detalles = factura.Detalles?.Select(d => new DetalleFactura
                {
                    Id = d.Id,
                    ProductoId = d.ProductoId,
                    NombreProducto = d.NombreProducto ?? "Producto sin nombre",
                    Cantidad = d.Cantidad,
                    PrecioUnitario = d.PrecioUnitario,
                    Total = d.Total
                }).ToList() ?? new List<DetalleFactura>(),
                // Add any other properties that might be needed by the view
                MetodoPago = factura.MetodoPago ?? "No especificado",
                FechaPago = factura.FechaPago,
                DireccionEnvio = factura.DireccionEnvio
            };

            return View(viewModel);
        }
    }
}
