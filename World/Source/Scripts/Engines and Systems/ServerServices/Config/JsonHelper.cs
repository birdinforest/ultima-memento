using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Web.Script.Serialization;

namespace Server.Engines.ServerServices
{
	/// <summary>
	/// Thin wrapper over JavaScriptSerializer with typed, default-tolerant accessors.
	/// </summary>
	public static class JsonHelper
	{
		private static JavaScriptSerializer CreateSerializer()
		{
			return new JavaScriptSerializer { MaxJsonLength = 16 * 1024 * 1024 };
		}

		public static Dictionary<string, object> ParseObject(string json)
		{
			object root = CreateSerializer().DeserializeObject(json);
			var dict = root as Dictionary<string, object>;

			if (dict == null)
				throw new FormatException("Root JSON value must be an object.");

			return new Dictionary<string, object>(dict, StringComparer.OrdinalIgnoreCase);
		}

		public static string Serialize(object value)
		{
			return CreateSerializer().Serialize(value);
		}

		public static bool TryGet(Dictionary<string, object> d, string key, out object value)
		{
			value = null;
			return d != null && d.TryGetValue(key, out value) && value != null;
		}

		public static bool GetBool(Dictionary<string, object> d, string key, bool fallback)
		{
			object v;
			if (!TryGet(d, key, out v))
				return fallback;

			if (v is bool)
				return (bool)v;

			string s = Convert.ToString(v, CultureInfo.InvariantCulture).Trim();
			if (string.Equals(s, "true", StringComparison.OrdinalIgnoreCase) || s == "1" || string.Equals(s, "yes", StringComparison.OrdinalIgnoreCase))
				return true;
			if (string.Equals(s, "false", StringComparison.OrdinalIgnoreCase) || s == "0" || string.Equals(s, "no", StringComparison.OrdinalIgnoreCase))
				return false;

			throw new FormatException(string.Format("'{0}' must be a boolean.", key));
		}

		public static double GetDouble(Dictionary<string, object> d, string key, double fallback)
		{
			object v;
			if (!TryGet(d, key, out v))
				return fallback;

			try
			{
				return Convert.ToDouble(v, CultureInfo.InvariantCulture);
			}
			catch (Exception)
			{
				throw new FormatException(string.Format("'{0}' must be a number.", key));
			}
		}

		public static int GetInt(Dictionary<string, object> d, string key, int fallback)
		{
			return (int)Math.Round(GetDouble(d, key, fallback));
		}

		public static string GetString(Dictionary<string, object> d, string key, string fallback)
		{
			object v;
			return TryGet(d, key, out v) ? Convert.ToString(v, CultureInfo.InvariantCulture) : fallback;
		}

		public static Dictionary<string, object> GetObject(Dictionary<string, object> d, string key)
		{
			object v;
			if (!TryGet(d, key, out v))
				return null;

			var obj = v as Dictionary<string, object>;
			if (obj == null)
				throw new FormatException(string.Format("'{0}' must be an object.", key));

			return new Dictionary<string, object>(obj, StringComparer.OrdinalIgnoreCase);
		}

		public static List<object> GetArray(Dictionary<string, object> d, string key)
		{
			object v;
			if (!TryGet(d, key, out v))
				return null;

			var list = v as IEnumerable;
			if (list == null || v is string)
				throw new FormatException(string.Format("'{0}' must be an array.", key));

			var result = new List<object>();
			foreach (object o in list)
				result.Add(o);

			return result;
		}

		public static int[] GetIntArray(Dictionary<string, object> d, string key, int[] fallback)
		{
			List<object> list = GetArray(d, key);
			if (list == null)
				return fallback;

			var result = new int[list.Count];
			for (int i = 0; i < list.Count; i++)
				result[i] = (int)Math.Round(Convert.ToDouble(list[i], CultureInfo.InvariantCulture));

			return result;
		}

		public static string[] GetStringArray(Dictionary<string, object> d, string key, string[] fallback)
		{
			List<object> list = GetArray(d, key);
			if (list == null)
				return fallback;

			var result = new string[list.Count];
			for (int i = 0; i < list.Count; i++)
				result[i] = Convert.ToString(list[i], CultureInfo.InvariantCulture);

			return result;
		}
	}
}
