// Pages/Account/Login.cshtml.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PaginaBizu.Models;
using System.ComponentModel.DataAnnotations;

public class LoginModel : PageModel
{
	private readonly SignInManager<ApplicationUser> _signInManager;

	public LoginModel(SignInManager<ApplicationUser> signInManager)
		=> _signInManager = signInManager;

	[BindProperty]
	public InputModel Input { get; set; } = new();

	public class InputModel
	{
		[Required, EmailAddress]
		public string Email { get; set; } = string.Empty;

		[Required, DataType(DataType.Password)]
		public string Password { get; set; } = string.Empty;

		public bool RememberMe { get; set; }
	}

	public async Task<IActionResult> OnPostAsync()
	{
		if (!ModelState.IsValid) return Page();

		// Identity busca en AspNetUsers y valida el PasswordHash
		var result = await _signInManager.PasswordSignInAsync(
						 Input.Email,
						 Input.Password,
						 Input.RememberMe,
						 lockoutOnFailure: false);

		if (result.Succeeded) return LocalRedirect("~/");
		if (result.IsLockedOut) ModelState.AddModelError("", "Cuenta bloqueada.");
		else ModelState.AddModelError("", "Credenciales inválidas.");

		return Page();
	}
}
