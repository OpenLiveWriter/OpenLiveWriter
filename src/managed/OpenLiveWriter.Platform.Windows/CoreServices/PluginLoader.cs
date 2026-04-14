// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Summary description for PluginLoader.
    /// </summary>
    [SupportedOSPlatform("windows")]
    public class PluginLoader
    {
        private readonly AssemblyName _apiAssemblyName;

        public delegate void PluginLoadedCallback(Type pluginType, Type type);

        readonly Hashtable _typeHandlers;
        public PluginLoader(Assembly apiAssembly)
        {
            _apiAssemblyName = apiAssembly == null ? null : apiAssembly.GetName();
            _typeHandlers = new Hashtable();
        }

        public void LoadPluginsFromType(Type externalType)
        {
            foreach (Type pluginType in _typeHandlers.Keys)
            {
                if (pluginType.IsAssignableFrom(externalType))
                {
                    GetTypeCallbackList(pluginType).RegisterType(externalType);
                }
            }
        }
        public void LoadPluginsFromDirectory(string directory, bool showErrors)
        {
            string[] filenames = Directory.GetFiles(directory);
            foreach (string filename in filenames)
            {
                if (Path.GetExtension(filename).ToUpperInvariant() == ".DLL")
                {
                    string assemblyPath = Path.Combine(directory, filename);
                    LoadPluginsFromAssemblyPath(assemblyPath, showErrors);
                }
            }
        }

        private static Assembly LoadFromWithRetry(string assemblyPath, int numRetries)
        {
            for (int i = 0; ; i++)
            {
                try
                {
                    return Assembly.LoadFrom(assemblyPath);
                }
                catch (FileLoadException)
                {
                    Trace.WriteLine("Failed to load assembly " + assemblyPath + " on attempt " + i);
                    if (i >= numRetries)
                    {
                        throw;
                    }
                    Thread.Sleep(1000);
                }
            }
        }

        public void LoadPluginsFromAssemblyPath(string assemblyPath, bool showErrors)
        {
            try
            {
                lock (this)
                {
                    string assemblyPathNormalized = assemblyPath.ToLower(CultureInfo.InvariantCulture);
                    if (!loadedPlugins.Contains(assemblyPathNormalized))
                    {
                        Assembly assembly = LoadFromWithRetry(assemblyPath, 3);

                        loadedPlugins.Add(assemblyPathNormalized);
                        LoadPluginsFromAssembly(assembly);
                    }
                }

            }
            catch (ReflectionTypeLoadException e)
            {
                Trace.WriteLine(String.Format(CultureInfo.InvariantCulture, "Failed to load plugin assembly [{0}]", assemblyPath));
                Trace.WriteLine(e.ToString());
                if (e.LoaderExceptions != null)
                {
                    foreach (Exception e1 in e.LoaderExceptions)
                    {
                        Trace.WriteLine(e1.ToString());
                    }
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(String.Format(CultureInfo.InvariantCulture, "Failed to load plugin assembly [{0}]", assemblyPath));
                Trace.WriteLine(e.ToString());
            }
        }

        private readonly HashSet loadedPlugins = new HashSet();
        public void LoadPluginsFromAssembly(Assembly assembly)
        {
            if (_apiAssemblyName != null)
            {
                foreach (AssemblyName assemblyName in assembly.GetReferencedAssemblies())
                {
                    if (assemblyName.Name == _apiAssemblyName.Name
                        && ArrayHelper.CompareBytes(assemblyName.GetPublicKeyToken(), _apiAssemblyName.GetPublicKeyToken())
                        && assemblyName.Version > _apiAssemblyName.Version)
                    {
                        // TODO: Show message indicating plugin expects future version of Writer
                        //MessageBox.Show("Foo");
                        Trace.WriteLine("Plugin assembly " + assembly.GetName().Name + " has a dependency on a future version of the API DLL");
                        return;
                    }
                }
            }

            Type[] types = assembly.GetTypes();
            foreach (Type externalType in types)
            {
                LoadPluginsFromType(externalType);
            }
        }

        public void AddPluginTypeCallback(Type pluginType, PluginLoadedCallback loadedCallback)
        {
            TypeRegistrationCallbackList list = GetTypeCallbackList(pluginType);
            if (list == null)
            {
                list = new TypeRegistrationCallbackList(pluginType);
                _typeHandlers[pluginType] = list;
            }
            list.AddRegisterTypeCallback(loadedCallback);
        }

        public void RemovePluginTypeCallback(Type type, PluginLoadedCallback loadedCallback)
        {
            TypeRegistrationCallbackList list = GetTypeCallbackList(type);
            if (list != null)
            {
                list.RemoveRegisterTypeCallback(loadedCallback);
                if (list.Count == 0)
                {
                    _typeHandlers.Remove(type);
                }
            }
        }

        private TypeRegistrationCallbackList GetTypeCallbackList(Type type)
        {
            return (TypeRegistrationCallbackList)_typeHandlers[type];
        }

        [DllImport("mscoree.dll")]
        private static extern int GetFileVersion(
            [MarshalAs(UnmanagedType.LPWStr)] string szFilename,
            [MarshalAs(UnmanagedType.LPWStr)] StringBuilder szBuffer,
            int cchBuffer,
            out int dwLength);

        private class TypeRegistrationCallbackList
        {
            readonly ArrayList _registerCallbacks;
            readonly Type _pluginType;
            public TypeRegistrationCallbackList(Type pluginType)
            {
                _registerCallbacks = new ArrayList();
                _pluginType = pluginType;
            }

            public void RegisterType(Type type)
            {
                foreach (PluginLoadedCallback callback in _registerCallbacks)
                {
                    callback(_pluginType, type);
                }
            }

            public void AddRegisterTypeCallback(PluginLoadedCallback loadedCallback)
            {
                _registerCallbacks.Add(loadedCallback);
            }

            public void RemoveRegisterTypeCallback(PluginLoadedCallback loadedCallback)
            {
                _registerCallbacks.Remove(loadedCallback);
            }

            public int Count
            {
                get
                {
                    return _registerCallbacks.Count;
                }
            }
        }
    }
}
