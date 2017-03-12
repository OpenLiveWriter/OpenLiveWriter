// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Globalization;
using System.IO;
using System.Collections;
using System.Diagnostics;
using OpenLiveWriter.Api;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Settings;
using OpenLiveWriter.Interop.Windows;
using OpenLiveWriter.Extensibility.ImageEditing;
using Microsoft.Win32;

namespace OpenLiveWriter.PostEditor
{
    /// <summary>
    /// Manages the loading of external plugins for the post editor.
    /// </summary>
    public class PostEditorPluginManager
    {
        /// <summary>
        /// The list of supported plugin types that should be collected from plugin assemblies.
        /// </summary>
        private Type[] SupportedPluginTypes = new Type[]
        {
            //  Note to developers: add a type here for each plugin interface type supported.
            typeof(WriterPlugin)
        };

        #region Public Methods
        /// <summary>
        /// Returns the singleton plugin manager instance.
        /// </summary>
        public static PostEditorPluginManager Instance
        {
            get
            {
                Trace.Assert(_instance != null, "There is no instance of PostEditorPluginManager, are you sure that plugins are enabled?");
                return _instance;
            }
        }
        private static PostEditorPluginManager _instance;

        internal static void Init()
        {
            _instance = new PostEditorPluginManager();
        }

        /// <summary>
        /// Return the plugin implementation types that are compatible with the specified plugin type.
        /// </summary>
        /// <param name="pluginType">The plugin interface or baseclass that the plugins should implement</param>
        /// <returns></returns>
        public Type[] GetPlugins(Type pluginType)
        {
            lock (_pluginTypesTable)
            {
                ArrayList typeList = (ArrayList)_pluginTypesTable[pluginType];
                if (typeList == null)
                {
                    Debug.Fail("Unsupported plugin type (did you update the SupportedPluginTypes list?): " + pluginType.FullName);
                }
                return (Type[])typeList.ToArray(typeof(Type));
            }
        }

        public event EventHandler PluginListChanged;

        private void OnPluginListChanged()
        {
            if (PluginListChanged != null)
                PluginListChanged(this, EventArgs.Empty);
        }

        #endregion

        #region PluginManager internal implementation
        private PostEditorPluginManager()
        {
            LoadPluginTypes(true);

            // watch file system for new plugins
            if (PluginDirectory != null && Directory.Exists(PluginDirectory))
            {
                try
                {
                    FileSystemWatcher fileWatcher = new FileSystemWatcher(PluginDirectory);
                    fileWatcher.Created += new FileSystemEventHandler(OnPluginDirectoryChanged);
                    fileWatcher.Deleted += new FileSystemEventHandler(OnPluginDirectoryChanged);
                    fileWatcher.EnableRaisingEvents = true;

                    // We used to hard code our install path to program files/Open Live Writer
                    // (and we documented that in our SDK in 1.0 Beta 1)
                    // so continue to scan there even if the plugin directory is somewhere different
                    if (PluginDirectory != PluginDirectoryLegacy)
                    {
                        try
                        {
                            if (Directory.Exists(PluginDirectoryLegacy))
                            {
                                FileSystemWatcher legacyWatcher = new FileSystemWatcher(PluginDirectoryLegacy);
                                legacyWatcher.Created += new FileSystemEventHandler(OnPluginDirectoryChanged);
                                legacyWatcher.Deleted += new FileSystemEventHandler(OnPluginDirectoryChanged);
                                legacyWatcher.EnableRaisingEvents = true;
                            }
                        }
                        catch (ArgumentException)
                        {
                            Trace.WriteLine("No legacy directory to monitor for plugins. Ignoring legacy directory.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("Unexpected exception attempting to configure file monitoring for plugin directory: " + ex.ToString());
                }
            }

            // watch registry for new plugins

            // HKCU
            RegistryMonitor.Instance.AddRegistryChangeListener(HKEY.CURRENT_USER, _pluginsKey, new RegistryKeyEventHandler(OnPluginRegistryKeyChanged), true);
            if (_pluginsKey != PLUGIN_LEGACY_KEY)
            {
                try
                {
                    RegistryMonitor.Instance.AddRegistryChangeListener(HKEY.CURRENT_USER, PLUGIN_LEGACY_KEY, new RegistryKeyEventHandler(OnPluginRegistryKeyChanged), true);
                }
                catch
                {
                    Trace.WriteLine("Not monitoring legacy registry key in HKCU.");
                }
            }

            // HKLM (this may not work for limited users and/or on Vista)
            try
            {
                RegistryMonitor.Instance.AddRegistryChangeListener(HKEY.LOCAL_MACHINE, _pluginsKey, new RegistryKeyEventHandler(OnPluginRegistryKeyChanged), false);
                if (_pluginsKey != PLUGIN_LEGACY_KEY)
                {
                    try
                    {
                        RegistryMonitor.Instance.AddRegistryChangeListener(HKEY.LOCAL_MACHINE, PLUGIN_LEGACY_KEY, new RegistryKeyEventHandler(OnPluginRegistryKeyChanged), false);
                    }
                    catch
                    {
                        Trace.WriteLine("Not monitoring legacy registry key in HKLM.");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error occurred attempting to register for HKLM PluginAssembly change notifications: " + ex.ToString());
            }

        }

        private void OnPluginRegistryKeyChanged(object sender, RegistryKeyEventArgs e)
        {
            LoadPluginTypes(false);
        }

        private void OnPluginDirectoryChanged(object source, FileSystemEventArgs e)
        {
            LoadPluginTypes(false);
        }

        private string PluginDirectory
        {
            get
            {
                string pluginDirectory = Path.Combine(ApplicationEnvironment.InstallationDirectory, "Plugins");
#if DEBUG
                if (!Directory.Exists(pluginDirectory))
                    Directory.CreateDirectory(pluginDirectory);
#endif
                return pluginDirectory;
            }
        }

        private string PluginDirectoryLegacy
        {
            get
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"Open Live Writer\Plugins");
            }
        }

        private Hashtable _pluginTypesTable = new Hashtable();
        private void LoadPluginTypes(bool showErrors)
        {
            lock (_pluginTypesTable)
            {
                // reset existing contents
                _pluginTypesTable.Clear();

                PluginLoader pluginLoader = new PluginLoader(typeof(WriterPlugin).Assembly);

                //register the callbacks for each supported plugin type.
                foreach (Type pluginType in SupportedPluginTypes)
                {
                    //subscribe for a callback when a matching implementation type is found.
                    pluginLoader.AddPluginTypeCallback(pluginType, new PluginLoader.PluginLoadedCallback(RegisterPluginImpl));

                    //create a list to hold discovered plugin implementations.
                    _pluginTypesTable[pluginType] = new ArrayList();
                }

                // load from plugin directory
                if (PluginDirectory != null && Directory.Exists(PluginDirectory))
                    pluginLoader.LoadPluginsFromDirectory(PluginDirectory, showErrors);

                // load from plugins specified in the registry
                LoadPluginsFromRegistryPaths(pluginLoader, showErrors);

                // load from legacy plugin directory
                if (PluginDirectoryLegacy != null && Directory.Exists(PluginDirectoryLegacy))
                    pluginLoader.LoadPluginsFromDirectory(PluginDirectoryLegacy, showErrors);
            }

            // fire event
            if (PluginListChanged != null)
                PluginListChanged(this, EventArgs.Empty);
        }

        private void LoadPluginsFromRegistryPaths(PluginLoader pluginLoader, bool showErrors)
        {
            try
            {
                // build list of assemblies
                ArrayList assemblyPaths = new ArrayList();

                // HKCU
                using (SettingsPersisterHelper userSettingsKey = ApplicationEnvironment.UserSettingsRoot.GetSubSettings(PLUGIN_ASSEMBLIES))
                    AddPluginsFromKey(assemblyPaths, userSettingsKey);

                if (_pluginsKey != PLUGIN_LEGACY_KEY)
                    using (RegistryKey legacyKey = Registry.CurrentUser.OpenSubKey(PLUGIN_LEGACY_KEY))
                        if (legacyKey != null) // Don't create the key if it doesn't already exist
                            using (SettingsPersisterHelper legacyUserSettingsKey = new SettingsPersisterHelper(new RegistrySettingsPersister(Registry.CurrentUser, PLUGIN_LEGACY_KEY)))
                                AddPluginsFromKey(assemblyPaths, legacyUserSettingsKey);

                // HKLM (this may not work for limited users and/or on vista)

                try
                {
                    if (ApplicationEnvironment.MachineSettingsRoot.HasSubSettings(PLUGIN_ASSEMBLIES))
                        using (SettingsPersisterHelper machineSettingsKey = ApplicationEnvironment.MachineSettingsRoot.GetSubSettings(PLUGIN_ASSEMBLIES))
                            AddPluginsFromKey(assemblyPaths, machineSettingsKey);

                    if (_pluginsKey != PLUGIN_LEGACY_KEY)
                        using (RegistryKey legacyKey = Registry.LocalMachine.OpenSubKey(PLUGIN_LEGACY_KEY))
                            if (legacyKey != null) // Don't create the key if it doesn't already exist
                                using (SettingsPersisterHelper legacyUserSettingsKey = new SettingsPersisterHelper(new RegistrySettingsPersister(Registry.LocalMachine, PLUGIN_LEGACY_KEY)))
                                    AddPluginsFromKey(assemblyPaths, legacyUserSettingsKey);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error occurred while attempting to read HKLM for PluginAssemblies: " + ex.ToString());
                }

                // load them
                foreach (string assemblyPath in assemblyPaths)
                    pluginLoader.LoadPluginsFromAssemblyPath(assemblyPath, showErrors);
            }
            catch (Exception ex)
            {
                Trace.Fail("Unexpected exception loading plugins from registry paths: " + ex.ToString());
            }
        }

        private string _pluginsKey = String.Format(CultureInfo.InvariantCulture, "{0}\\{1}", ApplicationEnvironment.SettingsRootKeyName, PLUGIN_ASSEMBLIES);
        private const string PLUGIN_ASSEMBLIES = "PluginAssemblies";
        private readonly string PLUGIN_LEGACY_KEY = ApplicationEnvironment.SettingsRootKeyName + @"\PluginAssemblies";

        private void AddPluginsFromKey(ArrayList pluginPaths, SettingsPersisterHelper settingsKey)
        {
            foreach (string name in settingsKey.GetNames())
            {
                string assemblyPath = settingsKey.GetString(name, String.Empty);
                if (!pluginPaths.Contains(assemblyPath))
                    pluginPaths.Add(assemblyPath);
            }
        }

        /// <summary>
        /// Handles registration plugin implementation when it is found in the plugins directory.
        /// </summary>
        /// <param name="pluginImplType"></param>
        private void RegisterPluginImpl(Type pluginType, Type pluginImplType)
        {
            if (!pluginImplType.IsAbstract && pluginImplType.IsPublic)
            {
                ArrayList typeList = (ArrayList)_pluginTypesTable[pluginType];
                Debug.Assert(typeList != null, "Illegal state! no type list was found in the table");
                typeList.Add(pluginImplType);
            }
        }
        #endregion
    }
}
