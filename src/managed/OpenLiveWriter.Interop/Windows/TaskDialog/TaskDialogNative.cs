// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace OpenLiveWriter.Interop.Windows.TaskDialog
{
    internal static class TaskDialogNative
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="pTaskConfig">Pointer to a TASKDIALOGCONFIG structure that contains information used to display the task dialog.</param>
        /// <param name="pnButton">Address of a variable that receives either:
        ///   - one of the button IDs specified in the pButtons member of the pTaskConfig parameter
        ///   - one of the following values:
        ///         Value       Description
        ///         0           Function call failed. Refer to return value for more information.
        ///         IDCANCEL    Cancel button was selected, Alt-F4 was pressed, Escape was pressed or the user clicked on the close window button.
        ///         IDNO        No button was selected.
        ///         IDOK        OK button was selected.
        ///         IDRETRY     Retry button was selected.
        ///         IDYES       Yes button was selected.
        ///
        /// If this parameter is NULL, no value is returned.</param>
        /// <param name="pnRadioButton">Address of a variable that receives one of the button IDs specified in the pRadioButtons member of the pTaskConfig parameter. If this parameter is NULL, no value is returned.</param>
        /// <param name="pfVerificationFlagChecked">Address of a variable that receives one of the following values:
        ///         Value       Description
        ///         TRUE        The verification checkbox was checked when the dialog was dismissed.
        ///         FALSE       The verification checkbox was not checked when the dialog was dismissed.
        ///
        /// If this parameter is NULL, the verification checkbox is disabled.</param>
        /// <returns>Returns one of the following values.
        ///         S_OK            The operation completed successfully.
        ///         E_OUTOFMEMORY   There is insufficient memory to complete the operation.
        ///         E_INVALIDARG    One or more arguments are not valid.
        ///         E_FAIL          The operation failed.
        /// </returns>
        [DllImport("comctl32.dll", CharSet = CharSet.Unicode)]
        public static extern int TaskDialogIndirect(
            [In] ref TASKDIALOGCONFIG pTaskConfig,
            [Out] out int pnButton,
            [Out] out int pnRadioButton,
            [Out] out bool pfVerificationFlagChecked
            );
    }

    /*
    typedef struct _TASKDIALOGCONFIG {
        UINT cbSize;
        HWND hwndParent;
        HINSTANCE hInstance;
        TASKDIALOG_FLAGS dwFlags;
        TASKDIALOG_COMMON_BUTTON_FLAGS dwCommonButtons;
        PCWSTR pszWindowTitle;
        union {
            HICON hMainIcon;
            PCWSTR pszMainIcon;
        };
        PCWSTR pszMainInstruction;
        PCWSTR pszContent;
        UINT cButtons;
        const TASKDIALOG_BUTTON *pButtons;
        int nDefaultButton;
        UINT cRadioButtons;
        const TASKDIALOG_BUTTON *pRadioButtons;
        int nDefaultRadioButton;
        PCWSTR pszVerificationText;
        PCWSTR pszExpandedInformation;
        PCWSTR pszExpandedControlText;
        PCWSTR pszCollapsedControlText;
        union {
            HICON hFooterIcon;
            PCWSTR pszFooterIcon;
        };
        PCWSTR pszFooter;
        PFTASKDIALOGCALLBACK pfCallback;
        LONG_PTR lpCallbackData;
        UINT cxWidth;
    } TASKDIALOGCONFIG;
     */
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal unsafe struct TASKDIALOGCONFIG
    {
        public int cbSize;
        public IntPtr hwndParent;
        public IntPtr hInstance;
        public TASKDIALOG_FLAGS dwFlags;
        public TASKDIALOG_COMMON_BUTTON_FLAGS dwCommonButtons;
        public string pszWindowTitle;
        public IntPtr hMainIcon;
        public string pszMainInstruction;
        public string pszContent;
        public uint cButtons;
        public IntPtr pButtons; // TASKDIALOG_BUTTON*
        public int nDefaultButton;
        public uint cRadioButtons;
        public IntPtr pRadioButtons; // TASKDIALOG_BUTTON*
        public int nDefaultRadioButton;
        public string pszVerificationText;
        public string pszExpandedInformation;
        public string pszExpandedControlText;
        public string pszCollapsedControlText;
        public IntPtr hFooterIcon;
        public string pszFooter;
        public PFTASKDIALOGCALLBACK pfCallback;
        public IntPtr lpCallbackData;
        public uint cxWidth;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct TASKDIALOG_BUTTON
    {
        public int nButtonID;
        public string pszButtonText;
    }

    [Flags]
    internal enum TASKDIALOG_FLAGS
    {
        TDF_ENABLE_HYPERLINKS = 0x0001,
        TDF_USE_HICON_MAIN = 0x0002,
        TDF_USE_HICON_FOOTER = 0x0004,
        TDF_ALLOW_DIALOG_CANCELLATION = 0x0008,
        TDF_USE_COMMAND_LINKS = 0x0010,
        TDF_USE_COMMAND_LINKS_NO_ICON = 0x0020,
        TDF_EXPAND_FOOTER_AREA = 0x0040,
        TDF_EXPANDED_BY_DEFAULT = 0x0080,
        TDF_VERIFICATION_FLAG_CHECKED = 0x0100,
        TDF_SHOW_PROGRESS_BAR = 0x0200,
        TDF_SHOW_MARQUEE_PROGRESS_BAR = 0x0400,
        TDF_CALLBACK_TIMER = 0x0800,
        TDF_POSITION_RELATIVE_TO_WINDOW = 0x1000,
        TDF_RTL_LAYOUT = 0x2000,
        TDF_NO_DEFAULT_RADIO_BUTTON = 0x4000,
        TDF_CAN_BE_MINIMIZED = 0x8000
    }

    [Flags]
    internal enum TASKDIALOG_COMMON_BUTTON_FLAGS
    {
        TDCBF_OK_BUTTON = 0x0001, // selected control return value IDOK
        TDCBF_YES_BUTTON = 0x0002, // selected control return value IDYES
        TDCBF_NO_BUTTON = 0x0004, // selected control return value IDNO
        TDCBF_CANCEL_BUTTON = 0x0008, // selected control return value IDCANCEL
        TDCBF_RETRY_BUTTON = 0x0010, // selected control return value IDRETRY
        TDCBF_CLOSE_BUTTON = 0x0020  // selected control return value IDCLOSE
    }

    internal enum TASKDIALOG_NOTIFICATIONS : uint
    {
        TDN_CREATED = 0,
        TDN_NAVIGATED = 1,
        TDN_BUTTON_CLICKED = 2,            // wParam = Button ID
        TDN_HYPERLINK_CLICKED = 3,            // lParam = (LPCWSTR)pszHREF
        TDN_TIMER = 4,            // wParam = Milliseconds since dialog created or timer reset
        TDN_DESTROYED = 5,
        TDN_RADIO_BUTTON_CLICKED = 6,            // wParam = Radio Button ID
        TDN_DIALOG_CONSTRUCTED = 7,
        TDN_VERIFICATION_CLICKED = 8,             // wParam = 1 if checkbox checked, 0 if not, lParam is unused and always 0
        TDN_HELP = 9,
        TDN_EXPANDO_BUTTON_CLICKED = 10            // wParam = 0 (dialog is now collapsed), wParam != 0 (dialog is now expanded)
    }

    internal enum TASKDIALOG_MESSAGES
    {
        TDM_NAVIGATE_PAGE = 0x400 + 101,
        TDM_CLICK_BUTTON = 0x400 + 102, // wParam = Button ID
        TDM_SET_MARQUEE_PROGRESS_BAR = 0x400 + 103, // wParam = 0 (nonMarque) wParam != 0 (Marquee)
        TDM_SET_PROGRESS_BAR_STATE = 0x400 + 104, // wParam = new progress state
        TDM_SET_PROGRESS_BAR_RANGE = 0x400 + 105, // lParam = MAKELPARAM(nMinRange, nMaxRange)
        TDM_SET_PROGRESS_BAR_POS = 0x400 + 106, // wParam = new position
        TDM_SET_PROGRESS_BAR_MARQUEE = 0x400 + 107, // wParam = 0 (stop marquee), wParam != 0 (start marquee), lparam = speed (milliseconds between repaints)
        TDM_SET_ELEMENT_TEXT = 0x400 + 108, // wParam = element (TASKDIALOG_ELEMENTS), lParam = new element text (LPCWSTR)
        TDM_CLICK_RADIO_BUTTON = 0x400 + 110, // wParam = Radio Button ID
        TDM_ENABLE_BUTTON = 0x400 + 111, // lParam = 0 (disable), lParam != 0 (enable), wParam = Button ID
        TDM_ENABLE_RADIO_BUTTON = 0x400 + 112, // lParam = 0 (disable), lParam != 0 (enable), wParam = Radio Button ID
        TDM_CLICK_VERIFICATION = 0x400 + 113, // wParam = 0 (unchecked), 1 (checked), lParam = 1 (set key focus)
        TDM_UPDATE_ELEMENT_TEXT = 0x400 + 114, // wParam = element (TASKDIALOG_ELEMENTS), lParam = new element text (LPCWSTR)
        TDM_SET_BUTTON_ELEVATION_REQUIRED_STATE = 0x400 + 115, // wParam = Button ID, lParam = 0 (elevation not required), lParam != 0 (elevation required)
        TDM_UPDATE_ICON = 0x400 + 116  // wParam = icon element (TASKDIALOG_ICON_ELEMENTS), lParam = new icon (hIcon if TDF_USE_HICON_* was set, PCWSTR otherwise)
    }

    internal enum TASKDIALOG_ELEMENTS
    {
        TDE_CONTENT,
        TDE_EXPANDED_INFORMATION,
        TDE_FOOTER,
        TDE_MAIN_INSTRUCTION
    }

    internal enum TASKDIALOG_ICON_ELEMENTS
    {
        TDIE_ICON_MAIN,
        TDIE_ICON_FOOTER
    }

    internal delegate IntPtr PFTASKDIALOGCALLBACK(
        IntPtr hwnd,
        uint msg,
        IntPtr wParam,
        IntPtr lParam,
        IntPtr lpRefData);
}
