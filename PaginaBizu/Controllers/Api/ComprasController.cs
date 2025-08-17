using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaginaBizu.Data;
using PaginaBizu.DTOs;
using PaginaBizu.Models;

namespace PaginaBizu.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Requiere autenticación
    public class ComprasController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ComprasController> _logger;

        public ComprasController(AppDbContext context, ILogger<ComprasController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost("procesar-compra")]
        public async Task<IActionResult> ProcesarCompra([FromBody] ProcesarCompraDTO compraDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Crear la orden
                var orden = new Order
                {
                    UsuarioId = compraDto.UsuarioId,
                    Fecha = DateTime.Now,
                    Estado = "Completada",
                    Total = compraDto.Items.Sum(i => i.Cantidad * i.PrecioUnitario),
                    OrderItems = compraDto.Items.Select(i => new OrderDetail
                    {
                        ProductId = i.ProductoId,
                        Cantidad = i.Cantidad,
                        PrecioUnitario = i.PrecioUnitario
                    }).ToList()
                };

                _context.Orders.Add(orden);
                await _context.SaveChangesAsync();

                // 2. Generar la factura
                var factura = new Factura
                {
                    OrderId = orden.Id,
                    NumeroFactura = $"FACT-{DateTime.Now:yyyyMMdd}-{orden.Id:D5}",
                    NombreCliente = compraDto.NombreCliente,
                    FechaEmision = DateTime.Now,
                    Subtotal = orden.Total,
                    Impuesto = orden.Total * 0.13m, // 13% de impuesto
                    Total = orden.Total * 1.13m,
                    Estado = "Pagada"
                };

                // Agregar detalles de la factura
                foreach (var item in orden.OrderItems)
                {
                    var producto = await _context.Products.FindAsync(item.ProductId);
                    
                    factura.Detalles.Add(new DetalleFactura
                    {
                        ProductoId = item.ProductId,
                        NombreProducto = producto?.NombreArticulo ?? "Producto no encontrado",
                        Cantidad = item.Cantidad,
                        PrecioUnitario = item.PrecioUnitario,
                        Subtotal = item.Cantidad * item.PrecioUnitario
                    });
                }

                _context.Facturas.Add(factura);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok(new 
                { 
                    success = true, 
                    mensaje = "Compra procesada exitosamente",
                    ordenId = orden.Id,
                    facturaId = factura.Id,
                    numeroFactura = factura.NumeroFactura
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error al procesar la compra");
                return StatusCode(500, new { success = false, mensaje = "Error al procesar la compra" });
            }
        }

        [HttpGet("factura/{id}")]
        public async Task<IActionResult> ObtenerFactura(int id)
        {
            var factura = await _context.Facturas
                .Include(f => f.Detalles)
                .Include(f => f.Orden)
                    .ThenInclude(o => o.OrderItems)
                    .ThenInclude(oi => oi.Producto)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (factura == null)
            {
                return NotFound(new { success = false, mensaje = "Factura no encontrada" });
            }

            return Ok(new { success = true, factura });
        }
    }
}
