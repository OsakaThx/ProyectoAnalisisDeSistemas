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

	var adminEmail = "hoshuacastillo48@gmail.com";
	var adminPassword = "Joshua0905."; // Cambia por una contraseña segura

	var adminUser = await userManager.FindByEmailAsync(adminEmail);

	if (adminUser == null)
	{
		adminUser = new ApplicationUser
		{
			UserName = adminEmail,
			Email = adminEmail,
			EmailConfirmed = true
		};

		var result = await userManager.CreateAsync(adminUser, adminPassword);

		if (!result.Succeeded)
		{
			throw new Exception("No se pudo crear el usuario admin: " + string.Join(", ", result.Errors.Select(e => e.Description)));
		}
	}

	if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
	{
		await userManager.AddToRoleAsync(adminUser, "Admin");
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
