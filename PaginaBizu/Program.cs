using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PaginaBizu.Data;
using PaginaBizu.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
	options.SignIn.RequireConfirmedAccount = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddControllersWithViews();
builder.Services.AddSession();

var app = builder.Build();

// Crear rol Admin y usuario admin automáticamente
using (var scope = app.Services.CreateScope())
{
	var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
	var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

	// Crear rol Admin si no existe
	if (!await roleManager.RoleExistsAsync("Admin"))
	{
		await roleManager.CreateAsync(new IdentityRole("Admin"));
	}

	var usuariosAdmin = new List<(string Email, string Password)>
{
	("hoshuacastillo48@gmail.com", "Joshua0905."),       // Tú
    ("compa1@gmail.com", "Password123!"),                // Compañero 1
    ("compa2@gmail.com", "Password456!")                 // Compañero 2
};

	foreach (var (email, password) in usuariosAdmin)
	{
		var user = await userManager.FindByEmailAsync(email);
		if (user == null)
		{
			user = new ApplicationUser
			{
				UserName = email,
				Email = email,
				EmailConfirmed = true
			};

			var result = await userManager.CreateAsync(user, password);
			if (!result.Succeeded)
			{
				throw new Exception($"No se pudo crear el usuario {email}: " + string.Join(", ", result.Errors.Select(e => e.Description)));
			}
		}

		if (!await userManager.IsInRoleAsync(user, "Admin"))
		{
			await userManager.AddToRoleAsync(user, "Admin");
		}
	}

}

// Middleware pipeline
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseSession(); // <-- ESTA LÍNEA HABILITA LA SESIÓN
app.UseAuthorization();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
