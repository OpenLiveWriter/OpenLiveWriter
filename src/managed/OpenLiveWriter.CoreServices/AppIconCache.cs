// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using Microsoft.Win32;
using System;
using System.Collections;
using System.Drawing;
using System.Runtime.InteropServices;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Interop.Windows;

// NOTE: This code removed from the CoreServices project because it is obsoleted by
// ShellHelper.GetSmallIconForExtension and ShellHelper.GetLargeIconForExtension

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// A utility that loads and caches Icons
    /// </summary>
    [Obsolete("Use ShellHelper Icon methods instead (produce more correct/complete results)", true )]
    public class AppIconCache
    {
        /// <summary>
        /// Gets an Icon based upon the progId of a particular application
        /// </summary>
        /// <param name="progId">The progId for which is locate the Icon</param>
        /// <returns>The appropriate default Icon</returns>
        public static Icon GetIconFromProgId(string progId)
        {
            if (progId == null || !m_icons.ContainsKey(progId))
            {
                IntPtr hInstance = Marshal.GetHINSTANCE(typeof(AppIconCache).Module);

                string IconPath = GetDefaultIconPath(progId);
                if (IconPath != null)
                {
                    string[] iconInfo = IconPath.Split(',');
                    IntPtr hIcon = Shell32.ExtractIcon(hInstance, iconInfo[0], Convert.ToInt32(iconInfo[1]));
                    Icon icon = Icon.FromHandle(hIcon);
                    if (progId == null)
                        return icon;
                    m_icons[progId] = icon;
                }
                else
                {
                    if (progId == null)
                        return null;
                    m_icons[progId] = null;
                }
            }
            return (Icon) m_icons[progId];
        }

        public static Icon GetUnknownIcon()
        {
            return GetIconFromProgId(null);
        }

        /// <summary>
        /// Gets the path and index to a default icon based upon a progId
        /// </summary>
        /// <param name="progId">The progId</param>
        /// <returns>A string representing the path to the default icon</returns>
        private static string GetDefaultIconPath(string progId)
        {
            if (progId == null)
            {
                return Environment.SystemDirectory + @"\Shell32.dll,0";
            }

            string iconPath = null;
            RegistryKey progKey = Registry.ClassesRoot.OpenSubKey(progId);
            if (progKey != null)
            {
                RegistryKey openKey = progKey.OpenSubKey("DefaultIcon");
                if (openKey != null)
                    iconPath = (string) openKey.GetValue(null);
            }
            return iconPath;
        }

        /// <summary>
        /// Cached set of application icons
        /// </summary>
        private static Hashtable m_icons = Hashtable.Synchronized(new Hashtable());
    }

}
