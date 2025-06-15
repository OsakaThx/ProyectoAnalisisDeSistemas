using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PaginaBizu.Data;
using PaginaBizu.Models;

var builder = WebApplication.CreateBuilder(args);

// Opcional: si quieres asegurarte de la carga strict
builder.Configuration
	   .SetBasePath(builder.Environment.ContentRootPath)
	   .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
	   .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
	   .AddEnvironmentVariables();

// Configuración de DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
	options.UseSqlServer(
		builder.Configuration.GetConnectionString("DefaultConnection"),
		sqlOptions => sqlOptions.EnableRetryOnFailure()
	)
);

// Identity igual que antes…
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
	options.SignIn.RequireConfirmedAccount = false;
	// Solo aflojar contraseña en Dev o Docker
	if (builder.Environment.IsDevelopment() || builder.Environment.EnvironmentName == "Docker")
	{
		options.Password.RequireDigit = false;
		options.Password.RequireLowercase = false;
		options.Password.RequireNonAlphanumeric = false;
		options.Password.RequireUppercase = false;
		options.Password.RequiredLength = 6;
	}
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddControllersWithViews();
builder.Services.AddSession();

var app = builder.Build();

// Crear roles/usuarios en Dev o Docker
if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Docker")
{
	await CreateAdminRolesAndUsers(app);
}

// Pipeline
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}"
);
app.MapRazorPages();

app.Run();

async Task CreateAdminRolesAndUsers(WebApplication app)
{
	using var scope = app.Services.CreateScope();
	var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
	var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
	var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

	if (!await roleManager.RoleExistsAsync("Admin"))
		await roleManager.CreateAsync(new IdentityRole("Admin"));

	var admins = new[]
	{
		("hoshuacastillo48@gmail.com", "Joshua0905."),
		("compa1@gmail.com",          "Password123!"),
		("compa2@gmail.com",          "Password456!")
	};

	foreach (var (email, pwd) in admins)
	{
		var user = await userManager.FindByEmailAsync(email);
		if (user == null)
		{
			user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true };
			var result = await userManager.CreateAsync(user, pwd);
			if (!result.Succeeded)
			{
				logger.LogError("Error creando usuario {Email}: {Errors}",
								email,
								string.Join("; ", result.Errors.Select(e => e.Description)));
				continue;
			}
		}
		if (!await userManager.IsInRoleAsync(user, "Admin"))
			await userManager.AddToRoleAsync(user, "Admin");
	}
}
