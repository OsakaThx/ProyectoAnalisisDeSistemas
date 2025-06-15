// Controllers/AccountController.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PaginaBizu.Models;

namespace PaginaBizu.Controllers
{
	public class AccountController : Controller
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly SignInManager<ApplicationUser> _signInManager;
		private readonly RoleManager<IdentityRole> _roleManager;

		public AccountController(
			UserManager<ApplicationUser> userManager,
			SignInManager<ApplicationUser> signInManager,
			RoleManager<IdentityRole> roleManager)
		{
			_userManager = userManager;
			_signInManager = signInManager;
			_roleManager = roleManager;
		}

		[HttpGet]
		public IActionResult SingIn()
		{
			return View("~/Views/Shared/_SingIn.cshtml");
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> SingIn(SingInModel model)
		{
			if (!ModelState.IsValid)
				return View("~/Views/Shared/_SingIn.cshtml", model);

			// Validar si el correo ya existe
			var existingUser = await _userManager.FindByEmailAsync(model.Email);
			if (existingUser != null)
			{
				ModelState.AddModelError("Email", "Este correo ya está registrado.");
				return View("~/Views/Shared/_SingIn.cshtml", model);
			}

			// Validar que las contraseñas coincidan
			if (model.Password != model.ConfirmPassword)
			{
				ModelState.AddModelError("ConfirmPassword", "Las contraseñas no coinciden.");
				return View("~/Views/Shared/_SingIn.cshtml", model);
			}

			// Crear el usuario
			var user = new ApplicationUser
			{
				UserName = model.Email,
				Email = model.Email
			};

			var result = await _userManager.CreateAsync(user, model.Password);

			// Si falla la creación, mostrar errores (ej. contraseña débil)
			if (!result.Succeeded)
			{
				foreach (var e in result.Errors)
					ModelState.AddModelError(string.Empty, e.Description);

				return View("~/Views/Shared/_SingIn.cshtml", model);
			}

			// Verificar si el rol 'user' existe; si no, lo crea
			if (!await _roleManager.RoleExistsAsync("user"))
			{
				var roleResult = await _roleManager.CreateAsync(new IdentityRole("user"));
				if (!roleResult.Succeeded)
				{
					ModelState.AddModelError(string.Empty, "Error al crear el rol de usuario.");
					return View("~/Views/Shared/_SingIn.cshtml", model);
				}
			}

			// Asignar el rol al usuario
			await _userManager.AddToRoleAsync(user, "user");

			// Iniciar sesión automáticamente
			await _signInManager.SignInAsync(user, isPersistent: false);

			// Redirigir a la página principal
			return RedirectToAction("Index", "Home");
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Logout()
		{
			await _signInManager.SignOutAsync();
			return RedirectToAction("Index", "Home");
		}
		[HttpGet]
		public IActionResult Login()
		{
			return View("~/Views/Shared/_LoginPartialUser.cshtml");
		}

	}
}
