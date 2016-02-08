// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Diagnostics;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.PostEditor
{
    interface IFocusableControl
    {
        bool ContainsFocus { get; }
        bool Focus();
        bool Visible { get; }
    }
    /// <summary>
    /// Summary description for FocusListManager.
    /// </summary>
    internal class FocusControlManager
    {
        private ArrayList _focusControlList;
        public FocusControlManager()
        {
            _focusControlList = new ArrayList();
        }

        public void AddControl(IFocusableControl control)
        {
            _focusControlList.Add(control);
        }

        public void RemoveControl(IFocusableControl control)
        {
            _focusControlList.Remove(control);
        }

        public void FocusNextControl()
        {
            IFocusableControl nextControl = GetNextFocusableControl(true);
            if (nextControl != null)
            {
                nextControl.Focus();
            }
        }

        public void FocusPreviousControl()
        {
            IFocusableControl prevControl = GetNextFocusableControl(false);
            if (prevControl != null)
            {
                prevControl.Focus();
            }
        }

        private IFocusableControl GetNextFocusableControl(bool forward)
        {
            int index = GetFocusedControlIndex();
            if (index == -1)
                return null;
            int nextIndex = GetNextIndex(index, forward);
            while (nextIndex != index)
            {
                IFocusableControl control = (IFocusableControl)_focusControlList[nextIndex];
                if (!control.Visible)
                    nextIndex = GetNextIndex(nextIndex, forward);
                else
                {
                    return control;
                }
            }
            return null;
        }

        private int GetNextIndex(int currIndex, bool forward)
        {
            if (_focusControlList.Count == 0)
                return -1;

            if (forward)
                currIndex = (currIndex + 1) % _focusControlList.Count;
            else
            {
                if (currIndex == 0)
                    currIndex = _focusControlList.Count - 1;
                else
                    currIndex = currIndex - 1;
            }
            return currIndex;
        }

        private int GetFocusedControlIndex()
        {
            if (_focusControlList.Count == 0)
                return -1;

            for (int i = 0; i < _focusControlList.Count; i++)
            {
                IFocusableControl control = (IFocusableControl)_focusControlList[i];
                if (control.ContainsFocus)
                    return i;
            }
            return 0;
        }
    }

    internal class FocusableControl : IFocusableControl
    {
        private Control _control;
        public FocusableControl(Control control)
        {
            _control = control;
        }
        public bool ContainsFocus
        {
            get { return _control.ContainsFocus; }
        }

        public bool Focus()
        {
            return ControlHelper.FocusControl(_control, true);
        }

        public bool Visible
        {
            get { return _control.Visible; }
        }
    }

    internal class FocusableControlProxy : IFocusableControl
    {
        private IFocusableControl _control;

        protected FocusableControlProxy()
        {
            _control = NullFocusableControl.Instance;
        }

        public FocusableControlProxy(IFocusableControl control)
        {
            _control = control;
        }
        public bool ContainsFocus
        {
            get { return FocusableControlTarget.ContainsFocus; }
        }

        public bool Focus()
        {
            return FocusableControlTarget.Focus();
        }

        public bool Visible
        {
            get { return FocusableControlTarget.Visible; }
        }

        public virtual IFocusableControl FocusableControlTarget
        {
            get
            {
                return _control;
            }
        }

    }

    internal class NullFocusableControl : IFocusableControl
    {
        public readonly static IFocusableControl Instance = new NullFocusableControl();

        private NullFocusableControl()
        {
        }
        public bool ContainsFocus
        {
            get { return false; }
        }

        public bool Focus()
        {
            return false;
        }

        public bool Visible
        {
            get { return false; }
        }
    }

}
