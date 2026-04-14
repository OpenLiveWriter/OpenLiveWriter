// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using OpenLiveWriter.CoreServices.Diagnostics;
using OpenLiveWriter.Interop.Windows;
using Microsoft.Win32;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Helper functions for dealing with the system's Web browser(s).
    /// </summary>
    public partial class BrowserHelper
    {
        private BrowserHelper()
        {
            //static helper, no instances allowed
        }

        /// <summary>
        /// Static helper method to get the currently installed version of Internet
        /// Explorer. This implementation is based on the "Licensing and Distribution"
        /// article at:	http://msdn.microsoft.com/workshop/browser/license/licensing.asp
        /// </summary>
        /// <param name="majorVersion">out parameter for the major version (e.g. 5 or 6)</param>
        /// <param name="minorVersion">our parameter for the minor version (e.g. 0 or 5)</param>
        public static void GetInstalledVersion(out int majorVersion, out int minorVersion)
        {
            // constants used for seeking IE version information
            const string INTERNET_EXPLORER_KEY = @"Software\Microsoft\Internet Explorer";
            const string VERSION_VALUE = "Version";

            // default to no installed version
            majorVersion = 0;
            minorVersion = 0;

            // try to find the installed version
            RegistryKey key = Registry.LocalMachine.OpenSubKey(INTERNET_EXPLORER_KEY);
            if (key != null)
            {
                string version = key.GetValue(VERSION_VALUE) as string;
                if (version != null)
                {
                    string[] versionInfo = version.Split(new char[] { '.' });
                    if (versionInfo.Length >= 2)
                    {
                        majorVersion = Convert.ToInt32(versionInfo[0], CultureInfo.InvariantCulture);
                        minorVersion = Convert.ToInt32(versionInfo[1], CultureInfo.InvariantCulture);
                    }
                }
            }
        }

        public static FileVersionInfo IEVersion
        {
            get
            {
                string systemPath = Environment.GetFolderPath(Environment.SpecialFolder.System);
                string mshtmlDllPath = Path.Combine(systemPath, "mshtml.dll");
                if (File.Exists(mshtmlDllPath))
                    return FileVersionInfo.GetVersionInfo(mshtmlDllPath);
                else
                    return null;
            }
        }

        /// <summary>
        /// Is Mozilla the default browser?
        /// </summary>
        public static bool FirefoxIsDefaultBrowser
        {
            get
            {
                string defaultBrowserPath = DefaultBrowserPath;
                return IsBrowserPathFirefox(defaultBrowserPath);
            }
        }

        /// <summary>
        /// Is Mozilla the default browser?
        /// </summary>
        public static bool IsBrowserPathFirefox(string browserPath)
        {
            if (browserPath != null)
                return browserPath.IndexOf("firefox", StringComparison.OrdinalIgnoreCase) != -1;
            else
                return false;
        }

        public static bool InternetExplorerIsDefaultBrowser
        {
            get
            {
                string defaultBrowserPath = DefaultBrowserPath;
                return IsBrowserPathInternetExplorer(defaultBrowserPath);
            }
        }

        public static bool IsBrowserPathInternetExplorer(string browserPath)
        {
            if (browserPath != null)
                return browserPath.IndexOf("iexplore", StringComparison.OrdinalIgnoreCase) != -1;
            else
                return false;
        }

        /// <summary>
        /// Path to the default Browser executable
        /// </summary>
        public static string DefaultBrowserPath
        {
            get
            {
                string defaultBrowserPath = null;
                // create temporary HTML file for finding default browser
                if (_tmpHtmlFile == null || !File.Exists(_tmpHtmlFile))
                    _tmpHtmlFile = TempFileManager.Instance.CreateTempFile("index.htm");

                // determine the default browser EXE path
                StringBuilder pathBuilder = new StringBuilder(Kernel32.MAX_PATH * 4);
                IntPtr returnCode = Shell32.FindExecutable(_tmpHtmlFile, String.Empty, pathBuilder);
                int rc = returnCode.ToInt32();
                if (rc > 32)
                {
                    defaultBrowserPath = pathBuilder.ToString();
                }
                else // error
                {
                    switch (rc)
                    {
                        case SE_ERR.FNF:
                            Debug.Fail("Unexpected File Not Found error attempting to locate default browser");
                            break;
                        case SE_ERR.NOASSOC:
                            Debug.Fail("Unexpected failure to find .htm file assocation attempting to locate default browser");
                            break;
                        case SE_ERR.OOM:
                            Debug.Fail("Unexpected out of memory error attempting to find default browser");
                            break;
                        default:
                            Debug.Fail("Unexpected error number " + rc.ToString(CultureInfo.InvariantCulture) + " attempting to find default browser");
                            break;
                    }
                }
                return defaultBrowserPath;
            }
        }
        private static string _tmpHtmlFile;

        public static void DisplayUrl(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(String.Format(CultureInfo.InvariantCulture, "Failed to open browser to display url({0}): {1}", url, ex));
                UnexpectedErrorMessage.Show(ex);
            }
        }
    }
}
