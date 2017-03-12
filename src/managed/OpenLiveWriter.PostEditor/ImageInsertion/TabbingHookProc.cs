// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Windows.Forms;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Interop.Windows;

namespace OpenLiveWriter.PostEditor.ImageInsertion
{
    /// <summary>
    /// Summary description for TabbingHookProc.
    /// </summary>
    class TabbingHookProc : KeyboardHook
    {
        private TabLightweightControl _tabs;

        public TabbingHookProc(TabLightweightControl tabs)
        {
            _tabs = tabs;
        }

        /// <summary>
        /// Help requested event
        /// </summary>
        public event EventHandler Tab;
        protected virtual void OnTab(EventArgs ea)
        {
            if (Tab != null)
                Tab(this, ea);
        }

        /// <summary>
        /// Override F1 to prevent it from getting to MSHTML. Will fire the HelpRequested
        /// event when the F1 key is pressed
        /// </summary>
        protected override IntPtr OnKeyHooked(int nCode, UIntPtr wParam, IntPtr lParam)
        {
            // only process HC_ACTION
            if (nCode == HC.ACTION)
            {
                // We want one key event per key key-press. To do this we need to
                // mask out key-down repeats and key-ups by making sure that bits 30
                // and 31 of the lParam are NOT set. Bit 30 specifies the previous
                // key state. The value is 1 if the key is down before the message is
                // sent; it is 0 if the key is up. Bit 31 specifies the transition
                // state. The value is 0 if the key is being pressed and 1 if it is
                // being released. Therefore, we are only interested in key events
                // where both bits are set to 0. To test for both of these bits being
                // set to 0 we use the constant REDUNDANT_KEY_EVENT_MASK.
                const uint REDUNDANT_KEY_EVENT_MASK = 0xC0000000;
                if (((uint)lParam & REDUNDANT_KEY_EVENT_MASK) == 0)
                {
                    // extract the keyCode and combine with modifier keys
                    Keys keyCombo =
                        ((Keys)(int)wParam & Keys.KeyCode) | KeyboardHelper.GetModifierKeys();

                    if (_tabs.CheckForTabSwitch(keyCombo))
                        return new IntPtr(1);
                }
            }

            // key not handled by our hook, continue processing
            return CallNextHook(nCode, wParam, lParam);
        }
    }
}
