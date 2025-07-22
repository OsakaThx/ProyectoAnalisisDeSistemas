using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PaginaBizu.Data;
using PaginaBizu.Models;
using Serilog;
using PaginaBizu.Filters;

var builder = WebApplication.CreateBuilder(args);

// Configuración de Serilog para logging
Log.Logger = new LoggerConfiguration()
	.ReadFrom.Configuration(builder.Configuration)  // lee configuracion desde appsettings.json
	.WriteTo.Console()                              // logs en consola
	.WriteTo.File("logs/paginabizu.log", rollingInterval: RollingInterval.Day)  // logs en archivo
	.CreateLogger();


// Integrar Serilog al host de la aplicación
builder.Host.UseSerilog();

// Configuración de appsettings
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


// Configuracion de Swagger
// Este servicio habilita la exploración de endpoints de API para Swagger.
builder.Services.AddSession();


// Configuración de Swagger (documentación de API)
builder.Services.AddEndpointsApiExplorer(); // Permite explorar endpoints
builder.Services.AddSwaggerGen(c =>
{
	// Este bloque incluye los comentarios XML en Swagger si están habilitados en el .csproj
	var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
	var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
	c.IncludeXmlComments(xmlPath);
});
// Genera la documentación Swagger


// Configuración de Identity
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
	options.SignIn.RequireConfirmedAccount = false;
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


builder.Services.AddScoped<LogActionFilter>();

builder.Services.AddControllersWithViews(options =>
{
	options.Filters.Add<LogActionFilter>(); 
});

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


// Activación de Swagger (solo en desarrollo)

// Swagger genera la documentación de tu API y una interfaz web para probarla.
// Solo se activa en desarrollo para evitar exponer información sensible en producción.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();    // Habilita el middleware para generar el JSON de Swagger
	app.UseSwaggerUI();  // Habilita el frontend en /swagger para probar la API
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

// --- Usuarios y roles predefinidos ---
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
		("compa1@gmail.com", "Password123!"),
		("compa2@gmail.com", "Password456!")
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
