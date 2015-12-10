// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using Microsoft.Win32;

namespace OpenLiveWriter.CoreServices
{
    public class UserPreferencesMonitor : IDisposable
    {
        public event EventHandler AccessibilityUserPreferencesChanged;

        public UserPreferencesMonitor()
        {
            SystemEvents.UserPreferenceChanged += new UserPreferenceChangedEventHandler(SystemEvents_UserPreferenceChanged);
        }

        ~UserPreferencesMonitor()
        {
            Trace.Fail("Failed to dispose of UserPreferencesMonitor");
            Dispose();
        }

        void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            // High contrast setting change is part of the Accessibility category.
            if (e.Category == UserPreferenceCategory.Accessibility)
            {
                OnAccessibilityUserPreferencesChanged();
            }
        }

        private void OnAccessibilityUserPreferencesChanged()
        {
            if (AccessibilityUserPreferencesChanged != null)
                AccessibilityUserPreferencesChanged(this, EventArgs.Empty);
        }

        private bool _disposed;
        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                SystemEvents.UserPreferenceChanged -= new UserPreferenceChangedEventHandler(SystemEvents_UserPreferenceChanged);
            }

            GC.SuppressFinalize(this);
        }
    }
}
