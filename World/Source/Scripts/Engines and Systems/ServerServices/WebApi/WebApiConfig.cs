using System;
using System.Collections.Generic;

namespace Server.Engines.ServerServices.WebApi
{
	public sealed class WebApiConfig
	{
		public bool Enabled { get; private set; }
		public ServiceLogLevel LogLevel { get; private set; }
		public string[] Prefixes { get; private set; }
		public string ApiKey { get; private set; }
		public string[] CorsAllowedOrigins { get; private set; }
		public int RateLimitPerMinute { get; private set; }
		public bool LogRequests { get; private set; }

		public bool RequiresApiKey { get { return !string.IsNullOrEmpty(ApiKey); } }

		public string ListenerSignature
		{
			get { return Enabled + "|" + string.Join(" ", Prefixes); }
		}

		public static WebApiConfig FromJson(Dictionary<string, object> d)
		{
			return new WebApiConfig
			{
				Enabled = JsonHelper.GetBool(d, "enabled", false),
				LogLevel = ServiceLog.ParseLevel(JsonHelper.GetString(d, "log_level", null), ServiceLogLevel.Info),
				Prefixes = JsonHelper.GetStringArray(d, "prefixes", new[] { "http://localhost:8085/" }),
				ApiKey = JsonHelper.GetString(d, "api_key", ""),
				CorsAllowedOrigins = JsonHelper.GetStringArray(d, "cors_allowed_origins", new string[0]),
				RateLimitPerMinute = JsonHelper.GetInt(d, "rate_limit_per_minute", 120),
				LogRequests = JsonHelper.GetBool(d, "log_requests", false),
			};
		}

		public static WebApiConfig Defaults()
		{
			return FromJson(new Dictionary<string, object>());
		}

		public List<string> Validate()
		{
			var errors = new List<string>();

			if (Enabled && Prefixes.Length == 0)
				errors.Add("prefixes must contain at least one URL when enabled.");

			foreach (string prefix in Prefixes)
			{
				if (string.IsNullOrEmpty(prefix) || !prefix.EndsWith("/") || !(prefix.StartsWith("http://") || prefix.StartsWith("https://")))
					errors.Add(string.Format("prefix '{0}' must start with http:// or https:// and end with '/'.", prefix));
			}

			if (RateLimitPerMinute < 0)
				errors.Add("rate_limit_per_minute must be >= 0 (0 = unlimited).");

			return errors;
		}
	}
}
