using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

namespace BlogServer
{
	public class FileAssociation
	{
		public static void RegisterFileAssociation( string className, string description, string extension )
		{
			// calculate file type name
			string fileTypeName = String.Format( "BlogServer.{0}.1", className);
				
			// write the extension key
			using (RegistryKey extensionKey = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Classes\\" + extension) )
			{
				extensionKey.SetValue(null, fileTypeName); 
				RegistryKey extensionFileTypeKey = extensionKey.CreateSubKey(fileTypeName) ;
				extensionFileTypeKey.Close();
			}

			// write the file type key
			using (RegistryKey fileTypeKey =  Registry.CurrentUser.CreateSubKey("SOFTWARE\\Classes\\" + fileTypeName))
			{
				fileTypeKey.SetValue(null, description); 
					
				using ( RegistryKey curVerKey = fileTypeKey.CreateSubKey("CurVer"))
					curVerKey.SetValue(null, fileTypeName); 
					
				using ( RegistryKey shellOpenCommandKey = fileTypeKey.CreateSubKey( @"shell\open\command"))
					shellOpenCommandKey.SetValue(null, String.Format( "\"{0}\" \"%1\"", Process.GetCurrentProcess().MainModule.FileName) ) ;
			}
		}
	}
}
