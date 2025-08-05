using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaginaBizu.Migrations
{
	/// <inheritdoc />
	public partial class InicialProductos : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "Products",
				columns: table => new
				{
					Id = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					NombreArticulo = table.Column<string>(type: "nvarchar(max)", nullable: false),
					CantidadDisponible = table.Column<int>(type: "int", nullable: false),
					Precio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
					Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
					ImagenUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
					Categoria = table.Column<string>(type: "nvarchar(max)", nullable: false),
					FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Products", x => x.Id);
				});
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "Products");
		}
	}
}
