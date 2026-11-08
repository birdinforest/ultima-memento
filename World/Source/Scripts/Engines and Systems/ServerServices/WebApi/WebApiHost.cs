using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Net;
using System.Text;
using Server.Items;

namespace Server.Engines.ServerServices.WebApi
{
	public sealed class ApiRequest
	{
		public string Method { get; internal set; }
		public string Path { get; internal set; }
		public NameValueCollection Query { get; internal set; }
		public string RemoteAddress { get; internal set; }
		public HttpListenerRequest Raw { get; internal set; }

		public string GetHeader(string name)
		{
			return Raw.Headers[name];
		}
	}

	public sealed class ApiException : Exception
	{
		public int Status { get; private set; }
		public string Code { get; private set; }

		public ApiException(int status, string code, string message) : base(message)
		{
			Status = status;
			Code = code;
		}
	}

	/// <summary>
	/// Handlers run on HTTP worker threads. They must not touch live game objects;
	/// read snapshots that systems publish from the game thread instead.
	/// </summary>
	public delegate object ApiHandler(ApiRequest request);

	/// <summary>
	/// Shared HTTP host for read-only web APIs, configured by Data/System/CFG/Services/web-api.json.
	/// Systems call <see cref="Register"/> to expose endpoints.
	/// </summary>
	public static class WebApiHost
	{
		public const string LogSystem = "WebApi";
		public const string ApiRoot = "/api/v1";

		private sealed class Route
		{
			public string Method;
			public string Path;
			public string Description;
			public ApiHandler Handler;
		}

		private sealed class RateWindow
		{
			public DateTime Start;
			public int Count;
		}

		private static readonly JsonConfigFile<WebApiConfig> m_File;
		private static readonly object m_RouteLock = new object();
		private static readonly object m_ListenerLock = new object();
		private static readonly Dictionary<string, RateWindow> m_RateWindows = new Dictionary<string, RateWindow>();

		private static volatile Dictionary<string, Route> m_Routes = new Dictionary<string, Route>(StringComparer.OrdinalIgnoreCase);
		private static volatile WebApiConfig m_Config;
		private static HttpListener m_Listener;

		static WebApiHost()
		{
			m_File = ConfigService.Register("web-api", "web-api.json", WebApiConfig.FromJson, WebApiConfig.Defaults, c => c.Validate());
			m_Config = m_File.Current;
			ServiceLog.SetLevel(LogSystem, m_Config.LogLevel);
			m_File.Changed += OnConfigChanged;
		}

		public static void Initialize()
		{
			Register("GET", ApiRoot, IndexHandler, "Lists available endpoints.");
			Register("GET", ApiRoot + "/health", HealthHandler, "Server uptime and config file status.");

			EventSink.Shutdown += delegate { StopListener("server shutdown"); };
			EventSink.Crashed += delegate { StopListener("server crash"); };

			if (m_Config.Enabled)
				StartListener(m_Config);
			else
				ServiceLog.Info(LogSystem, "Web API is disabled (web-api.json enabled=false).");
		}

		public static bool IsListening
		{
			get { lock (m_ListenerLock) return m_Listener != null && m_Listener.IsListening; }
		}

		public static void Register(string method, string path, ApiHandler handler, string description)
		{
			string normalized = NormalizePath(path);
			string key = RouteKey(method, normalized);

			lock (m_RouteLock)
			{
				var routes = new Dictionary<string, Route>(m_Routes, StringComparer.OrdinalIgnoreCase);
				if (routes.ContainsKey(key))
					ServiceLog.Warn(LogSystem, "Route {0} registered twice; replacing.", key);

				routes[key] = new Route { Method = method.ToUpperInvariant(), Path = normalized, Description = description, Handler = handler };
				m_Routes = routes;
			}
		}

		private static void OnConfigChanged(WebApiConfig previous, WebApiConfig next)
		{
			ServiceLog.SetLevel(LogSystem, next.LogLevel);
			m_Config = next;

			if (previous.ListenerSignature != next.ListenerSignature)
			{
				StopListener("config changed");
				if (next.Enabled)
					StartListener(next);
			}

			if (previous.ApiKey != next.ApiKey)
				ServiceLog.Info(LogSystem, "API key {0}.", next.RequiresApiKey ? "set/changed" : "removed (no auth)");
			if (string.Join(",", previous.CorsAllowedOrigins) != string.Join(",", next.CorsAllowedOrigins))
				ServiceLog.Info(LogSystem, "CORS origins: [{0}]", string.Join(", ", next.CorsAllowedOrigins));
			if (previous.RateLimitPerMinute != next.RateLimitPerMinute)
				ServiceLog.Info(LogSystem, "rate_limit_per_minute: {0} -> {1}", previous.RateLimitPerMinute, next.RateLimitPerMinute);
		}

		private static void StartListener(WebApiConfig config)
		{
			lock (m_ListenerLock)
			{
				if (m_Listener != null)
					return;

				if (!HttpListener.IsSupported)
				{
					ServiceLog.Error(LogSystem, "HttpListener is not supported on this platform.");
					return;
				}

				var listener = new HttpListener();
				try
				{
					foreach (string prefix in config.Prefixes)
						listener.Prefixes.Add(prefix);

					listener.Start();
					m_Listener = listener;
					listener.BeginGetContext(OnContext, listener);

					ServiceLog.Info(LogSystem, "Listening on {0}{1}", string.Join(", ", config.Prefixes), config.RequiresApiKey ? " (API key required)" : "");
				}
				catch (Exception ex)
				{
					ServiceLog.Error(LogSystem, "Failed to start listener on {0}: {1}. On Windows, non-localhost prefixes need 'netsh http add urlacl'.",
						string.Join(", ", config.Prefixes), ex.Message);
					try { listener.Close(); } catch { }
				}
			}
		}

		private static void StopListener(string reason)
		{
			lock (m_ListenerLock)
			{
				if (m_Listener == null)
					return;

				try
				{
					m_Listener.Stop();
					m_Listener.Close();
				}
				catch (Exception ex)
				{
					ServiceLog.Warn(LogSystem, "Error stopping listener: {0}", ex.Message);
				}

				m_Listener = null;
				ServiceLog.Info(LogSystem, "Listener stopped ({0}).", reason);
			}
		}

		private static void OnContext(IAsyncResult result)
		{
			var listener = (HttpListener)result.AsyncState;
			HttpListenerContext context;

			try
			{
				context = listener.EndGetContext(result);
			}
			catch (Exception)
			{
				return; // Listener stopped.
			}

			try
			{
				if (listener.IsListening)
					listener.BeginGetContext(OnContext, listener);
			}
			catch (Exception)
			{
			}

			Handle(context);
		}

		private static void Handle(HttpListenerContext context)
		{
			Stopwatch watch = Stopwatch.StartNew();
			WebApiConfig config = m_Config;
			HttpListenerRequest request = context.Request;
			HttpListenerResponse response = context.Response;
			string remote = request.RemoteEndPoint != null ? request.RemoteEndPoint.Address.ToString() : "?";
			string path = NormalizePath(request.Url.AbsolutePath);
			int status = 200;

			try
			{
				ApplyCors(config, request, response);

				if (request.HttpMethod == "OPTIONS")
				{
					status = 204;
					WriteEmpty(response, status);
					return;
				}

				if (!CheckRateLimit(config, remote))
					throw new ApiException(429, "rate_limited", "Too many requests; slow down.");

				if (config.RequiresApiKey && !HasValidKey(config, request))
					throw new ApiException(401, "unauthorized", "Missing or invalid API key (X-Api-Key header).");

				Dictionary<string, Route> routes = m_Routes;
				Route route;
				if (!routes.TryGetValue(RouteKey(request.HttpMethod, path), out route))
				{
					foreach (Route r in routes.Values)
					{
						if (string.Equals(r.Path, path, StringComparison.OrdinalIgnoreCase))
							throw new ApiException(405, "method_not_allowed", "Method not allowed.");
					}

					throw new ApiException(404, "not_found", "No endpoint at " + path + ". See " + ApiRoot + ".");
				}

				var apiRequest = new ApiRequest
				{
					Method = request.HttpMethod,
					Path = path,
					Query = request.QueryString,
					RemoteAddress = remote,
					Raw = request,
				};

				WriteJson(response, 200, route.Handler(apiRequest));
			}
			catch (ApiException ex)
			{
				status = ex.Status;
				WriteJson(response, status, ErrorBody(ex.Code, ex.Message));
			}
			catch (Exception ex)
			{
				status = 500;
				ServiceLog.Error(LogSystem, "{0} {1} failed: {2}", request.HttpMethod, path, ex);
				WriteJson(response, status, ErrorBody("internal_error", "Internal server error."));
			}
			finally
			{
				if (config.LogRequests)
					ServiceLog.Info(LogSystem, "{0} {1} {2} {3}ms from {4}", request.HttpMethod, path, status, watch.ElapsedMilliseconds, remote);
			}
		}

		private static object ErrorBody(string code, string message)
		{
			return new Dictionary<string, object>
			{
				{ "error", new Dictionary<string, object> { { "code", code }, { "message", message } } },
			};
		}

		private static void ApplyCors(WebApiConfig config, HttpListenerRequest request, HttpListenerResponse response)
		{
			string origin = request.Headers["Origin"];
			if (string.IsNullOrEmpty(origin) || config.CorsAllowedOrigins.Length == 0)
				return;

			string allowed = null;
			foreach (string candidate in config.CorsAllowedOrigins)
			{
				if (candidate == "*")
					allowed = "*";
				else if (string.Equals(candidate.TrimEnd('/'), origin.TrimEnd('/'), StringComparison.OrdinalIgnoreCase))
				{
					allowed = origin;
					break;
				}
			}

			if (allowed == null)
				return;

			response.AddHeader("Access-Control-Allow-Origin", allowed);
			response.AddHeader("Access-Control-Allow-Methods", "GET, OPTIONS");
			response.AddHeader("Access-Control-Allow-Headers", "X-Api-Key, Authorization, Content-Type");
			response.AddHeader("Access-Control-Max-Age", "600");
			if (allowed != "*")
				response.AddHeader("Vary", "Origin");
		}

		private static bool CheckRateLimit(WebApiConfig config, string remote)
		{
			if (config.RateLimitPerMinute <= 0)
				return true;

			DateTime now = DateTime.UtcNow;
			lock (m_RateWindows)
			{
				if (m_RateWindows.Count > 10000)
					m_RateWindows.Clear();

				RateWindow window;
				if (!m_RateWindows.TryGetValue(remote, out window) || now - window.Start >= TimeSpan.FromMinutes(1))
				{
					window = new RateWindow { Start = now, Count = 0 };
					m_RateWindows[remote] = window;
				}

				window.Count++;
				return window.Count <= config.RateLimitPerMinute;
			}
		}

		private static bool HasValidKey(WebApiConfig config, HttpListenerRequest request)
		{
			string supplied = request.Headers["X-Api-Key"];
			if (string.IsNullOrEmpty(supplied))
			{
				string auth = request.Headers["Authorization"];
				if (auth != null && auth.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
					supplied = auth.Substring(7).Trim();
			}

			return supplied != null && FixedTimeEquals(supplied, config.ApiKey);
		}

		private static bool FixedTimeEquals(string a, string b)
		{
			int diff = a.Length ^ b.Length;
			for (int i = 0; i < a.Length && i < b.Length; i++)
				diff |= a[i] ^ b[i];
			return diff == 0;
		}

		private static void WriteJson(HttpListenerResponse response, int status, object body)
		{
			try
			{
				byte[] bytes = Encoding.UTF8.GetBytes(JsonHelper.Serialize(body));
				response.StatusCode = status;
				response.ContentType = "application/json; charset=utf-8";
				response.AddHeader("Cache-Control", "no-store");
				response.ContentLength64 = bytes.Length;
				response.OutputStream.Write(bytes, 0, bytes.Length);
			}
			catch (Exception)
			{
			}
			finally
			{
				try { response.Close(); } catch { }
			}
		}

		private static void WriteEmpty(HttpListenerResponse response, int status)
		{
			try
			{
				response.StatusCode = status;
				response.ContentLength64 = 0;
			}
			finally
			{
				try { response.Close(); } catch { }
			}
		}

		private static string NormalizePath(string path)
		{
			if (string.IsNullOrEmpty(path))
				return "/";

			path = path.TrimEnd('/');
			return path.Length == 0 ? "/" : path.ToLowerInvariant();
		}

		private static string RouteKey(string method, string path)
		{
			return method.ToUpperInvariant() + " " + path;
		}

		private static object IndexHandler(ApiRequest request)
		{
			var endpoints = new List<Dictionary<string, object>>();
			foreach (Route route in m_Routes.Values)
			{
				endpoints.Add(new Dictionary<string, object>
				{
					{ "method", route.Method },
					{ "path", route.Path },
					{ "description", route.Description },
				});
			}

			endpoints.Sort((a, b) => string.CompareOrdinal((string)a["path"], (string)b["path"]));
			return new Dictionary<string, object> { { "endpoints", endpoints } };
		}

		private static object HealthHandler(ApiRequest request)
		{
			DateTime start = Clock.ServerStart;
			return new Dictionary<string, object>
			{
				{ "status", "ok" },
				{ "server_time", DateTime.Now.ToString("o") },
				{ "started_at", start == DateTime.MinValue ? null : start.ToString("o") },
				{ "uptime_seconds", start == DateTime.MinValue ? 0 : (long)(DateTime.Now - start).TotalSeconds },
				{ "configs", ConfigService.StatusSnapshot },
			};
		}
	}
}
