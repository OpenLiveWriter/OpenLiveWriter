// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace OpenLiveWriter.CoreServices.UI
{
    /// <summary>
    /// Summary description for ControlColorTheme.
    /// </summary>
    public abstract class ControlUITheme
    {
        private Control _control;
        public ControlUITheme(Control control, bool applyTheme)
        {
            _control = control;
            Microsoft.Win32.SystemEvents.UserPreferenceChanged += new Microsoft.Win32.UserPreferenceChangedEventHandler(SystemEvents_UserPreferenceChanged);

            control.Disposed += new EventHandler(control_Disposed);

            if (applyTheme)
                ApplyTheme();
        }

        protected Control Control
        {
            get { return _control; }
        }

        public void Refresh()
        {
            if (_control.InvokeRequired)
            {

                try
                {
                    if (ControlHelper.ControlCanHandleInvoke(_control))
                        _control.BeginInvoke(new InvokeInUIThreadDelegate(ApplyTheme));
                }
                catch (Exception e)
                {
                    Trace.Fail(e.Message, e.ToString());
                }
            }
            else
                ApplyTheme();
        }

        protected bool UseHighContrastTheme
        {
            get { return SystemInformation.HighContrast; }
        }

        protected virtual void ApplyTheme(bool highContrast)
        {
            PerformLayoutAndInvalidate();
        }

        protected void PerformLayoutAndInvalidate()
        {
            Control.PerformLayout();
            Control.Invalidate();
        }

        private void SystemEvents_UserPreferenceChanged(object sender, Microsoft.Win32.UserPreferenceChangedEventArgs e)
        {
            if (ControlHelper.ControlCanHandleInvoke(_control))
            {
                _control.BeginInvoke(new InvokeInUIThreadDelegate(ApplyTheme));
            }
        }

        protected void ApplyTheme()
        {
            ApplyTheme(UseHighContrastTheme);
        }

        private void control_Disposed(object sender, EventArgs e)
        {
            Microsoft.Win32.SystemEvents.UserPreferenceChanged -= new Microsoft.Win32.UserPreferenceChangedEventHandler(SystemEvents_UserPreferenceChanged);
            Control.Disposed -= new EventHandler(control_Disposed);
            Dispose();
        }

        protected virtual void Dispose()
        {
        }
    }
}
