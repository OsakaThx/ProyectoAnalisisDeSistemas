using Microsoft.AspNetCore.Mvc;

namespace PaginaBizu.Controllers.Api
{
	/// <summary>
	/// Controlador de prueba para verificar que Swagger funciona correctamente.
	/// </summary>
	[ApiController]
	[Route("api/[controller]")]
	public class TestController : ControllerBase
	{
		/// <summary>
		/// Retorna un mensaje de prueba para verificar que el API está activo.
		/// </summary>
		/// <returns>Un mensaje "pong"</returns>
		[HttpGet("ping")]
		public IActionResult Ping()
		{
			return Ok("pong");
		}

		/// <summary>
		/// Suma dos números dados como parámetros.
		/// </summary>
		/// <param name="a">Primer número entero</param>
		/// <param name="b">Segundo número entero</param>
		/// <returns>Resultado de la suma</returns>
		[HttpGet("sumar")]
		public IActionResult Sumar(int a, int b)
		{
			int resultado = a + b;
			return Ok(new { Resultado = resultado });
		}
	}
}