namespace PaginaBizu.Models
{
	/// <summary>
	/// Modelo para manejar información de error en la vista de error.
	/// </summary>
	public class ErrorViewModel
	{
		/// <summary>
		/// Identificador único de la solicitud para seguimiento.
		/// </summary>
		public string? RequestId { get; set; }

		/// <summary>
		/// Indica si se debe mostrar el RequestId en la vista.
		/// </summary>
		public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
	}
}
