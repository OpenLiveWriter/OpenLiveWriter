// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#define PORTABLE
using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.Win32;
using OpenLiveWriter.CoreServices.Diagnostics;
using OpenLiveWriter.CoreServices.Settings;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.CoreServices
{

    public class ApplicationEnvironment
    {
        // Changing the taskbar application id in upgrade scenarios can break the jumplist
        // in the sense that it will be empty (no drafts/posts) until the post list cache is
        // refreshed, which happens on initial configuration, post-publishing, and draft-saving.
        // We use a unique, culture-invariant, hard-coded string here to avoid any inadvertent breaking changes.
        public static string TaskbarApplicationId = "Open Live Writer - {3DDDAFC5-5C01-4BCF-B81A-A4976A0999E9}";

        private const string DefaultProductName = "Open Live Writer";
        private const string AppDataFolderName = "OpenLiveWriter";              // Squirrel installs the app to the folder that matches nuspec's ID.
        private const string DefaultSettingsRootKeyName = @"Software\\OpenLiveWriter";

        public static void Initialize()
        {
            Initialize(Assembly.GetCallingAssembly());
        }

        public static void Initialize(Assembly rootAssembly)
        {
            Initialize(rootAssembly, Path.GetDirectoryName(rootAssembly.Location));
        }

        public static void Initialize(Assembly rootAssembly, string installationDirectory)
        {
            Initialize(rootAssembly, installationDirectory, DefaultSettingsRootKeyName, DefaultProductName);
        }

        public static void Initialize(Assembly rootAssembly, string installationDirectory, string settingsRootKeyName, string productName)
        {
            // initialize name and version based on assembly metadata
            string rootAssemblyPath = rootAssembly.Location;
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(rootAssemblyPath);
            _companyName = fileVersionInfo.CompanyName;
            _productName = productName;
            _productVersion = String.Format(CultureInfo.InvariantCulture, "{0}.{1}.{2}.{3}", fileVersionInfo.ProductMajorPart, fileVersionInfo.ProductMinorPart, fileVersionInfo.ProductBuildPart, fileVersionInfo.ProductPrivatePart);
            _appVersion = new Version(_productVersion);

            Debug.Assert(_appVersion.Build < UInt16.MaxValue &&
                         _appVersion.Revision < UInt16.MaxValue &&
                         _appVersion.Major < UInt16.MaxValue &&
                         _appVersion.Minor < UInt16.MaxValue, "Invalid ApplicationVersion: " + _appVersion);

            // set installation directory and executable name
            _installationDirectory = installationDirectory;
            _mainExecutableName = Path.GetFileName(rootAssemblyPath);

            // initialize icon/user-agent, etc.
            _userAgent = FormatUserAgentString(ProductName, true);
            _productIcon = ResourceHelper.LoadAssemblyResourceIcon("Images.ApplicationIcon.ico");
            _productIconSmall = ResourceHelper.LoadAssemblyResourceIcon("Images.ApplicationIcon.ico", 16, 16);

            // initialize IsHighContrastWhite and IsHighContrastBlack
            InitializeIsHighContrastBlackWhite();

            _settingsRootKeyName = settingsRootKeyName;
            string dataPath;

            // see if we're running in portable mode
#if PORTABLE
            dataPath = Path.Combine(_installationDirectory, "UserData");
            if (Directory.Exists(dataPath))
            {
                _portable = true;
                // initialize application data directories
                _applicationDataDirectory = Path.Combine(dataPath, "AppData\\Roaming");
                _localApplicationDataDirectory = Path.Combine(dataPath, "AppData\\Local");

                // initialize settings
                _userSettingsRoot = new SettingsPersisterHelper(XmlFileSettingsPersister.Open(Path.Combine(dataPath, "UserSettings.xml")));
                _machineSettingsRoot = new SettingsPersisterHelper(XmlFileSettingsPersister.Open(Path.Combine(dataPath, "MachineSettings.xml")));
                _preferencesSettingsRoot = _userSettingsRoot.GetSubSettings(ApplicationConstants.PREFERENCES_SUB_KEY);
            }
            else
#endif
            {
                _portable = false;
                // initialize application data directories.
                _applicationDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppDataFolderName);
                _localApplicationDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppDataFolderName);

                // initialize settings
                _userSettingsRoot = new SettingsPersisterHelper(new RegistrySettingsPersister(Registry.CurrentUser, SettingsRootKeyName));
                _machineSettingsRoot = new SettingsPersisterHelper(new RegistrySettingsPersister(Registry.LocalMachine, SettingsRootKeyName));
                _preferencesSettingsRoot = _userSettingsRoot.GetSubSettings(ApplicationConstants.PREFERENCES_SUB_KEY);

                dataPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            }

            string postsDirectoryPostEditor = PreferencesSettingsRoot.GetSubSettings("PostEditor").GetString("PostsDirectory", null);

            if (string.IsNullOrEmpty(postsDirectoryPostEditor))
            {
                _myWeblogPostsFolder = _userSettingsRoot.GetString("PostsDirectory", null);
                if (string.IsNullOrEmpty(_myWeblogPostsFolder))
                {
                    if ((_productName == DefaultProductName) && (string.IsNullOrEmpty(dataPath)))
                    {
                        throw new DirectoryException(MessageId.PersonalDirectoryFail);
                    }
                    else
                    {
                        _myWeblogPostsFolder = Path.Combine(dataPath, "My Weblog Posts");
                    }
                }

                PreferencesSettingsRoot.GetSubSettings("PostEditor").SetString("PostsDirectory", _myWeblogPostsFolder);
            }
            else
            {
                _myWeblogPostsFolder = postsDirectoryPostEditor;
            }

            // initialize diagnostics
            InitializeLogFilePath();
            _applicationDiagnostics = new ApplicationDiagnostics(LogFilePath, rootAssembly.GetName().Name);

            if (!Directory.Exists(_applicationDataDirectory))
                Directory.CreateDirectory(_applicationDataDirectory);
            if (!Directory.Exists(_localApplicationDataDirectory))
                Directory.CreateDirectory(_localApplicationDataDirectory);
        }

        // allow override of product-name for user-agent (useful to cloak product's
        // real identify during private beta testing)
        public static void OverrideUserAgent(string productName, bool browserBased)
        {
            _userAgent = FormatUserAgentString(productName, browserBased);

        }

        public static string CompanyName
        {
            get
            {
                return _companyName;
            }
        }
        private static string _companyName = string.Empty;

        public static string ProductName_Short
        {
            get
            {
                return _productName_short;
            }
            set
            {
                _productName_short = value;
            }
        }
        private static string _productName_short = string.Empty;

        public static string ProductName
        {
            get
            {
                return _productName;
            }
        }
        private static string _productName;

        public static string ProductNameQualified
        {
            get
            {
#if BETA_BUILD
                return ProductName + " " + Res.Get(StringId.Beta);
#else
                return ProductName;
#endif
            }
        }

        public static string ProductNameVersioned
        {
            get { return Res.Get(StringId.ProductNameVersioned); }
        }

        public static string ProductVersion
        {
            get
            {
                return _productVersion;
            }
        }
        private static string _productVersion;

        public static string ProductVersionMajor
        {
            get
            {
                return String.Format(CultureInfo.InvariantCulture, "{0}.{1}", _appVersion.Major, _appVersion.Build);
            }
        }

        public static string ProductVersionMinor
        {
            get
            {
                return String.Format(CultureInfo.InvariantCulture, "{0}.{1}", _appVersion.Minor, _appVersion.Revision);
            }
        }
        private static Version _appVersion;

        public static string ProductDisplayVersion
        {
            get
            {
                return _productDisplayVersion;
            }
            set
            {
                _productDisplayVersion = value;
            }
        }
        private static string _productDisplayVersion;

        public static string InstallationDirectory
        {
            get
            {
                return _installationDirectory;
            }
        }
        private static string _installationDirectory;

        public static string MainExecutableName
        {
            get
            {
                return _mainExecutableName;
            }
        }
        private static string _mainExecutableName;

        public static string ApplicationDataDirectory
        {
            get
            {
                return _applicationDataDirectory;
            }
        }
        private static string _applicationDataDirectory;

        public static string LocalApplicationDataDirectory
        {
            get
            {
                return _localApplicationDataDirectory;
            }
        }
        private static string _localApplicationDataDirectory;

        public static string UserAgent
        {
            get
            {
                return _userAgent;
            }
        }
        private static string _userAgent;

        public static Icon ProductIcon
        {
            get
            {
                return _productIcon;
            }
        }
        private static Icon _productIcon;

        public static Icon ProductIconSmall
        {
            get
            {
                return _productIconSmall;
            }
        }
        private static Icon _productIconSmall;

        private static void InitializeIsHighContrastBlackWhite()
        {
            if (System.Windows.Forms.SystemInformation.HighContrast)
            {
                if (SystemColors.Window.R.Equals(255) &&
                    SystemColors.Window.G.Equals(255) &&
                    SystemColors.Window.B.Equals(255))
                {
                    _IsHighContrastWhite = true;
                }
                else
                {
                    _IsHighContrastBlack = true;
                }
            }
        }

        public static bool IsHighContrastWhite
        {
            get
            {
                return _IsHighContrastWhite;
            }
        }
        private static bool _IsHighContrastWhite;

        public static bool IsHighContrastBlack
        {
            get
            {
                return _IsHighContrastBlack;
            }
        }
        private static bool _IsHighContrastBlack;

        public static string SettingsRootKeyName
        {
            get
            {
                return _settingsRootKeyName;
            }
        }
        private static string _settingsRootKeyName;

        public static SettingsPersisterHelper UserSettingsRoot
        {
            get
            {
                return _userSettingsRoot;
            }
        }
        private static SettingsPersisterHelper _userSettingsRoot;

        public static SettingsPersisterHelper MachineSettingsRoot
        {
            get
            {
                return _machineSettingsRoot;
            }
        }
        private static SettingsPersisterHelper _machineSettingsRoot;

        public static SettingsPersisterHelper PreferencesSettingsRoot
        {
            get
            {
                return _preferencesSettingsRoot;
            }
        }
        private static SettingsPersisterHelper _preferencesSettingsRoot;

        private const string CUSTOMCOLORS_NAME = "CustomColors";
        public static int[] CustomColors
        {
            get
            {
                try
                {
                    string strVal = PreferencesSettingsRoot.GetString(CUSTOMCOLORS_NAME, null);
                    if (strVal != null)
                    {
                        string[] parts = StringHelper.Split(strVal, ",");
                        int[] retVal = new int[parts.Length];
                        for (int i = 0; i < retVal.Length; i++)
                            retVal[i] = int.Parse(parts[i], CultureInfo.InvariantCulture);
                        return retVal;
                    }
                }
                catch (Exception e)
                {
                    Trace.Fail(e.ToString());
                }

                return new int[]
                        {
                            (0 | 0 << 8 | 0 << 16),
                            (64 | 64 << 8 | 64 << 16),
                            (128 | 128 << 8 | 128 << 16),
                            (255 | 255 << 8 | 255 << 16),
                            (0 | 0 << 8 | 128 << 16),
                            (0 | 128 << 8 | 0 << 16),
                            (0 | 128 << 8 | 128 << 16),
                            (128 | 0 << 8 | 0 << 16),
                            (128 | 0 << 8 | 128 << 16),
                            (128 | 128 << 8 | 0 << 16),
                            (0 | 0 << 8 | 255 << 16),
                            (0 | 255 << 8 | 0 << 16),
                            (0 | 255 << 8 | 255 << 16),
                            (255 | 0 << 8 | 0 << 16),
                            (255 | 0 << 8 | 255 << 16),
                            (255 | 255 << 8 | 0 << 16)
                        };
            }
            set
            {
                if (value == null)
                {
                    PreferencesSettingsRoot.Unset(CUSTOMCOLORS_NAME);
                }
                else
                {
                    StringBuilder sb = new StringBuilder();
                    string delim = "";
                    foreach (int i in value)
                    {
                        sb.Append(delim);
                        sb.Append(i.ToString(CultureInfo.InvariantCulture));
                        delim = ",";
                    }
                    PreferencesSettingsRoot.SetString(CUSTOMCOLORS_NAME, sb.ToString());
                }
            }
        }

        public static string LogFilePath
        {
            get
            {
                return _logFilePath;
            }
        }
        private static string _logFilePath;

        private static void InitializeLogFilePath()
        {
#if DEBUG
            _logFilePath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
#else
            _logFilePath = LocalApplicationDataDirectory ;
#endif
            _logFilePath = Path.Combine(_logFilePath, String.Format(CultureInfo.InvariantCulture, "{0}.log", ProductName));

        }

        public static ApplicationDiagnostics ApplicationDiagnostics
        {
            get
            {
                // WinLive 218929 : If we are null, most likely something went wrong before we are fully
                // initialized and we are trying to watson. Just create a new instance here
                // using temp paths.
                if (_applicationDiagnostics == null)
                {
                    string templogPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        String.Format(CultureInfo.InvariantCulture, "{0}.log", DefaultProductName));
                    _applicationDiagnostics = new ApplicationDiagnostics(templogPath, Assembly.GetCallingAssembly().GetName().Name);
                }
                return _applicationDiagnostics;
            }
        }
        private static ApplicationDiagnostics _applicationDiagnostics;

        public static bool IsPortableMode
        {
            get
            {
                if (_portable == null)
                    throw new InvalidOperationException("ApplicationEnvironment has not been initialized");
                return _portable.Value;
            }
        }
        private static bool? _portable;

        public static string FormatUserAgentString(string productName, bool browserBased)
        {
            // get browser version
            int majorBrowserVersion, minorBrowserVersion;
            SafeGetBrowserVersion(out majorBrowserVersion, out minorBrowserVersion);

            // get os version
            Version osVersion = Environment.OSVersion.Version;

            // format user-agent string
            string userAgent;
            if (browserBased)
            {
                // e.g. "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 7.0; Open Live Writer 1.0)"
                userAgent = String.Format(CultureInfo.InvariantCulture,
                                          "Mozilla/4.0 (compatible; MSIE {0}.{1}; Windows NT {2}.{3}; {4} 1.0)",
                                          majorBrowserVersion,
                                          minorBrowserVersion,
                                          osVersion.Major,
                                          osVersion.Minor,
                                          productName);
            }
            else
            {
                // e.g. "Open Live Writer 1.0 (Windows NT 7.0)"
                userAgent = String.Format(CultureInfo.InvariantCulture,
                                          "{0} 1.0 (Windows NT {1}.{2})",
                                          productName,
                                          osVersion.Major,
                                          osVersion.Minor);
            }

            return userAgent;
        }

        public static Version BrowserVersion
        {
            get
            {
                int majorBrowserVersion, minorBrowserVersion;
                SafeGetBrowserVersion(out majorBrowserVersion, out minorBrowserVersion);
                return new Version(majorBrowserVersion, minorBrowserVersion);
            }
        }

        private static void SafeGetBrowserVersion(out int majorBrowserVersion, out int minorBrowserVersion)
        {
            majorBrowserVersion = 6;
            minorBrowserVersion = 0;
            try
            {
                BrowserHelper.GetInstalledVersion(out majorBrowserVersion, out minorBrowserVersion);
            }
            catch (Exception ex)
            {
                Debug.Fail("Unexpected exception getting browser version: " + ex.ToString());
                majorBrowserVersion = 6;
                minorBrowserVersion = 0;
            }
        }

        // default initialization for designer dependencies (only do this
        // when running in the IDE)
#if DEBUG
        static ApplicationEnvironment()
        {
            if (ProcessHelper.GetCurrentProcessName() == "devenv.exe")
            {
                Initialize(Assembly.GetExecutingAssembly());
            }
        }
#endif
        private static string _myWeblogPostsFolder;

        public static string MyWeblogPostsFolder
        {
            get
            {
                return PreferencesSettingsRoot.GetSubSettings("PostEditor").GetString("PostsDirectory", null); 
            }
        }

        public static string InsertImageDirectory
        {
            get
            {
                using (SettingsPersisterHelper settings = UserSettingsRoot.GetSubSettings("Preferences\\PostEditor"))
                {
                    string insertImageDirectory = settings.GetString("ImageInsertDir", null);
                    if (string.IsNullOrEmpty(insertImageDirectory) || !Directory.Exists(insertImageDirectory))
                        insertImageDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                    return insertImageDirectory;
                }
            }
            set
            {
                using (SettingsPersisterHelper settings = UserSettingsRoot.GetSubSettings("Preferences\\PostEditor"))
                {
                    settings.SetString("ImageInsertDir", value);
                }
            }
        }
    }
}
