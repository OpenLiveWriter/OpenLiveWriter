using System;
using System.Collections;
using System.Text.RegularExpressions;

namespace BlogServer.Config
{
	public class ConfigProperties
	{
		public const string BASE_DIR = "baseDir";
		
		private static ConfigProperties s_instance = new ConfigProperties();
		public static ConfigProperties Instance { get { return s_instance; }}
		
		private static readonly Regex istringExpr = new Regex(@"\$\{(.+?)\}");
		
		private ConfigProperties()
		{}
		
		private readonly Hashtable _values = Hashtable.Synchronized(new Hashtable());
		
		public object this[string name]
		{
			get { return _values[name]; }
			set { _values[name] = value; }
		}
		
		public int Count { get { return _values.Count; }}
		
		public string EvaluateInterpolatedString(string istring)
		{
			return istringExpr.Replace(istring, new MatchEvaluator(Evaluate));
		}

		private string Evaluate(Match match)
		{
			object value = this[match.Groups[1].Value];
			if (value != null)
				return value.ToString();
			else
				return match.Groups[0].ToString();
		}
	}
}
