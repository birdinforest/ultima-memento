using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Server;

namespace Server.Localization.Regression
{
	// Mirror of Citizens.ResolveCitizenRumorToChineseForBroadcast (Scripts) for core-only regression.
	internal static class CitizenBroadcastZhHans
	{
		public static string Resolve( string englishRumor )
		{
			if ( englishRumor == null || englishRumor.Length == 0 )
				return englishRumor;
			string s = CommonTalkDynamicZh.TryApplyForBroadcast( englishRumor );
			if ( s == null )
				s = QuestCompositeResolver.ResolveCompositeToZhHans( englishRumor );
			return NpcSpeechTokenZh.ApplyNpcVocabularyTokensToZh( s );
		}
	}

	/// <summary>
	/// Loads flat JSON case files from <c>Data/Localization/regression/cases/*.json</c> (all values must be strings;
	/// see <c>README.txt</c> in that folder). Invoked from <c>Core.Main</c> with <c>-localization-regression</c>.
	/// </summary>
	public static class LocalizationRegressionRunner
	{
		private const string CasesSubdir = "Data/Localization/regression/cases";
		private const string ReportRelativePath = "Data/Localization/tools-output/localization-regression-report.json";

		private static Mobile s_Speaker;
		private static Mobile s_Viewer;

		public static int Run()
		{
			QuestCompositeResolver.EnsureInitialized();

			string casesDir = Path.Combine( Core.BaseDirectory, CasesSubdir );

			if ( !Directory.Exists( casesDir ) )
			{
				Console.WriteLine( "LocalizationRegression: missing cases directory: {0}", casesDir );
				return 1;
			}

			EnsureRegressionMobiles();

			var failures = new List<RegressionFailure>();
			int passed = 0;

			string[] files = Directory.GetFiles( casesDir, "*.json" );

			if ( files.Length == 0 )
			{
				Console.WriteLine( "LocalizationRegression: no *.json cases in {0}", casesDir );
				return 1;
			}

			Array.Sort( files, StringComparer.Ordinal );

			for ( int fi = 0; fi < files.Length; ++fi )
			{
				string filePath = files[fi];
				string json;

				try
				{
					json = File.ReadAllText( filePath );
				}
				catch ( Exception ex )
				{
					failures.Add( new RegressionFailure
					{
						File = filePath,
						Id = Path.GetFileName( filePath ),
						Reason = "read error: " + ex.Message
					} );
					continue;
				}

				var dict = new Dictionary<string, string>( StringComparer.Ordinal );
				SimpleJsonObject.ParseStringProperties( json, dict );

				string id = GetDict( dict, "id" );
				if ( id == null || id.Length == 0 )
					id = Path.GetFileName( filePath );

				string pipeline = GetDict( dict, "pipeline" );
				string en = GetDict( dict, "en" );
				string expected = GetDict( dict, "expectedZh" );

				if ( pipeline == null || en == null || expected == null )
				{
					failures.Add( new RegressionFailure
					{
						File = filePath,
						Id = id,
						Reason = "missing required string field (pipeline, en, expectedZh)"
					} );
					continue;
				}

				string actual;

				try
				{
					actual = RunPipeline( pipeline, en );
				}
				catch ( Exception ex )
				{
					failures.Add( new RegressionFailure
					{
						File = filePath,
						Id = id,
						Reason = "pipeline exception: " + ex.Message
					} );
					continue;
				}

				if ( actual == expected )
				{
					++passed;
				}
				else
				{
					failures.Add( new RegressionFailure
					{
						File = filePath,
						Id = id,
						Reason = "mismatch",
						Expected = expected,
						Actual = actual
					} );
				}
			}

			WriteReport( passed, failures.Count, failures );

			Console.WriteLine( "LocalizationRegression: {0} passed, {1} failed (cases dir: {2})", passed, failures.Count, casesDir );

			for ( int i = 0; i < failures.Count; ++i )
			{
				RegressionFailure f = failures[i];
				Console.WriteLine( "  FAIL [{0}] {1}", f.Id, f.Reason );
				if ( f.Expected != null )
				{
					Console.WriteLine( "    expected: {0}", f.Expected );
					Console.WriteLine( "    actual:   {0}", f.Actual );
				}
			}

			return failures.Count > 0 ? 1 : 0;
		}

		private static void EnsureRegressionMobiles()
		{
			if ( s_Speaker != null )
				return;

			s_Speaker = new Mobile();
			s_Viewer = new Mobile();
			s_Viewer.Account = new RegressionZhHansStubAccount();
		}

		private static string GetDict( Dictionary<string, string> dict, string key )
		{
			if ( dict == null || key == null )
				return null;
			string v;
			return dict.TryGetValue( key, out v ) ? v : null;
		}

		/// <summary>Same chain as Scripts citizens broadcast rumor resolver (TryApplyForBroadcast → composite → NPC tokens).</summary>
		private static string RunCitizenBroadcast( string englishRumor )
		{
			return CitizenBroadcastZhHans.Resolve( englishRumor );
		}

		/// <summary>Speaker overhead path after <c>StringCatalog.TryResolve</c> for the viewer (matches production order).</summary>
		private static string RunOverheadChain( string english )
		{
			if ( english == null || english.Length == 0 )
				return english;
			string lang = AccountLang.GetLanguageCode( s_Viewer.Account );
			string work = StringCatalog.TryResolve( lang, english ) ?? english;
			work = s_Speaker.LocalizeDynamicOverheadForViewer( s_Viewer, work );
			return work;
		}

		private static string RunPipeline( string pipeline, string en )
		{
			string p = pipeline.Trim();

			if ( Insensitive.Equals( p, "citizen_broadcast" ) )
				return RunCitizenBroadcast( en );

			if ( Insensitive.Equals( p, "overhead_chain" ) )
				return RunOverheadChain( en );

			if ( Insensitive.Equals( p, "composite_only" ) )
				return QuestCompositeResolver.ResolveCompositeToZhHans( en );

			if ( Insensitive.Equals( p, "string_catalog_only" ) )
				return StringCatalog.TryResolve( "zh-Hans", en ) ?? en;

			throw new InvalidOperationException( "unknown pipeline: " + pipeline );
		}

		private static void WriteReport( int passed, int failed, List<RegressionFailure> failures )
		{
			try
			{
				string reportPath = Path.Combine( Core.BaseDirectory, ReportRelativePath );
				string dir = Path.GetDirectoryName( reportPath );

				if ( dir != null && dir.Length > 0 && !Directory.Exists( dir ) )
					Directory.CreateDirectory( dir );

				var sb = new StringBuilder();
				sb.Append( "{\"passed\":" ).Append( passed ).Append( ",\"failed\":" ).Append( failed ).Append( ",\"failures\":[" );

				for ( int i = 0; i < failures.Count; ++i )
				{
					if ( i > 0 )
						sb.Append( ',' );
					RegressionFailure f = failures[i];
					sb.Append( "{\"id\":\"" ).Append( JsonEscape( f.Id ) ).Append( "\",\"file\":\"" ).Append( JsonEscape( f.File ) ).Append( "\",\"reason\":\"" ).Append( JsonEscape( f.Reason ) ).Append( "\"" );
					if ( f.Expected != null )
						sb.Append( ",\"expected\":\"" ).Append( JsonEscape( f.Expected ) ).Append( "\",\"actual\":\"" ).Append( JsonEscape( f.Actual ) ).Append( "\"" );
					sb.Append( '}' );
				}

				sb.Append( "]}" );
				File.WriteAllText( reportPath, sb.ToString() );
			}
			catch
			{
			}
		}

		private static string JsonEscape( string s )
		{
			if ( s == null )
				return "";
			return s.Replace( "\\", "\\\\" ).Replace( "\"", "\\\"" ).Replace( "\r", "\\r" ).Replace( "\n", "\\n" );
		}

		private sealed class RegressionFailure
		{
			public string File;
			public string Id;
			public string Reason;
			public string Expected;
			public string Actual;
		}
	}
}
