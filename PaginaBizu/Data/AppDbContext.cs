using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PaginaBizu.Models;

namespace PaginaBizu.Data
{
	public class AppDbContext : IdentityDbContext<ApplicationUser>
	{
		public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

		public DbSet<Product> Products { get; set; }
	}
}
