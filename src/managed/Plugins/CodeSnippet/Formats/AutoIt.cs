using System;

namespace CodeSnippet.Formats
{
	public class AutoIt : VisualBasicFormat
	{
		protected override string CommentRegex
		{
			get
			{
				return "(?:;\\s*).*?(?=\\r|\\n)|(?:(#comments-start|#cs).*?(#comments-end|#ce)\\s).*?(?=\\r|\\n)";
			}
		}

		protected override string Keywords
		{
			get
			{
				return "False True ContinueCase ContinueLoop Default Dim Global Local Const Do Until Enum Exit ExitLoop For To Step Next For In Next Func Return EndFunc Func OnAutoItExit OnAutoItStart EndFunc If Then ElseIf Else EndIf ReDim Select Case EndSelect Switch EndSwitch While WEnd With EndWith";
			}
		}

		protected override string Preprocessors
		{
			get
			{
				return "#\\s*include #\\s*include-once #\\s*NoTrayIcon #\\s*RequireAdmin";
			}
		}

		public AutoIt()
		{
		}
	}
}