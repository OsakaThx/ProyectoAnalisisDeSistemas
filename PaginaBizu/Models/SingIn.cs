using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

// Models/SingInModel.cs


namespace PaginaBizu.Models
{
	public class SingInModel
	{
		[Required, EmailAddress]
		public string Email { get; set; } = string.Empty;

		[Required, DataType(DataType.Password)]
		public string Password { get; set; } = string.Empty;

		[Required, DataType(DataType.Password), Compare(nameof(Password))]
		public string ConfirmPassword { get; set; } = string.Empty;
	}
}
