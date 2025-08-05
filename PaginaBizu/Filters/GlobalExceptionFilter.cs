using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Claims;

namespace PaginaBizu.Filters
{
	public class LogActionFilter : IActionFilter
	{
		private readonly ILogger<LogActionFilter> _logger;

		public LogActionFilter(ILogger<LogActionFilter> logger)
		{
			_logger = logger;
		}

		public void OnActionExecuting(ActionExecutingContext context)
		{
			var user = context.HttpContext.User.Identity;
			var userName = user != null && user.IsAuthenticated
				? context.HttpContext.User.FindFirst(ClaimTypes.Name)?.Value ?? "Anónimo"
				: "No autenticado";

			var ip = context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Desconocida";

			var controller = context.RouteData.Values["controller"];
			var action = context.RouteData.Values["action"];

			_logger.LogInformation("➡️ Acción ejecutándose: {Controller}/{Action} | Usuario: {User} | IP: {IP} | Hora: {Time}",
				controller, action, userName, ip, DateTime.Now);
		}

		public void OnActionExecuted(ActionExecutedContext context)
		{
			var controller = context.RouteData.Values["controller"];
			var action = context.RouteData.Values["action"];

			_logger.LogInformation("✅ Acción ejecutada: {Controller}/{Action} | Hora: {Time}",
				controller, action, DateTime.Now);
		}
	}
}
