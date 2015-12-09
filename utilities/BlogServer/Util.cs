using System;
using System.Collections;
using System.Collections.Specialized;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;

namespace BlogServer
{
	public class Util
	{
		public static IDictionary MakeDictionary(params object[] parameters)
		{
			if ((parameters.Length & 1) != 0)
				throw new ArgumentException("Even number of parameters expected");
			HybridDictionary dict = new HybridDictionary();
			for (int i = 0; i < parameters.Length; i += 2)
				dict[parameters[i]] = parameters[i + 1];
			return dict;
		}
		
		public static string GetContentTypeForExtension(string extension)
		{
			if (extension == null || extension.Length == 0)
				return null;

			using (RegistryKey key = Registry.ClassesRoot.OpenSubKey(extension))
			{
				if (key == null)
					return null;
				return key.GetValue("Content Type", null) as string;
			}
		}
		
		public static string GetExtensionForContentType(string contentType)
		{
			if (contentType == null || contentType.Length == 0)
				return null;
			using (RegistryKey key = Registry.ClassesRoot.OpenSubKey(@"MIME\Database\Content Type\" + contentType))
			{
				return key.GetValue("Extension", null) as string;
			}
		}
		
		public static string PathCanonicalize(string path)
		{
			StringBuilder sb = new StringBuilder(512);
			if (!PathCanonicalize(sb, path))
				throw new ArgumentException("Invalid path: " + path, "path");
			return sb.ToString();
		}
		
		[DllImport("shlwapi.dll", CharSet=CharSet.Unicode)]
		private static extern bool PathCanonicalize(
			[Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder lpszDst, 
			[In, MarshalAs(UnmanagedType.LPWStr)] string lpszSrc);

	}
}
