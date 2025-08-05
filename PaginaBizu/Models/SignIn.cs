using System.ComponentModel.DataAnnotations;

namespace PaginaBizu.Models
{
	public class SignInModel
	{
		[Required(ErrorMessage = "El correo es obligatorio")]
		[EmailAddress(ErrorMessage = "Ingresa un correo válido")]
		public string Email { get; set; } = string.Empty;

		[Required(ErrorMessage = "La contraseña es obligatoria")]
		[DataType(DataType.Password)]
		[MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
		public string Password { get; set; } = string.Empty;

		[Required(ErrorMessage = "Debes confirmar la contraseña")]
		[DataType(DataType.Password)]
		[Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden")]
		public string ConfirmPassword { get; set; } = string.Empty;
	}
}
