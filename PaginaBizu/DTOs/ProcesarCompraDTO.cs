using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PaginaBizu.DTOs
{
    public class ProcesarCompraDTO
    {
        [Required]
        public string UsuarioId { get; set; }
        
        [Required]
        public string NombreCliente { get; set; }
        
        [Required]
        public List<ItemCompraDTO> Items { get; set; } = new List<ItemCompraDTO>();
    }

    public class ItemCompraDTO
    {
        [Required]
        public int ProductoId { get; set; }
        
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser al menos 1")]
        public int Cantidad { get; set; }
        
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio unitario debe ser mayor a 0")]
        public decimal PrecioUnitario { get; set; }
    }
}
