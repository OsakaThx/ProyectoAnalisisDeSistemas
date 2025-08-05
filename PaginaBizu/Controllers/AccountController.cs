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
		public IActionResult SignIn()
		{
			if (User.Identity.IsAuthenticated)
			{
				return RedirectToAction("Index", "Home");
			}

			return View("~/Views/Shared/_SignIn.cshtml");
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> SignIn(SignInModel model)
		{
			if (!ModelState.IsValid)
				return View("~/Views/Shared/_SignIn.cshtml", model);

			var existingUser = await _userManager.FindByEmailAsync(model.Email);
			if (existingUser != null)
			{
				ModelState.AddModelError("Email", "Este correo ya está registrado.");
				return View("~/Views/Shared/_SignIn.cshtml", model);
			}

			if (model.Password != model.ConfirmPassword)
			{
				ModelState.AddModelError("ConfirmPassword", "Las contraseñas no coinciden.");
				return View("~/Views/Shared/_SignIn.cshtml", model);
			}

			var user = new ApplicationUser
			{
				UserName = model.Email,
				Email = model.Email
			};

			var result = await _userManager.CreateAsync(user, model.Password);

			if (!result.Succeeded)
			{
				foreach (var e in result.Errors)
					ModelState.AddModelError(string.Empty, e.Description);

				return View("~/Views/Shared/_SignIn.cshtml", model);
			}

			if (!await _roleManager.RoleExistsAsync("user"))
			{
				var roleResult = await _roleManager.CreateAsync(new IdentityRole("user"));
				if (!roleResult.Succeeded)
				{
					ModelState.AddModelError(string.Empty, "Error al crear el rol de usuario.");
					return View("~/Views/Shared/_SignIn.cshtml", model);
				}
			}

			await _userManager.AddToRoleAsync(user, "user");
			await _signInManager.SignInAsync(user, isPersistent: false);

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
			if (User.Identity.IsAuthenticated)
			{
				return RedirectToAction("Index", "Home");
			}

			return View("~/Views/Shared/_LoginPartialUser.cshtml");
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Login(string email, string password, bool rememberMe)
		{
			if (!ModelState.IsValid)
				return View("~/Views/Shared/_LoginPartialUser.cshtml");

			var result = await _signInManager.PasswordSignInAsync(email, password, rememberMe, lockoutOnFailure: true);

			if (result.Succeeded)
			{
				return RedirectToAction("Index", "Home");
			}

			ModelState.AddModelError(string.Empty, "Correo o contraseña incorrectos.");
			return View("~/Views/Shared/_LoginPartialUser.cshtml");
		}
	}
}
