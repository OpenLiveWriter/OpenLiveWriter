// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace OpenLiveWriter.Interop.Windows.TaskDialog
{
    /// <summary>
    /// Managed wrapper for the Windows Task Dialog API.
    /// http://msdn.microsoft.com/en-us/library/bb787471(VS.85).aspx
    /// </summary>
    public class TaskDialog
    {
        private TASKDIALOGCONFIG config;
        private readonly List<TaskDialogButton> buttons;
        private readonly List<TaskDialogButton> radioButtons;
        private TaskDialogIcon mainIcon;
        private TaskDialogIcon footerIcon;

        public TaskDialog()
        {
            config = new TASKDIALOGCONFIG();
            config.cbSize = Marshal.SizeOf(config);
            config.pfCallback = Callback;

            buttons = new List<TaskDialogButton>();
            radioButtons = new List<TaskDialogButton>();
        }

        #region Event callbacks

        /// <summary>
        /// Sent by the Task Dialog when the user selects a button or command link in the task dialog.
        /// </summary>
        public event TaskDialogButtonEventHandler ButtonClicked;

        /// <summary>
        /// Sent by the Task Dialog once the dialog has been created and before it is displayed.
        /// </summary>
        public event TaskDialogEventHandler Created;

        /// <summary>
        /// Sent by the Task Dialog when it is destroyed and its window handle is no longer valid.
        /// </summary>
        public event TaskDialogEventHandler Destroyed;

        /// <summary>
        /// Sent by the Task Dialog once the dialog has been created and before it is displayed.
        /// </summary>
        public event TaskDialogEventHandler DialogConstructed;

        /// <summary>
        /// Sent by the task dialog when the user clicks on the dialog's expando button.
        /// </summary>
        public event TaskDialogExpandoEventHandler ExpandoButtonClicked;

        /// <summary>
        /// Sent by the Task Dialog when the user presses F1 on the keyboard while the dialog has focus.
        /// </summary>
        public event TaskDialogEventHandler Help;

        /// <summary>
        /// Sent by the Task Dialog when the user clicks a hyperlink in the Task Dialog content.
        /// </summary>
        public event TaskDialogHyperlinkEventHandler HyperlinkClicked;

        /*
        /// <summary>
        /// Sent by the Task Dialog when a navigation has occurred.
        /// </summary>
        public event TaskDialogEventHandler Navigated;
         */

        /// <summary>
        /// Sent by the Task Dialog when the user selects a radio button in the task dialog.
        /// </summary>
        public event TaskDialogButtonEventHandler RadioButtonClicked;

        /// <summary>
        /// Sent by the Task Dialog approximately every 200 milliseconds. This notification
        /// is sent when the CallbackTimer property is set to true.
        /// </summary>
        public event TaskDialogTimerEventHandler TimerTicked;

        /// <summary>
        /// Sent by the task dialog when the user clicks the Task Dialog verification check box.
        /// </summary>
        public event TaskDialogVerificationEventHandler VerificationClicked;

        #endregion

        #region Option properties

        /// <summary>
        /// Enables hyperlink processing for the strings specified in the pszContent,
        /// pszExpandedInformation and pszFooter members. When enabled, these members
        /// may point to strings that contain hyperlinks in the following form:
        /// &lt;A HREF="executablestring"&gt;Hyperlink Text&lt;/A&gt;
        /// </summary>
        /// <remarks>
        /// TaskDialog will not actually execute any hyperlinks. Hyperlink execution
        /// must be performed in response to the HyperlinkClicked event.
        /// </remarks>
        public bool EnableHyperlinks
        {
            get { return TestFlag(TASKDIALOG_FLAGS.TDF_ENABLE_HYPERLINKS); }
            set { SetFlag(TASKDIALOG_FLAGS.TDF_ENABLE_HYPERLINKS, value); }
        }

        /// <summary>
        /// Indicates that the dialog should be able to be closed using Alt-F4,
        /// Escape, and the title bar's close button even if no cancel button is
        /// specified in either the dwCommonButtons or pButtons members.
        /// </summary>
        public bool AllowDialogCancellation
        {
            get { return TestFlag(TASKDIALOG_FLAGS.TDF_ALLOW_DIALOG_CANCELLATION); }
            set { SetFlag(TASKDIALOG_FLAGS.TDF_ALLOW_DIALOG_CANCELLATION, value); }
        }

        /// <summary>
        /// Indicates that the buttons specified in the Buttons property are to be
        /// displayed as command links (using a standard task dialog glyph) instead
        /// of push buttons. When using command links, all characters up to the
        /// first new line character in the Text property will be treated as
        /// the command link's main text, and the remainder will be treated as the
        /// command link's note.
        /// </summary>
        public bool UseCommandLinks
        {
            get { return TestFlag(TASKDIALOG_FLAGS.TDF_USE_COMMAND_LINKS); }
            set { SetFlag(TASKDIALOG_FLAGS.TDF_USE_COMMAND_LINKS, value); }
        }

        /// <summary>
        /// Indicates that the buttons specified in the Buttons property are to be
        /// displayed as command links (without a glyph) instead of push buttons.
        /// When using command links, all characters up to the first new line
        /// character in the Text property will be treated as the command
        /// link's main text, and the remainder will be treated as the command
        /// link's note.
        /// </summary>
        public bool UseCommandLinksNoIcon
        {
            get { return TestFlag(TASKDIALOG_FLAGS.TDF_USE_COMMAND_LINKS_NO_ICON); }
            set { SetFlag(TASKDIALOG_FLAGS.TDF_USE_COMMAND_LINKS_NO_ICON, value); }
        }

        /// <summary>
        /// Indicates that the string specified by the ExpandedInformation property
        /// is displayed at the bottom of the dialog's footer area instead of
        /// immediately after the dialog's content. This flag is ignored if the
        /// ExpandedInformation property is null.
        /// </summary>
        public bool ExpandFooterArea
        {
            get { return TestFlag(TASKDIALOG_FLAGS.TDF_EXPAND_FOOTER_AREA); }
            set { SetFlag(TASKDIALOG_FLAGS.TDF_EXPAND_FOOTER_AREA, value); }
        }

        /// <summary>
        /// Indicates that the string specified by the ExpandedInformation property
        /// is displayed when the dialog is initially displayed. This flag is
        /// ignored if the ExpandedInformation property is null.
        /// </summary>
        public bool ExpandedByDefault
        {
            get { return TestFlag(TASKDIALOG_FLAGS.TDF_EXPANDED_BY_DEFAULT); }
            set { SetFlag(TASKDIALOG_FLAGS.TDF_EXPANDED_BY_DEFAULT, value); }
        }

        /// <summary>
        /// Indicates that the verification checkbox in the dialog is checked
        /// when the dialog is initially displayed. This flag is ignored if the
        /// VerificationText property is null.
        /// </summary>
        public bool VerificationFlagChecked
        {
            get { return TestFlag(TASKDIALOG_FLAGS.TDF_VERIFICATION_FLAG_CHECKED); }
            set { SetFlag(TASKDIALOG_FLAGS.TDF_VERIFICATION_FLAG_CHECKED, value); }
        }

        /// <summary>
        /// Indicates that a Progress Bar is to be displayed.
        /// </summary>
        public bool ShowProgressBar
        {
            get { return TestFlag(TASKDIALOG_FLAGS.TDF_SHOW_PROGRESS_BAR); }
            set { SetFlag(TASKDIALOG_FLAGS.TDF_SHOW_PROGRESS_BAR, value); }
        }

        /// <summary>
        /// Indicates that an Marquee Progress Bar is to be displayed.
        /// </summary>
        public bool ShowMarqueeProgressBar
        {
            get { return TestFlag(TASKDIALOG_FLAGS.TDF_SHOW_MARQUEE_PROGRESS_BAR); }
            set { SetFlag(TASKDIALOG_FLAGS.TDF_SHOW_MARQUEE_PROGRESS_BAR, value); }
        }

        /// <summary>
        /// Indicates that the task dialog's TimerTicked event is to be
        /// fired approximately every 200 milliseconds.
        /// </summary>
        public bool CallbackTimer
        {
            get { return TestFlag(TASKDIALOG_FLAGS.TDF_CALLBACK_TIMER); }
            set { SetFlag(TASKDIALOG_FLAGS.TDF_CALLBACK_TIMER, value); }
        }

        /// <summary>
        /// Indicates that the task dialog is positioned (centered) relative
        /// to the parent window. If the flag is not supplied (or no parent
        /// is specified), the task dialog is positioned (centered) relative
        /// to the monitor.
        /// </summary>
        public bool PositionRelativeToWindow
        {
            get { return TestFlag(TASKDIALOG_FLAGS.TDF_POSITION_RELATIVE_TO_WINDOW); }
            set { SetFlag(TASKDIALOG_FLAGS.TDF_POSITION_RELATIVE_TO_WINDOW, value); }
        }

        /// <summary>
        /// Indicates that text is displayed reading right to left.
        /// </summary>
        public bool RtlLayout
        {
            get { return TestFlag(TASKDIALOG_FLAGS.TDF_RTL_LAYOUT); }
            set { SetFlag(TASKDIALOG_FLAGS.TDF_RTL_LAYOUT, value); }
        }

        /// <summary>
        /// Indicates that no default item will be selected.
        /// </summary>
        public bool NoDefaultRadioButton
        {
            get { return TestFlag(TASKDIALOG_FLAGS.TDF_NO_DEFAULT_RADIO_BUTTON); }
            set { SetFlag(TASKDIALOG_FLAGS.TDF_NO_DEFAULT_RADIO_BUTTON, value); }
        }

        /// <summary>
        /// Indicates that the task dialog can be minimized.
        /// </summary>
        public bool CanBeMinimized
        {
            get { return TestFlag(TASKDIALOG_FLAGS.TDF_CAN_BE_MINIMIZED); }
            set { SetFlag(TASKDIALOG_FLAGS.TDF_CAN_BE_MINIMIZED, value); }
        }

        /*
        /// <summary>
        /// Version 6.00 and Windows 7. Indicates that the width of the task dialog
        /// is determined by the width of its content area. This flag is ignored if
        /// cxWidth is not set to 0.
        ///
        /// Note: This flag may not be supported in future versions of Comctl32.dll.
        /// Also, this flag is not defined in commctrl.h. To use it, add the
        /// following definition to the source files of your application:
        /// #define TDIF_SIZE_TO_CONTENT 0x1000000.
        /// </summary>
        public bool SizeToContent
        {
            get { return TestFlag(TASKDIALOG_FLAGS.TDIF_SIZE_TO_CONTENT); }
            set { SetFlag(TASKDIALOG_FLAGS.TDIF_SIZE_TO_CONTENT, value); }
        }
        */

        private bool TestFlag(TASKDIALOG_FLAGS flag)
        {
            return (config.dwFlags & flag) == flag;
        }

        private void SetFlag(TASKDIALOG_FLAGS flag, bool value)
        {
            EnsureNotShowing();

            if (value)
                config.dwFlags |= flag;
            else
                config.dwFlags &= ~flag;
        }

        #endregion

        #region Public properties

        /// <summary>
        /// String to be used for the task dialog title.
        /// </summary>
        public string WindowTitle
        {
            get { return config.pszWindowTitle; }
            set
            {
                EnsureNotShowing();
                config.pszWindowTitle = value;
            }
        }

        /// <summary>
        /// Icon that is to be displayed in the task dialog, or null for none.
        /// </summary>
        public TaskDialogIcon MainIcon
        {
            get { return mainIcon; }
            set
            {
                EnsureNotShowing();
                mainIcon = value;
            }
        }

        /// <summary>
        /// String to be used for the main instruction.
        /// </summary>
        public string MainInstruction
        {
            get { return config.pszMainInstruction; }
            set
            {
                EnsureNotShowing();
                config.pszMainInstruction = value;
            }
        }

        /// <summary>
        /// String to be used for the dialog's primary content.
        /// If the EnableHyperlinks property is set to true, then this string may
        /// contain hyperlinks in the form:
        /// &lt;A HREF="executablestring"&gt;Hyperlink Text&lt;/A&gt;.
        /// </summary>
        public string Content
        {
            get { return config.pszContent; }
            set
            {
                EnsureNotShowing();
                config.pszContent = value;
            }
        }

        /// <summary>
        /// Specifies the push buttons displayed in the task dialog. If no common
        /// buttons are specified and no custom buttons are specified using the
        /// Buttons property, the task dialog will contain the OK button by default.
        /// </summary>
        public TaskDialogCommonButtons CommonButtons
        {
            get { return (TaskDialogCommonButtons)config.dwCommonButtons; }
            set
            {
                EnsureNotShowing();
                config.dwCommonButtons = (TASKDIALOG_COMMON_BUTTON_FLAGS)value;
            }
        }

        /// <summary>
        /// The custom buttons that are to be displayed in the dialog.
        /// </summary>
        public ICollection<TaskDialogButton> Buttons
        {
            get { return buttons; }
        }

        /// <summary>
        /// The button ID of the button that is selected by default.
        /// </summary>
        public int DefaultButtonId
        {
            get { return config.nDefaultButton; }
            set
            {
                EnsureNotShowing();
                config.nDefaultButton = value;
            }
        }

        /// <summary>
        /// The radio buttons that are to be displayed in the dialog.
        /// </summary>
        public ICollection<TaskDialogButton> RadioButtons
        {
            get { return radioButtons; }
        }

        /// <summary>
        /// The button ID of the radio button that is selected by default. If
        /// this value does not correspond to a button ID, the first button in
        /// the array is selected by default.
        /// </summary>
        public int DefaultRadioButtonId
        {
            get { return config.nDefaultRadioButton; }
            set
            {
                EnsureNotShowing();
                config.nDefaultRadioButton = value;
            }
        }

        /// <summary>
        /// String to be used to label the verification checkbox.
        /// If this parameter is null, the verification checkbox is not displayed
        /// in the task dialog.
        /// </summary>
        public string VerificationText
        {
            get { return config.pszVerificationText; }
            set
            {
                EnsureNotShowing();
                config.pszVerificationText = value;
            }
        }

        /// <summary>
        /// String to be used for displaying additional information.
        /// The additional information is displayed either immediately below the
        /// content or below the footer text depending on whether the
        /// ExpandFooterArea property is true.
        /// If the EnableHyperlinks property is set to true, then this string may
        /// contain hyperlinks in the form:
        /// &lt;A HREF="executablestring"&gt;Hyperlink Text&lt;/A&gt;.
        /// </summary>
        public string ExpandedInformation
        {
            get { return config.pszExpandedInformation; }
            set
            {
                EnsureNotShowing();
                config.pszExpandedInformation = value;
            }
        }

        /// <summary>
        /// String to be used to label the button for collapsing the expandable
        /// information. If this member is null and CollapsedControlText is
        /// specified, then the CollapsedControlText value will be used for this
        /// property as well.
        /// </summary>
        public string ExpandedControlText
        {
            get { return config.pszExpandedControlText; }
            set
            {
                EnsureNotShowing();
                config.pszExpandedControlText = value;
            }
        }

        /// <summary>
        /// String to be used to label the button for expanding the expandable
        /// information. If this member is null and ExpandedControlText is
        /// specified, then the ExpandedControlText value will be used for this
        /// property as well.
        /// </summary>
        public string CollapsedControlText
        {
            get { return config.pszCollapsedControlText; }
            set
            {
                EnsureNotShowing();
                config.pszCollapsedControlText = value;
            }
        }

        /// <summary>
        /// Icon that is to be displayed in the footer of the task dialog, or
        /// null for none.
        /// </summary>
        public TaskDialogIcon FooterIcon
        {
            get { return footerIcon; }
            set
            {
                EnsureNotShowing();
                footerIcon = value;
            }
        }

        /// <summary>
        /// String to be used in the footer area of the task dialog.
        /// If the EnableHyperlinks property is set to true, then this string may
        /// contain hyperlinks in the form:
        /// &lt;A HREF="executablestring"&gt;Hyperlink Text&lt;/A&gt;.
        /// </summary>
        public string Footer
        {
            get { return config.pszFooter; }
            set
            {
                EnsureNotShowing();
                config.pszFooter = value;
            }
        }

        /// <summary>
        /// The width of the task dialog's client area. If 0, the task dialog manager will calculate the ideal width.
        /// </summary>
        public int Width
        {
            get { return (int)config.cxWidth; }
            set
            {
                EnsureNotShowing();
                config.cxWidth = (uint)value;
            }
        }

        #endregion

        public void Show(IWin32Window dialogOwner, out int buttonResult, out int radioButtonResult, out bool verificationFlagResult)
        {
            config.hwndParent = (dialogOwner != null) ? dialogOwner.Handle : IntPtr.Zero;

            TaskDialogIcon mainIconToUse = mainIcon ?? TaskDialogIcon.None;
            config.hMainIcon = mainIconToUse.Value;
            SetFlag(TASKDIALOG_FLAGS.TDF_USE_HICON_MAIN, mainIconToUse.UseIcon);

            TaskDialogIcon footerIconToUse = footerIcon ?? TaskDialogIcon.None;
            config.hFooterIcon = footerIconToUse.Value;
            SetFlag(TASKDIALOG_FLAGS.TDF_USE_HICON_FOOTER, footerIconToUse.UseIcon);

            using (StructArrayMarshaller<TASKDIALOG_BUTTON> samButtons = MarshalButtons(buttons))
            {
                using (StructArrayMarshaller<TASKDIALOG_BUTTON> samRadioButtons = MarshalButtons(radioButtons))
                {
                    config.cButtons = (uint)buttons.Count;
                    config.pButtons = samButtons.Buffer;

                    config.cRadioButtons = (uint)radioButtons.Count;
                    config.pRadioButtons = samRadioButtons.Buffer;

                    int hr = TaskDialogNative.TaskDialogIndirect(
                        ref config,
                        out buttonResult,
                        out radioButtonResult,
                        out verificationFlagResult);

                    Marshal.ThrowExceptionForHR(hr);
                }
            }
        }

        private IntPtr Callback(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam, IntPtr lpRefData)
        {
            TaskDialogDriver driver = new TaskDialogDriver(hwnd);
            switch (msg)
            {
                case (uint)TASKDIALOG_NOTIFICATIONS.TDN_BUTTON_CLICKED:
                    if (ButtonClicked != null)
                        ButtonClicked(this, new TaskDialogButtonEventArgs(driver, wParam.ToInt32()));
                    break;
                case (uint)TASKDIALOG_NOTIFICATIONS.TDN_CREATED:
                    if (Created != null)
                        Created(this, new TaskDialogEventArgs(driver));
                    break;
                case (uint)TASKDIALOG_NOTIFICATIONS.TDN_DESTROYED:
                    if (Destroyed != null)
                        Destroyed(this, new TaskDialogEventArgs(driver));
                    break;
                case (uint)TASKDIALOG_NOTIFICATIONS.TDN_DIALOG_CONSTRUCTED:
                    if (DialogConstructed != null)
                        DialogConstructed(this, new TaskDialogEventArgs(driver));
                    break;
                case (uint)TASKDIALOG_NOTIFICATIONS.TDN_EXPANDO_BUTTON_CLICKED:
                    if (ExpandoButtonClicked != null)
                        ExpandoButtonClicked(this, new TaskDialogExpandoEventArgs(driver, wParam != IntPtr.Zero));
                    break;
                case (uint)TASKDIALOG_NOTIFICATIONS.TDN_HELP:
                    if (Help != null)
                        Help(this, new TaskDialogEventArgs(driver));
                    break;
                case (uint)TASKDIALOG_NOTIFICATIONS.TDN_HYPERLINK_CLICKED:
                    if (HyperlinkClicked != null)
                        HyperlinkClicked(this, new TaskDialogHyperlinkEventArgs(driver, Marshal.PtrToStringUni(lParam)));
                    break;
                /*
            case (uint)TASKDIALOG_NOTIFICATIONS.TDN_NAVIGATED:
                if (Navigated != null)
                    Navigated(this, new TaskDialogEventArgs(driver));
                break;
                 */
                case (uint)TASKDIALOG_NOTIFICATIONS.TDN_RADIO_BUTTON_CLICKED:
                    if (RadioButtonClicked != null)
                        RadioButtonClicked(this, new TaskDialogButtonEventArgs(driver, wParam.ToInt32()));
                    break;
                case (uint)TASKDIALOG_NOTIFICATIONS.TDN_TIMER:
                    if (TimerTicked != null)
                    {
                        TaskDialogTimerEventArgs args = new TaskDialogTimerEventArgs(driver, wParam.ToInt32());
                        TimerTicked(this, args);
                        if (args.ResetTickCount)
                            return new IntPtr(1);
                    }
                    break;
                case (uint)TASKDIALOG_NOTIFICATIONS.TDN_VERIFICATION_CLICKED:
                    if (VerificationClicked != null)
                        VerificationClicked(this, new TaskDialogVerificationEventArgs(driver, wParam.ToInt32() == 1));
                    break;
            }
            return IntPtr.Zero;
        }

        private StructArrayMarshaller<TASKDIALOG_BUTTON> MarshalButtons(List<TaskDialogButton> buttons)
        {
            if (buttons.Count == 0)
                return new StructArrayMarshaller<TASKDIALOG_BUTTON>();

            TASKDIALOG_BUTTON[] results = new TASKDIALOG_BUTTON[buttons.Count];
            for (int i = 0; i < results.Length; i++)
            {
                results[i] = new TASKDIALOG_BUTTON();
                results[i].nButtonID = buttons[i].Id;
                results[i].pszButtonText = buttons[i].Text;
            }

            return new StructArrayMarshaller<TASKDIALOG_BUTTON>(results);
        }

        private void EnsureNotShowing()
        {
        }
    }

    public class TaskDialogButton
    {
        private int id;
        private string text;

        public TaskDialogButton(int id, string text)
        {
            this.id = id;
            this.text = text;
        }

        public int Id { get { return id; } }
        public string Text { get { return text; } }
    }

    public class TaskDialogIcon : IDisposable
    {
        private IntPtr id;
        private IntPtr hicon;

        public TaskDialogIcon(Bitmap bitmap)
        {
            id = IntPtr.Zero;
            hicon = bitmap.GetHicon();
        }

        private TaskDialogIcon(int id)
        {
            this.id = new IntPtr(id);
        }

        public void Dispose()
        {
            IntPtr temp = Interlocked.CompareExchange(ref hicon, IntPtr.Zero, hicon);
            if (temp != IntPtr.Zero)
            {
                DestroyIcon(temp);
            }
        }

        public bool UseIcon { get { return hicon != IntPtr.Zero; } }
        public IntPtr Value { get { return UseIcon ? hicon : id; } }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private extern static bool DestroyIcon(IntPtr handle);

        public static TaskDialogIcon None { get { return new TaskDialogIcon(0); } }
        public static TaskDialogIcon Warning { get { return new TaskDialogIcon(-1 & 0xFFFF); } }
        public static TaskDialogIcon Error { get { return new TaskDialogIcon(-2 & 0xFFFF); } }
        public static TaskDialogIcon Information { get { return new TaskDialogIcon(-3 & 0xFFFF); } }
        public static TaskDialogIcon SecurityWarning { get { return new TaskDialogIcon(0xFFFF - 5); } }
        public static TaskDialogIcon SecurityError { get { return new TaskDialogIcon(0xFFFF - 6); } }
        public static TaskDialogIcon SecuritySuccess { get { return new TaskDialogIcon(0xFFFF - 7); } }
        public static TaskDialogIcon SecurityShield { get { return new TaskDialogIcon(0xFFFF - 3); } }
        public static TaskDialogIcon SecurityShieldBlue { get { return new TaskDialogIcon(0xFFFF - 4); } }
        public static TaskDialogIcon SecurityShieldGray { get { return new TaskDialogIcon(0xFFFF - 8); } }
    }

    public struct TaskDialogDriver
    {
        private readonly IntPtr hwnd;

        public TaskDialogDriver(IntPtr hwnd)
        {
            this.hwnd = hwnd;
        }

        public void ClickButton(int buttonId)
        {
            SendMessage(TASKDIALOG_MESSAGES.TDM_CLICK_BUTTON, buttonId, 0);
        }

        public void ClickRadioButton(int buttonId)
        {
            SendMessage(TASKDIALOG_MESSAGES.TDM_CLICK_RADIO_BUTTON, buttonId, 0);
        }

        public void ClickVerification(bool @checked, bool focused)
        {
            SendMessage(TASKDIALOG_MESSAGES.TDM_CLICK_VERIFICATION,
                @checked ? 1 : 0,
                focused ? 1 : 0);
        }

        public void EnableButton(int buttonId, bool enabled)
        {
            SendMessage(TASKDIALOG_MESSAGES.TDM_ENABLE_BUTTON,
                buttonId,
                enabled ? 1 : 0);
        }

        public void EnableRadioButton(int buttonId, bool enabled)
        {
            SendMessage(TASKDIALOG_MESSAGES.TDM_ENABLE_RADIO_BUTTON,
                buttonId,
                enabled ? 1 : 0);
        }

        //public void NavigatePage(TaskDialog newTaskDialog)
        //{
        //}

        public void SetButtonElevationRequiredState(int buttonId, bool elevationRequired)
        {
            SendMessage(TASKDIALOG_MESSAGES.TDM_SET_BUTTON_ELEVATION_REQUIRED_STATE,
                buttonId,
                elevationRequired ? 1 : 0);
        }

        public void SetContent(string text, bool relayout)
        {
            SetText(TASKDIALOG_ELEMENTS.TDE_CONTENT, text, relayout);
        }

        public void SetExpandedInformation(string text, bool relayout)
        {
            SetText(TASKDIALOG_ELEMENTS.TDE_EXPANDED_INFORMATION, text, relayout);
        }

        public void SetFooter(string text, bool relayout)
        {
            SetText(TASKDIALOG_ELEMENTS.TDE_FOOTER, text, relayout);
        }

        public void SetMainInstruction(string text, bool relayout)
        {
            SetText(TASKDIALOG_ELEMENTS.TDE_MAIN_INSTRUCTION, text, relayout);
        }

        private void SetText(TASKDIALOG_ELEMENTS tde, string text, bool relayout)
        {
            using (StringMarshaller sm = new StringMarshaller(text))
            {
                SendMessage(
                    relayout ? TASKDIALOG_MESSAGES.TDM_SET_ELEMENT_TEXT : TASKDIALOG_MESSAGES.TDM_UPDATE_ELEMENT_TEXT,
                    (int)tde,
                    sm.Value.ToInt32());
            }
        }

        public void SetMarqueeProgressBar(bool marquee)
        {
            SendMessage(TASKDIALOG_MESSAGES.TDM_SET_MARQUEE_PROGRESS_BAR,
                marquee ? 1 : 0,
                0);
        }

        public void SetProgressBarValue(int min, int max, int pos)
        {
            SendMessage(TASKDIALOG_MESSAGES.TDM_SET_PROGRESS_BAR_RANGE,
                0, (min & 0xffff) | ((max & 0xffff) << 16));
            SendMessage(TASKDIALOG_MESSAGES.TDM_SET_PROGRESS_BAR_POS,
                pos, 0);
        }

        public void SetProgressBarState(TaskDialogProgressBarState state)
        {
            SendMessage(TASKDIALOG_MESSAGES.TDM_SET_PROGRESS_BAR_STATE,
                (int)state, 0);
        }

        public void UpdateMainIcon(TaskDialogIcon icon)
        {
            SendMessage(TASKDIALOG_MESSAGES.TDM_UPDATE_ICON,
                (int)TASKDIALOG_ICON_ELEMENTS.TDIE_ICON_MAIN,
                icon.Value.ToInt32());
        }

        public void UpdateFooterIcon(TaskDialogIcon icon)
        {
            SendMessage(TASKDIALOG_MESSAGES.TDM_UPDATE_ICON,
                (int)TASKDIALOG_ICON_ELEMENTS.TDIE_ICON_FOOTER,
                icon.Value.ToInt32());
        }

        private void SendMessage(TASKDIALOG_MESSAGES msg, int wParam, int lParam)
        {
            SendMessage(hwnd, msg, wParam, lParam);
        }

        [DllImport("User32.dll")]
        private static extern int SendMessage(IntPtr hwnd, TASKDIALOG_MESSAGES msg, int wParam, int lParam);
    }

    public delegate void TaskDialogEventHandler(object sender, TaskDialogEventArgs args);
    public class TaskDialogEventArgs : EventArgs
    {
        public readonly TaskDialogDriver TaskDialog;

        internal TaskDialogEventArgs(TaskDialogDriver driver)
        {
            TaskDialog = driver;
        }
    }

    public delegate void TaskDialogButtonEventHandler(object sender, TaskDialogButtonEventArgs args);
    public class TaskDialogButtonEventArgs : TaskDialogEventArgs
    {
        public readonly int ButtonId;

        internal TaskDialogButtonEventArgs(TaskDialogDriver driver, int buttonId)
            : base(driver)
        {
            ButtonId = buttonId;
        }
    }

    public delegate void TaskDialogExpandoEventHandler(object sender, TaskDialogExpandoEventArgs args);
    public class TaskDialogExpandoEventArgs : TaskDialogEventArgs
    {
        public readonly bool Expanded;

        internal TaskDialogExpandoEventArgs(TaskDialogDriver driver, bool expanded)
            : base(driver)
        {
            Expanded = expanded;
        }
    }

    public delegate void TaskDialogHyperlinkEventHandler(object sender, TaskDialogHyperlinkEventArgs args);
    public class TaskDialogHyperlinkEventArgs : TaskDialogEventArgs
    {
        public readonly string Url;

        internal TaskDialogHyperlinkEventArgs(TaskDialogDriver driver, string url)
            : base(driver)
        {
            Url = url;
        }
    }

    public delegate void TaskDialogTimerEventHandler(object sender, TaskDialogTimerEventArgs args);
    public class TaskDialogTimerEventArgs : TaskDialogEventArgs
    {
        public readonly int TickCount;
        public bool ResetTickCount;

        internal TaskDialogTimerEventArgs(TaskDialogDriver driver, int tickCount)
            : base(driver)
        {
            TickCount = tickCount;
            ResetTickCount = false;
        }
    }

    public delegate void TaskDialogVerificationEventHandler(object sender, TaskDialogVerificationEventArgs args);
    public class TaskDialogVerificationEventArgs : TaskDialogEventArgs
    {
        public bool VerificationChecked;

        internal TaskDialogVerificationEventArgs(TaskDialogDriver driver, bool verificationChecked)
            : base(driver)
        {
            VerificationChecked = verificationChecked;
        }
    }

    public enum TaskDialogProgressBarState
    {
        Normal,
        Pause,
        Error
    }

    [Flags]
    public enum TaskDialogCommonButtons
    {
        None = 0,
        OK = 0x0001,
        Yes = 0x0002,
        No = 0x0004,
        Cancel = 0x0008,
        Retry = 0x0010,
        Close = 0x0020
    }

    public static class TaskDialogCommonButtonIds
    {
        public const int OK = 1;
        public const int Cancel = 2;
        public const int Retry = 4;
        public const int Yes = 6;
        public const int No = 7;
        public const int Close = 8;
    }
}
