// Helpers/SessionExtensions.cs
using System.Text.Json;

namespace PaginaBizu.Helpers
{
	public static class SessionExtensions
	{
		public static void SetObject<T>(this ISession session, string key, T value)
		{
			session.SetString(key, JsonSerializer.Serialize(value));
		}

		public static T GetObject<T>(this ISession session, string key)
		{
			var value = session.GetString(key);
			if (value == null) return default;

			try
			{
				return JsonSerializer.Deserialize<T>(value);
			}
			catch (JsonException)
			{
				// Opcional: registrar el error
				return default;
			}
		}
	}
}
