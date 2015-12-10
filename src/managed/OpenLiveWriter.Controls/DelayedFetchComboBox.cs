// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Interop.Windows;

namespace OpenLiveWriter.Controls
{

    /// <summary>
    /// Summary description for DelayedFetchComboBox.
    /// </summary>
    public class DelayedFetchComboBox : ComboBox
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        private IDelayedFetchHandler _fetchHandler;
        private DropDownNotifier _dropDownNotifier;

        public DelayedFetchComboBox(System.ComponentModel.IContainer container)
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            container.Add(this);
            InitializeComponent();
        }

        public DelayedFetchComboBox()
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            InitializeComponent();

            //
            // TODO: Add any constructor code after InitializeComponent call
            //
        }

        public void Initialize(object initialValue, IDelayedFetchHandler fetchHandler)
        {
            Initialize(new object[] { initialValue }, initialValue, fetchHandler);
        }

        public void Initialize(object[] items, object initialValue, IDelayedFetchHandler fetchHandler)
        {
            Items.Clear();
            Items.AddRange(items);
            SelectedItem = initialValue;
            _fetchHandler = fetchHandler;
        }

        /// <summary>
        /// Notification that the combo is dropping down.
        /// </summary>
        private void ComboDroppingDown()
        {
            if (_fetchHandler != null)
            {
                using (new WaitCursor())
                {
                    // attempt to fetch items
                    object[] items = _fetchHandler.FetchItems(FindForm());

                    // if we got some items then merge them with the existing combo
                    // and then disable fetching for this session
                    if (items != null)
                    {
                        foreach (object item in items)
                        {
                            if (!Items.Contains(item))
                                Items.Add(item);
                        }

                        _fetchHandler = null;
                    }
                }
            }
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);

            if (Parent != null)
            {
                if (Parent.IsHandleCreated)
                    InstallDropDownNotifier();
                else
                    Parent.HandleCreated += new EventHandler(Parent_HandleCreated);
            }
            else
            {
                RemoveDropDownNotifier();
            }
        }

        private void Parent_HandleCreated(object sender, EventArgs e)
        {
            Parent.HandleCreated -= new EventHandler(Parent_HandleCreated);
            InstallDropDownNotifier();
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                RemoveDropDownNotifier();

                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        private void InstallDropDownNotifier()
        {
            RemoveDropDownNotifier();
            _dropDownNotifier = new DropDownNotifier(this);
        }

        private void RemoveDropDownNotifier()
        {
            if (_dropDownNotifier != null)
            {
                _dropDownNotifier.Dispose();
                _dropDownNotifier = null;
            }
        }

        private class DropDownNotifier : NativeWindow, IDisposable
        {
            public DropDownNotifier(DelayedFetchComboBox delayedFetchCombo)
            {
                _delayedFetchCombo = delayedFetchCombo;
                AssignHandle(delayedFetchCombo.Parent.Handle);
            }

            protected override void WndProc(ref Message m)
            {
                // notify combo that it is dropping down
                if (m.Msg == WM.COMMAND && MessageHelper.HIWORDToInt32(m.WParam) == CBN.DROPDOWN)
                {
                    try
                    {
                        _delayedFetchCombo.ComboDroppingDown();
                    }
                    catch (Exception ex)
                    {
                        Trace.Fail("Unexpected exception processing combo drop down: " + ex.ToString());
                    }
                }

                // always call base
                base.WndProc(ref m);
            }

            public void Dispose()
            {
                ReleaseHandle();
            }

            private DelayedFetchComboBox _delayedFetchCombo;

        }

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            //
            // DelayedFetchComboBox
            //

        }
        #endregion

    }

    public interface IDelayedFetchHandler
    {
        object[] FetchItems(IWin32Window owner);
    }

}
