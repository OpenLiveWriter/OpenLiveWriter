// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using OpenLiveWriter.ApplicationFramework.Preferences;

namespace OpenLiveWriter.ApplicationFramework.ApplicationStyles
{
    /// <summary>
    /// Provides application management service.
    /// </summary>
    public sealed class ApplicationStyleManager
    {

        /// <summary>
        /// Gets or sets the ApplicationStyle object
        /// </summary>
        public static ApplicationStyle ApplicationStyle
        {
            get
            {
                return ApplicationManager.ApplicationStyle;
            }
        }

        #region ApplicatonStyle access/monitoring

        public static event EventHandler ApplicationStyleChanged
        {
            add
            {
                ApplicationStylePreferences.PreferencesChanged += value ;
            }
            remove
            {
                ApplicationStylePreferences.PreferencesChanged -= value ;
            }
        }

        public static void CheckForApplicationStyleChanges()
        {
            ApplicationStylePreferences.CheckForChanges() ;
        }

        /// Get the ApplicationStylePreferences instance for the current thread
        /// </summary>
        private static ApplicationStylePreferences ApplicationStylePreferences
        {
            get
            {
                if (_applicationStylePreferences == null)
                {
                    _applicationStylePreferences = Activator.CreateInstance(typeof(ApplicationStylePreferences), new object[] { true } ) as ApplicationStylePreferences ;
                }
                return _applicationStylePreferences;
            }
        }
        [ThreadStatic]
        private static ApplicationStylePreferences _applicationStylePreferences ;


        #endregion

    }
}
