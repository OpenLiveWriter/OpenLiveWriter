// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.HtmlEditor
{
    using System.Windows.Forms;
    using mshtml;
    using Mshtml;

    /// <summary>
    /// Handles the Ctrl+LShift or Ctrl+RShift shortcuts, which is the shortcut to toggle the current paragraph
    /// between RTL and LTR. By default, MSHTML will pick up this keystroke and render the paragraph correctly but it
    /// won't actually add any markup. This doesn't provide a good WYSIWYG experience because the published HTML won't
    /// match the compose experience, so we hide this key combination from MSHTML and implement this functionality
    /// ourselves instead. Our normal shortcut code path doesn't have support for differentiating between LShift and
    /// RShift, so we do it manually instead.
    /// </summary>
    internal class RtlAcceleratorTranslator
    {
        /// <summary>
        /// Gets or sets whether the left shift button is pressed down.
        /// </summary>
        private bool leftShiftDown;

        /// <summary>
        /// Gets or sets whether the right shift button is pressed down.
        /// </summary>
        private bool rightShiftDown;

        /// <summary>
        /// Gets or sets whether the control button is pressed down.
        /// </summary>
        private bool ctrlDown;

        /// <summary>
        /// Processes the keystroke event.
        /// </summary>
        /// <param name="inEvtDispId">The dispatch id of the event.</param>
        /// <param name="pIEventObj">The event object.</param>
        /// <returns>A KeyEventArgs object with the current state of the RTL accelerator keys. This can be null if there was nothing to process.</returns>
        public KeyEventArgs ProcessEvent(int inEvtDispId, IHTMLEventObj pIEventObj)
        {
            Keys currentKey = (Keys)pIEventObj.keyCode;
            KeyEventArgs e = null;

            if (inEvtDispId == DISPID_HTMLELEMENTEVENTS2.ONKEYDOWN)
            {
                if (currentKey == Keys.ControlKey)
                {
                    this.ctrlDown = true;
                }
                else if (currentKey == Keys.ShiftKey)
                {
                    // The first shift key down is the one we'll track.
                    if (((IHTMLEventObj3)pIEventObj).shiftLeft && !this.rightShiftDown)
                    {
                        this.leftShiftDown = true;
                    }
                    else if (!this.leftShiftDown)
                    {
                        this.rightShiftDown = true;
                    }
                }
                else
                {
                    // If any other keystrokes beside CTRL and SHIFT are pressed, stop tracking the keystrokes we've
                    // seen. For example, a user might hit CTRL+SHIFT+LEFT to start highlighting a word and we don't
                    // want that to trigger the RTL/LTR command.
                    this.Reset();
                }
            }
            else if (inEvtDispId == DISPID_HTMLELEMENTEVENTS2.ONKEYUP)
            {
                // We always want to fire an event if CTRL goes up with SHIFT still pressed or vice-versa so that
                // MSHTML doesn't attempt to handle the keystrokes.
                if ((currentKey == Keys.ControlKey && pIEventObj.shiftKey) || (currentKey == Keys.ShiftKey && pIEventObj.ctrlKey))
                {
                    e = this.GetKeyEventArgs();
                }

                this.Reset();
            }

            return e;
        }

        /// <summary>
        /// Gets the current KeyEventArgs based on the current state of the RTL accelerator keys.
        /// </summary>
        /// <returns>A KeyEventArgs with the current state of the RTL accelerator keys.</returns>
        private KeyEventArgs GetKeyEventArgs()
        {
            Keys keysPressed = Keys.None;

            if (this.ctrlDown)
            {
                keysPressed |= Keys.Control;
            }

            if (this.leftShiftDown)
            {
                keysPressed |= Keys.LShiftKey;
            }
            else if (this.rightShiftDown)
            {
                keysPressed |= Keys.RShiftKey;
            }

            KeyEventArgs e = new KeyEventArgs(keysPressed);

            if (keysPressed == Keys.None)
            {
                e.SuppressKeyPress = true;
            }

            return e;
        }

        /// <summary>
        /// Resets the state of the RTL accelerator keys.
        /// </summary>
        private void Reset()
        {
            this.ctrlDown = false;
            this.leftShiftDown = false;
            this.rightShiftDown = false;
        }
    }
}
