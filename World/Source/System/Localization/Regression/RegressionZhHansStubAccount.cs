using System;
using System.Collections.Generic;
using Server;
using Server.Accounting;

namespace Server.Localization.Regression
{
	/// <summary>Minimal IAccount for <see cref="LocalizationRegressionRunner"/> overhead_chain cases (zh-Hans viewer).</summary>
	internal sealed class RegressionZhHansStubAccount : IAccount
	{
		private readonly Dictionary<string, string> m_Tags;

		public RegressionZhHansStubAccount()
		{
			m_Tags = new Dictionary<string, string>( StringComparer.Ordinal );
			m_Tags[AccountLang.TagName] = "zh-Hans";
		}

		public string Username { get; set; }

		public AccessLevel AccessLevel { get; set; }

		public int Length { get { return 0; } }

		public int Limit { get { return 0; } }

		public int Count { get { return 0; } }

		public Mobile this[int index]
		{
			get { return null; }
			set { }
		}

		public void Delete()
		{
		}

		public void SetPassword( string password )
		{
		}

		public bool CheckPassword( string password )
		{
			return false;
		}

		public string GetTag( string name )
		{
			if ( name == null )
				return null;
			string v;
			return m_Tags.TryGetValue( name, out v ) ? v : null;
		}

		public void SetTag( string name, string value )
		{
			if ( name == null || value == null )
				return;
			m_Tags[name] = value;
		}
	}
}
