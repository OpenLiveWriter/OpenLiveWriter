using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace BlogServer.WebServer
{
	public class HttpHeaders
	{
		private static readonly Regex headerFieldRegex = new Regex(@"^([^()<>@,;:\\""/\[\]?={} \t]+?):[ \t]*(.*?)\r\n$");
		private static readonly Regex continuationRegex = new Regex(@"^([ \t].*?)\r\n$");

		private Hashtable fields = new Hashtable();

		public HttpHeaders(SocketReader socketReader)
		{
			int lineNum = 0;

			string line;
			string lastField = null;
			while ("\r\n" != (line = socketReader.NextLine()))
			{
				++lineNum;

				Match match = headerFieldRegex.Match(line);
				if (match.Success)
				{
					string name = match.Groups[1].Value.Trim().ToLower();
					string value = match.Groups[2].Value;
					if (!fields.Contains(name))
						fields[name] = value;
					else
						fields[name] = (string)fields[name] + "," + value;

					// in case continuation lines follow
					lastField = name;
				}
				else
				{
					Match match2 = continuationRegex.Match(line);
					if (match2.Success)
					{
						string value = match2.Groups[1].Value;
						if (lastField != null)
						{
							fields[lastField] = (string)fields[lastField] + value;
						}
						else
						{
							Trace.Fail("Illegal HTTP header: folding whitespace detected at illegal location (line " + lineNum + ")");
						}
					}
					else
					{
						Trace.Fail("Illegal HTTP header: could not parse line " + lineNum);
					}
				}
			}
		}

		public string this[string headerName]
		{
			get
			{
				return fields[headerName.Trim().ToLower()] as string;
			}
		}

		public int ContentLength
		{
			get
			{
				string contentLengthString = this["Content-Length"];
				try
				{
					if (contentLengthString != null && contentLengthString.Length != 0)
						return int.Parse(contentLengthString, CultureInfo.InvariantCulture);
				}
				catch
				{
				}
				return -1;
			}
		}
	}
}
