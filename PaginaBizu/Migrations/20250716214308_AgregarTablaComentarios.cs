using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaginaBizu.Migrations
{
	/// <inheritdoc />
	public partial class AgregarTablaComentarios : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "Comentarios",
				columns: table => new
				{
					Id = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					ProductoId = table.Column<int>(type: "int", nullable: false),
					UsuarioId = table.Column<string>(type: "nvarchar(450)", nullable: false),
					Contenido = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
					Calificacion = table.Column<int>(type: "int", nullable: false),
					Fecha = table.Column<DateTime>(type: "datetime2", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Comentarios", x => x.Id);
					table.ForeignKey(
						name: "FK_Comentarios_AspNetUsers_UsuarioId",
						column: x => x.UsuarioId,
						principalTable: "AspNetUsers",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "FK_Comentarios_Products_ProductoId",
						column: x => x.ProductoId,
						principalTable: "Products",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateIndex(
				name: "IX_Comentarios_ProductoId",
				table: "Comentarios",
				column: "ProductoId");

			migrationBuilder.CreateIndex(
				name: "IX_Comentarios_UsuarioId",
				table: "Comentarios",
				column: "UsuarioId");
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "Comentarios");
		}
	}
}
