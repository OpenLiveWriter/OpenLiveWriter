using CodeSnippet.Config;
using CodeSnippet.Forms;
using CodeSnippet.Helpers;
using CodeSnippet.Properties;
using OpenLiveWriter.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using WLWPluginBase.Win32;

namespace CodeSnippet
{
	[CLSCompliant(false)]
	[InsertableContentSource("Code Snippet")]
    [WriterPlugin("7290180F-2379-47c4-9962-3ECC30C9EA69", "Code Snippet", "CodeSnippet.png", PublisherUrl = "http://lvildosola.blogspot.com", Description = "Insert syntax-higlighted snippets of code.")]
    public class CodeSnippetPlugin : ContentSource
	{
		private const RegexOptions RXO_DEFAULTS = RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace;

		private const string RXP_EXTRACT_CODE_SNIPPET = "<(div|pre)[\\s]+.*id=[\"]?(codeSnippetWrapper|codeSnippet)[\"]?.*?(?<codeSnippet>.*?)</(div|pre)>";

		private const string RXP_EXTRACT_LNUM = "<span[\\s]+\\bid=[\"]?lnum[\\d]*[\"]?\\b[^>]*>.*?</span>[\\s]?";

		private const string RXP_EXTRACT_CRLF = "(<BR>|<!--CRLF-->)";

		private const string RXP_EXTRACT_HTML = "<(.|\\n)*?>";

		private const string RXP_REPLACE_LNUM = "^(?:\\r\\n|\\r|\\n)*(?<lineNumber>\\s*\\d+:\\s{1})(?<line>.*)(?!\\r\\n|\\r|\\n)*$";

		private const string RXG_CODE_SNIPPET = "codeSnippet";

		public CodeSnippetPlugin()
		{
		}

		public override DialogResult CreateContent(IWin32Window dialogOwner, ref string content)
		{
			ICodeSnippetForm codeSnippetForm;
			Win32EnumWindowsItem win32EnumWindowsItem;
			MessageBoxOptions messageBoxOption;
			DialogResult dialogResult = DialogResult.Cancel;
			CodeSnippetConfig config = CodeSnippetConfig.Load();
			if (config.General.RunSilent)
			{
				if (Control.ModifierKeys != Keys.Control)
				{
					content = FormatHelper.Format(config.General.ActiveLanguage, config.Editor, config.Style, Clipboard.GetText(), config.Editor.TrimIndentOnPaste);
					dialogResult = DialogResult.OK;
				}
				else
				{
					config.General.RunSilent = false;
				}
			}
			if (!config.General.RunSilent)
			{
				bool flag = true;
				bool flag1 = false;
				do
				{
					if (config.General.ActiveMode != CodeSnippetViewMode.Compact)
					{
						codeSnippetForm = new CodeSnippetForm(config);
						if (dialogOwner == null)
						{
							win32EnumWindowsItem = null;
						}
						else
						{
							win32EnumWindowsItem = Win32EnumWindows.FindByClassName(dialogOwner.Handle, "Internet Explorer_Server");
						}
						Win32EnumWindowsItem win32EnumWindowsItem1 = win32EnumWindowsItem;
						if (win32EnumWindowsItem1 != null && !string.IsNullOrEmpty(Win32IEHelper.GetSelectedText(win32EnumWindowsItem1.Handle)))
						{
							string selectedHtml = Win32IEHelper.GetSelectedHtml(win32EnumWindowsItem1.Handle);
							if (!string.IsNullOrEmpty((new Regex("<(div|pre)[\\s]+.*id=[\"]?(codeSnippetWrapper|codeSnippet)[\"]?.*?(?<codeSnippet>.*?)</(div|pre)>", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace)).Match(selectedHtml).Groups["codeSnippet"].Value))
							{
								flag1 = true;
							}
							else
							{
								if (flag)
								{
									IWin32Window win32Window = dialogOwner;
									string processSelectedTextAsCodeSnippet = Resources.ProcessSelectedTextAsCodeSnippet;
									string text = codeSnippetForm.Text;
									if (CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft)
									{
										messageBoxOption = MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading;
									}
									else
									{
										messageBoxOption = (MessageBoxOptions)0;
									}
									if (DialogResult.Yes == MessageBox.Show(win32Window, processSelectedTextAsCodeSnippet, text, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2, messageBoxOption))
									{
										flag1 = true;
									}
								}
								flag = false;
							}
							if (flag1)
							{
								string str = Regex.Replace(Regex.Replace(Regex.Replace(selectedHtml, "<span[\\s]+\\bid=[\"]?lnum[\\d]*[\"]?\\b[^>]*>.*?</span>[\\s]?", string.Empty, RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace), "(<BR>|<!--CRLF-->)", Environment.NewLine, RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace), "<(.|\\n)*?>", string.Empty, RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);
								str = str.Replace("&lt;", "<");
								str = str.Replace("&gt;", ">");
								str = str.Replace("&amp;", "&");
								str = str.Replace("&nbsp;", string.Empty);
								List<string> strs = new List<string>(0);
								MatchCollection matchCollections = (new Regex("^(?:\\r\\n|\\r|\\n)*(?<lineNumber>\\s*\\d+:\\s{1})(?<line>.*)(?!\\r\\n|\\r|\\n)*$", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace)).Matches(str);
								foreach (Match match in matchCollections)
								{
									strs.Add(match.Groups["line"].Value);
								}
								if (strs.Count > 0)
								{
									str = string.Join(string.Empty, strs.ToArray());
								}
								codeSnippetForm.CodeSnippetToEdit = str;
							}
						}
					}
					else
					{
						codeSnippetForm = new CodeSnippetCompactForm(config);
					}
					dialogResult = codeSnippetForm.ShowDialog(dialogOwner);
					if (dialogResult == DialogResult.OK)
					{
						content = codeSnippetForm.CodeSnippet;
					}
					config = codeSnippetForm.Config;
					config.Store();
				}
				while (dialogResult == DialogResult.Retry);
			}
			return dialogResult;
		}
	}
}