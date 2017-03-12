// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Diagnostics;
using System.Windows.Forms;

namespace OpenLiveWriter.Controls
{
    /// <summary>
    /// Custom Focus and Accessibility controller for a LightweightControlContainerControl (LCCC).
    /// </summary>
    internal class LCCCFocusAndAccessibilityController
    {
        private LightweightControlContainerControl _control;
        private bool _enteredState; //tracks the enter/leave state of the control (while entered, focus is restored)
        private int _focusedControlIndex = -1;
        private ArrayList _controls;
        private bool _wrap = false;

        internal LCCCFocusAndAccessibilityController(LightweightControlContainerControl control)
        {
            _control = control;
            _controls = new ArrayList();
        }

        internal LCCCFocusAndAccessibilityController(LightweightControlContainerControl control, bool wrap)
            : this(control)
        {
            _wrap = wrap;
        }

        /// <summary>
        /// Add a control to the list of accessible controls.
        /// </summary>
        /// <param name="control"></param>
        public void AddControl(LightweightControl control)
        {
            Debug.Assert(!_controls.Contains(control));
            _controls.Add(control);
        }

        /// <summary>
        /// Add a control to the list of accessible controls.
        /// </summary>
        /// <param name="control"></param>
        public void AddControl(Control control)
        {
            Debug.Assert(!_controls.Contains(control));
            _controls.Add(control);
        }

        public void ClearControls()
        {
            _controls.Clear();
        }

        internal void NotifyFocusedControlChanged()
        {
            LightweightControlContainerControl.LightweightControlContainerAccessibility accObject = (LightweightControlContainerControl.LightweightControlContainerAccessibility)_control.AccessibilityObject;
            if (accObject != null)
            {
                int index = GetFocusedControlIndex();
                if (index >= 0)
                    accObject.NotifyClients(AccessibleEvents.Focus, index);
            }
        }

        public int Count
        {
            get { return _controls.Count; }
        }

        /// <summary>
        /// Returns the Accessible object for the child at the specified index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public AccessibleObject GetAccessibleObject(int index)
        {
            object obj = _controls[index];
            if (obj is Control)
                return ((Control)obj).AccessibilityObject;
            else if (obj is LightweightControl)
                return ((LightweightControl)obj).AccessibilityObject;
            else
            {
                Debug.Fail("Unsupported control type detected: " + obj.GetType().Name);
                return null;
            }
        }

        /// <summary>
        /// Focus the next control
        /// </summary>
        /// <param name="forward"></param>
        /// <param name="wrap"></param>
        /// <returns></returns>
        private bool FocusNextControl(bool forward)
        {
            int index = GetFocusedControlIndex();
            return VisitControlsUntilStopValue(index, forward, _wrap, true, FocusControlVisitor.Instance);
        }

        /// <summary>
        /// Focus the control at the specified index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private bool FocusControl(int index)
        {
            return VisitControl(index, FocusControlVisitor.Instance);
        }

        /// <summary>
        /// Visit the control at the specified index.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="visitor"></param>
        /// <returns>true if the visit was successful</returns>
        private bool VisitControl(int index, ControlVisitor visitor)
        {
            object obj = _controls[index];
            if (obj is Control)
                return visitor.VisitControl((Control)obj);
            else if (obj is LightweightControl)
                return visitor.VisitLightweightControl((LightweightControl)obj);
            else
            {
                Debug.Fail("Unsupported control type detected: " + obj.GetType().Name);
                return false;
            }
        }

        /// <summary>
        /// Visit all controls in the specified forward/backward direction.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="forward"></param>
        /// <param name="wrap"></param>
        /// <param name="stopResult">the value expected from the visitor to stop the visit loop</param>
        /// <param name="visitor"></param>
        /// <returns></returns>
        private bool VisitControlsUntilStopValue(int from, bool forward, bool wrap, bool stopResult, ControlVisitor visitor)
        {
            int currIndex = from;
            while (true)
            {
                int nextIndex = -1;
                if (currIndex < 0)
                {
                    if (_controls.Count > 0)
                        nextIndex = forward ? 0 : _controls.Count - 1;
                }
                else
                    nextIndex = GetNextControlIndex(currIndex, forward, wrap);

                if (nextIndex != -1)
                {
                    if (VisitControl(nextIndex, visitor) == stopResult)
                        return stopResult;
                }
                else
                {
                    return !stopResult;
                }
                currIndex = nextIndex;
            }
        }

        /// <summary>
        /// Returns the index of the currently focused control.
        /// </summary>
        /// <returns></returns>
        internal int GetFocusedControlIndex()
        {
            for (int i = 0; i < _controls.Count; i++)
            {
                if (VisitControl(i, IsFocusControlVisitor.Instance))
                    return i;
            }
            return -1;
        }

        internal int GetNextControlIndex(bool forward)
        {
            int index = GetFocusedControlIndex();
            if (index != -1)
            {
                return GetNextControlIndex(index, forward, false);
            }
            return -1;
        }

        /// <summary>
        /// Returns the index of the next control in the specified forward/backward direction.
        /// </summary>
        /// <param name="from">the index to start from</param>
        /// <param name="forward"></param>
        /// <param name="wrap"></param>
        /// <returns></returns>
        public int GetNextControlIndex(int from, bool forward, bool wrap)
        {
            int nextIndex = from;

            while (true)
            {
                nextIndex += forward ? 1 : -1;

                if (wrap)
                    nextIndex = (nextIndex + _controls.Count) % _controls.Count;

                if (nextIndex < 0 || nextIndex >= _controls.Count)
                {
                    return -1;
                }

                if (!ControlAtIndexCanFocus(nextIndex))
                {
                    if (nextIndex == from)
                        return -1;

                    continue;
                }

                return nextIndex;
            }
        }

        private bool ControlAtIndexCanFocus(int index)
        {
            object obj = _controls[index];

            if (obj is Control)
            {
                Control c = ((Control)obj);
                return c.Visible && c.Enabled && c.CanFocus;
            }

            return true;
        }

        /// <summary>
        /// Move focus from the current control to the next control in the specified direction.
        /// </summary>
        /// <param name="forward"></param>
        /// <returns></returns>
        internal bool ProcessControlFocusNavigation(bool forward)
        {
            if (forward)
            {
                if (_control.ActiveControl != null)
                {
                    if (_control.SelectNextControl(_control.ActiveControl, forward, true, true, false))
                        return true;
                }
            }
            else
            {
                Control focusedControl = GetInnerMostFocusedControl(_control);
                if (focusedControl != null && focusedControl != _control)
                {
                    while (!focusedControl.Parent.SelectNextControl(focusedControl, forward, true, true, false) && focusedControl != _control)
                    {
                        if (focusedControl is ISupportsRightAndLeftArrows)
                        {
                            return true;
                        }
                        focusedControl = focusedControl.Parent;
                    }
                    if (focusedControl != _control)
                        return true;
                }
            }
            return FocusNextControl(forward);
        }

        /// <summary>
        /// Returns the inner most focused control.
        /// </summary>
        /// <param name="control"></param>
        /// <returns></returns>
        private Control GetInnerMostFocusedControl(Control control)
        {
            while (control is ContainerControl)
            {
                ContainerControl containerControl = (ContainerControl)control;
                if (containerControl.Focused)
                    return containerControl;
                control = containerControl.ActiveControl;
            }
            return control;
        }

        /// <summary>
        /// Handles OnEnter for the LCCC.
        /// </summary>
        internal void OnEnter()
        {
            _enteredState = true;
        }

        public bool DoDefaultAction()
        {
            int index = GetFocusedControlIndex();
            if (index >= 0)
            {
                if (_controls[index] is Control)
                    return false;
                AccessibleObject obj = GetAccessibleObject(index);
                obj.DoDefaultAction();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Handles OnLeave for the LCCC.
        /// </summary>
        internal void OnLeave()
        {
            //_restoreFocusTargetControl = null;
            _focusedControlIndex = -1;
            _enteredState = false;

            if (_control.ActiveLightweightControl != null)
            {
                LightweightControl focusedControl = _control.ActiveLightweightControl.GetFocusedControl();
                if (focusedControl != null)
                    focusedControl.Unfocus();
            }
        }

        /// <summary>
        /// Handles OnGotFocus for the LCCC.
        /// </summary>
        internal void OnGotFocus()
        {
            if (_focusedControlIndex >= 0)
            {
                FocusControl(_focusedControlIndex);
            }
            else
            {
                if (_controls.Count > 0)
                    FocusControl(0);
            }

            _control.Invalidate(true);
        }

        /// <summary>
        /// Handles OnLostFocus for the LCCC.
        /// </summary>
        internal void OnLostFocus()
        {
            if (_control.ActiveLightweightControl != null)
            {
                LightweightControl focusedControl = _control.ActiveLightweightControl.GetFocusedControl();
                if (focusedControl != null)
                {
                    if (_enteredState)
                    {
                        //Focus was lost, but since the control is in "entered" state, the focus needs
                        //to be restored to the appropriate subcontrol when focus returns to this control
                        //(this can happen when the active window changes)
                        _focusedControlIndex = GetFocusedControlIndex();
                    }
                    focusedControl.Unfocus();
                }
                _control.ActiveLightweightControl = null;
            }

            _control.Invalidate(true);
        }

        /// <summary>
        /// Interface for building helpers that visit controls in the list.
        /// </summary>
        internal class ControlVisitor
        {
            public virtual bool VisitControl(Control control)
            {
                return false;
            }

            public virtual bool VisitLightweightControl(LightweightControl control)
            {
                return false;
            }
        }

        /// <summary>
        /// Visitor that focuses a target control.
        /// </summary>
        class FocusControlVisitor : ControlVisitor
        {
            public static ControlVisitor Instance = new FocusControlVisitor();
            public override bool VisitControl(Control control)
            {
                if (!control.TabStop || !control.CanFocus)
                    return false;

                bool focused = control.Focus();
                return true;
            }

            public override bool VisitLightweightControl(LightweightControl control)
            {
                if (!control.Visible || !control.TabStop)
                    return false;

                if (control.Parent != null)
                {
                    if (!control.Parent.Focused)
                        control.Parent.Focus();
                    if (control.Parent is LightweightControlContainerControl)
                        (control.Parent as LightweightControlContainerControl).ActiveControl = null;
                }
                bool focused = control.Focus();
                return focused;
            }
        }

        /// <summary>
        /// Visitor that determines whether the specified target control is currently focused (or contains focus).
        /// </summary>
        class IsFocusControlVisitor : ControlVisitor
        {
            public static ControlVisitor Instance = new IsFocusControlVisitor();
            public override bool VisitControl(Control control)
            {
                return control.Focused || control.ContainsFocus;
            }

            public override bool VisitLightweightControl(LightweightControl control)
            {
                return control.Focused;
            }
        }
    }

    /// <summary>
    /// This is for controls, that might be part of a lighwieght control, but would like to handle
    /// the right and left arrows on their now instead of passing it up to its parents
    /// </summary>
    public interface ISupportsRightAndLeftArrows
    {

    }
}
