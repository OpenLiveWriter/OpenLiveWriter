// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Diagnostics;
using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.Interop.Windows;
using OpenLiveWriter.Localization.Bidi;
using OpenLiveWriter.PostEditor.PostHtmlEditing;
using OpenLiveWriter.CoreServices.Marketization;

namespace OpenLiveWriter.PostEditor.ImageInsertion
{
    /// <summary>
    /// The extensible OpenFileDialog
    /// </summary>
    public class InsertImageDialog : IDisposable
    {
        // The maximum number of characters permitted in a path
        private const int _MAX_PATH = 260;
        private static UIntPtr _UINT_MAX_PATH_PLUS_NULL = new UIntPtr(_MAX_PATH + 1);

        // The "control ID" of various windows inside the OpenFileDialog
        private const int _CONTENT_PANEL_ID = 0x0461;
        private const int _HELP_BUTTON_ID = 0x040e;
        private const int _OPEN_BUTTON_ID = 0x0001;
        private const int _CANCEL_BUTTON_ID = 0x0002;
        private const int _READY_ONLY_ID = 0x0410;
        private const int _BLANK_PANEL_ID = 0x0460;
        private const int _RESIZING_CORNER_ID = -0x0001;
        private const int _FILE_NAME_ID = 0x047c;
        private const int _FILE_TYPE_ID = 0x0470;
        private const int _PLACES_ID = 0x04a0;

        private const int TABS_HEIGHT = 50;
        private const int LINK_OPTIONS_HEIGHT = 55;
        private const int BUTTONS_HEIGHT = 48;

        private int _extraWindowHeight;
        private int _extraWindowWidth;

        //windows we communicate with a lot
        private IntPtr _hWndParent;
        private IntPtr _hWndContent;
        private IntPtr _hWndFileName;

        //top tab control
        private LightweightControlContainerControl mainTabControl;
        private TabLightweightControl tabs;

        //image source info for non local file images
        private InsertImageSource[] imageSources = new InsertImageSource[] { new WebImageSource() };
        private InsertImageSource activeImageSource = null;

        //panel for other image sources
        private Panel _panelImage;

        //panel insert/cancel buttons
        private Panel _buttonPanel;
        private Button _cancelButton;

        private bool IsRTL = false;

        private STATE state;
        private List<string> _chosenImage = new List<string>();
        private string _errorFileName;
        private bool _insertFile = false;

        private CommandListener listener;
        private ThumbnailReadinessListener thumbListener;

        private TabbingHookProc tabKeyboardHook;

        private enum STATE
        {
            FILE, WEB
        }

        //keeps a list of all the controls we find on the form...?
        private ArrayList controlsToShowHideOnTabSwitch = new ArrayList();

        // unmanaged memory buffers to hold the file name (with and without full path) and dir
        private IntPtr _fileNameBuffer;
        private IntPtr _fileTitleBuffer;
        private IntPtr _directoryBuffer;

        // the OPENFILENAME structure, used to control the appearance and behaviour of the OpenFileDialog
        private OpenFileName _ofn;

        // unmanaged memory buffer that holds the Win32 dialog template
        private IntPtr _ipTemplate;

        /// <summary>
        /// Sets up the data structures necessary to display the OpenFileDialog
        /// </summary>
        public InsertImageDialog(string defaultExtension, string directoryName, IWin32Window parent)
        {
            //for thumbnail later
            thumbListener = new ThumbnailReadinessListener(parent.Handle, this);

            // Need two buffers in unmanaged memory to hold the filename
            // Note: the multiplication by 2 is to allow for Unicode (16-bit) characters
            _fileNameBuffer = Marshal.AllocCoTaskMem(2 * _MAX_PATH);
            _fileTitleBuffer = Marshal.AllocCoTaskMem(2 * _MAX_PATH);
            _directoryBuffer = Marshal.AllocCoTaskMem(2 * _MAX_PATH);

            if (directoryName == String.Empty)
            {
                directoryName = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            }
            // Zero these two buffers
            byte[] zeroBuffer = new byte[2 * (_MAX_PATH + 1)];
            for (int i = 0; i < 2 * (_MAX_PATH + 1); i++) zeroBuffer[i] = 0;
            Marshal.Copy(zeroBuffer, 0, _fileNameBuffer, 2 * _MAX_PATH);
            Marshal.Copy(zeroBuffer, 0, _fileTitleBuffer, 2 * _MAX_PATH);
            Marshal.Copy(zeroBuffer, 0, _directoryBuffer, 2 * _MAX_PATH);

            // copy initial directory name into unmanaged memory buffer
            byte[] directoryBytes = Encoding.Unicode.GetBytes(directoryName);
            Marshal.Copy(directoryBytes, 0, _directoryBuffer, directoryBytes.Length);

            // Create an in-memory Win32 dialog template; this will be a "child" window inside the FileOpenDialog
            // We have no use for this child window, except that its presence allows us to capture events when
            // the user interacts with the FileOpenDialog
            _ipTemplate = BuildDialogTemplate();

            // Populate the OPENFILENAME structure
            // The flags specified are the minimal set to get the appearance and behaviour we need
            _ofn.lStructSize = Marshal.SizeOf(_ofn);
            _ofn.lpstrFile = _fileNameBuffer;
            _ofn.nMaxFile = _MAX_PATH + 1;
            _ofn.lpstrDefExt = Marshal.StringToCoTaskMemUni(defaultExtension);
            _ofn.lpstrFileTitle = _fileTitleBuffer;
            _ofn.nMaxFileTitle = _MAX_PATH + 1;
            _ofn.lpstrInitialDir = _directoryBuffer;
            _ofn.lpstrFilter = Marshal.StringToCoTaskMemUni(String.Format(CultureInfo.InvariantCulture, "{0} \0*.gif;*.jpg;*.jpeg;*.png\0{1} \0*.*\0\0", Res.Get(StringId.ImagesFilterString), Res.Get(StringId.AllFilesFilterString)));
            _ofn.Flags = OpenFileNameFlags.HideReadOnly | OpenFileNameFlags.EnableHook |
                         OpenFileNameFlags.EnableTemplateHandle | OpenFileNameFlags.EnableSizing
                         | OpenFileNameFlags.Explorer
                         | OpenFileNameFlags.FileMustExist | OpenFileNameFlags.PathMustExist;

            if (MarketizationOptions.IsFeatureEnabled(MarketizationOptions.Feature.AllowMultiSelectImage))
            {
                _ofn.Flags |= OpenFileNameFlags.AllowMultiSelect;
            }

            _ofn.hInstance = _ipTemplate;
            string title = String.Format(CultureInfo.InvariantCulture, "{0}\0\0", Res.Get(StringId.InsertPicture));
            _ofn.lpstrTitle = Marshal.StringToCoTaskMemUni(title);
            _ofn.lpfnHook = new OfnHookProc(MyHookProc);
            _ofn.hwndOwner = parent.Handle;
        }

        /// <summary>
        /// The finalizer will release the unmanaged memory, if I should forget to call Dispose
        /// </summary>
        ~InsertImageDialog()
        {
            Dispose(false);
        }

        /// <summary>
        /// Display the OpenFileDialog and allow user interaction
        /// Show the dialog w/ appropriate hooks to change size and default view
        /// </summary>
        /// <returns>true if the user clicked OK, false if they clicked cancel (or close)</returns>
        public bool Show()
        {
            //blocking call
            User32.GetOpenFileName(ref _ofn);
            // Remove help keyboard hook
            if (tabKeyboardHook.IsInstalled)
                tabKeyboardHook.Remove();
            return _insertFile;
        }

        /// <summary>
        /// Builds an in-memory Win32 dialog template.  See documentation for DLGTEMPLATE.
        /// </summary>
        /// <returns>a pointer to an unmanaged memory buffer containing the dialog template</returns>
        private IntPtr BuildDialogTemplate()
        {
            // We must place this child window inside the standard FileOpenDialog in order to get any
            // notifications sent to our hook procedure.  Also, this child window must contain at least
            // one control.  We make no direct use of the child window, or its control.

            // Set up the contents of the DLGTEMPLATE
            DlgTemplate template = new DlgTemplate();

            // Allocate some unmanaged memory for the template structure, and copy it in
            IntPtr ipTemplate = Marshal.AllocCoTaskMem(Marshal.SizeOf(template));
            Marshal.StructureToPtr(template, ipTemplate, true);
            return ipTemplate;
        }

        /// <summary>
        /// The hook procedure for window messages generated by the FileOpenDialog
        /// </summary>
        /// <param name="hWnd">the handle of the window at which this message is targeted</param>
        /// <param name="msg">the message identifier</param>
        /// <param name="wParam">message-specific parameter data</param>
        /// <param name="lParam">mess-specific parameter data</param>
        /// <returns></returns>
        public IntPtr MyHookProc(IntPtr hWnd, UInt32 msg, Int32 wParam, Int32 lParam)
        {
            try
            {
                if (hWnd == IntPtr.Zero)
                    return IntPtr.Zero;

                switch (msg)
                {
                    // We're not interested in every possible message; just return a NULL for those we don't care about
                    default:
                        {
                            return IntPtr.Zero;
                        }
                    // WM_INITDIALOG - at this point the OpenFileDialog exists, so we pull the user-supplied control
                    // into the FileOpenDialog now, using the SetParent API.
                    case WM.INITDIALOG:
                        {

                            _hWndParent = User32.GetParent(hWnd);

                            //setting a bool for whether the OS is RTL (not the installed language of WLW)
                            IsRTL = (User32.GetWindowLong(_hWndParent, User32.GWL_EXSTYLE) & User32.WS_EX_LAYOUTRTL) > 0;

                            //account for large title bar, borders for adjusting control locations
                            TITLEBARINFO titleBarInfo = new TITLEBARINFO();
                            titleBarInfo.cbSize = (uint)Marshal.SizeOf(titleBarInfo);
                            if (!User32.GetTitleBarInfo(_hWndParent, ref titleBarInfo))
                            {
                                throw new Win32Exception(Marshal.GetLastWin32Error());
                            }

                            WINDOWINFO info = new WINDOWINFO();
                            info.cbSize = (uint)Marshal.SizeOf(info);
                            if (!User32.GetWindowInfo(_hWndParent, ref info))
                            {
                                throw new Win32Exception(Marshal.GetLastWin32Error());
                            }

                            _extraWindowHeight = (titleBarInfo.rcTitleBar.bottom - titleBarInfo.rcTitleBar.top) + 2 * (int)info.cyWindowBorders;
                            _extraWindowWidth = 2 * (int)info.cxWindowBorders;

                            Rectangle rcClient = new Rectangle(0, 0, 0, 0);
                            // Get client rectangle of dialog
                            RECT rcTemp = new RECT();
                            User32.GetWindowRect(_hWndParent, ref rcTemp);
                            rcClient.X = rcTemp.left;
                            rcClient.Y = rcTemp.top;
                            rcClient.Width = rcTemp.Width;
                            rcClient.Height = rcTemp.Height + TABS_HEIGHT + BUTTONS_HEIGHT;
                            //make the dialog box bigger
                            User32.MoveWindow(_hWndParent, rcClient.Left, rcClient.Top, rcClient.Width, rcClient.Height, true);
                            //move all the controls down
                            AdjustControlLocations();
                            //top tab control
                            mainTabControl = new LightweightControlContainerControl();
                            User32.SetParent(mainTabControl.Handle, _hWndParent);
                            mainTabControl.Location = new Point(0, 0);
                            mainTabControl.Anchor = AnchorStyles.Left | AnchorStyles.Right;
                            mainTabControl.Size = new Size(rcClient.Width, TABS_HEIGHT);

                            tabs = new TabLightweightControl();
                            tabs.ColorizeBorder = false;
                            tabs.VirtualBounds = new Rectangle(0, 0, rcClient.Width, TABS_HEIGHT);
                            tabs.LightweightControlContainerControl = mainTabControl;
                            tabs.DrawSideAndBottomTabPageBorders = false;

                            InsertImageTabControl tabFromFile = new InsertImageTabControl();
                            tabFromFile.TabText = Res.Get(StringId.InsertImageInsertFromFile);
                            tabFromFile.TabBitmap = ResourceHelper.LoadAssemblyResourceBitmap("ImageInsertion.Images.TabInsertFromFile.png");
                            tabFromFile.BackColor = SystemColors.Control;
                            User32.SetParent(tabFromFile.Handle, _hWndParent);
                            tabs.SetTab(0, tabFromFile);

                            mainTabControl.BackColor = tabFromFile.ApplicationStyle.InactiveTabTopColor;

                            //now, add tabs for the other controls
                            int i = 1;
                            foreach (InsertImageSource imageSource in imageSources)
                            {
                                InsertImageTabControl tab = new InsertImageTabControl();
                                tab.TabText = imageSource.TabName;
                                tab.TabBitmap = imageSource.TabBitmap;
                                tab.BackColor = SystemColors.Control;
                                tabs.SetTab(i++, tab);
                            }

                            tabs.SelectedTabNumberChanged += new EventHandler(tabs_SelectedTabNumberChanged);

                            //set the keyboard hook for tab switching
                            tabKeyboardHook = new TabbingHookProc(tabs);
                            tabKeyboardHook.Install(_hWndParent);

                            //add other image source panels
                            _panelImage = new Panel();
                            _panelImage.Location = new Point(0, mainTabControl.Size.Height);
                            _panelImage.BorderStyle = BorderStyle.None;
                            _panelImage.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;
                            _panelImage.Size = new Size(rcClient.Width, rcClient.Height - TABS_HEIGHT - BUTTONS_HEIGHT - _extraWindowHeight);
                            _panelImage.Visible = false;

                            User32.SetParent(_panelImage.Handle, _hWndParent);

                            //initialize the other sources
                            foreach (InsertImageSource source in imageSources)
                            {
                                source.Init(_panelImage.Width, _panelImage.Height);
                                Control c = source.ImageSelectionControls;
                                DisplayHelper.Scale(c);
                                foreach (Control childControl in c.Controls)
                                    DisplayHelper.Scale(childControl);
                            }
                            DisplayHelper.Scale(_panelImage);

                            //special cancel button
                            _buttonPanel = new Panel();
                            _buttonPanel.Location = new Point(rcClient.Width - (int)(0.5 * _extraWindowWidth) - _buttonPanel.Width, _panelImage.Bounds.Bottom);
                            _buttonPanel.Size = new Size(75, 23);
                            _buttonPanel.BorderStyle = BorderStyle.None;

                            _cancelButton = new Button();
                            _cancelButton.TextAlign = ContentAlignment.MiddleCenter;
                            if (BidiHelper.IsRightToLeft)
                                _cancelButton.RightToLeft = RightToLeft.Yes;
                            _cancelButton.Text = Res.Get(StringId.CancelButton);
                            _cancelButton.FlatStyle = FlatStyle.System;
                            _cancelButton.Location = new Point(0, 0);
                            _cancelButton.Size = new Size(75, 23);
                            _cancelButton.Click += new EventHandler(_cancelButton_Click);

                            _buttonPanel.Controls.Add(_cancelButton);

                            User32.SetParent(_buttonPanel.Handle, _hWndParent);

                            int origWidth = _cancelButton.Width;
                            string tmp = _cancelButton.Text;
                            _cancelButton.Text = Res.Get(StringId.InsertImageButton);
                            int newWidth = Math.Max(origWidth, DisplayHelper.MeasureButton(_cancelButton));
                            _cancelButton.Text = tmp;
                            newWidth = Math.Max(newWidth, DisplayHelper.MeasureButton(_cancelButton));

                            _buttonPanel.Width = _cancelButton.Width = newWidth;
                            int deltaX = newWidth - origWidth;
                            _buttonPanel.Left -= deltaX;

                            //fixing up button text and tab order
                            IntPtr hWndOpenButton = User32.GetDlgItem(_hWndParent, _OPEN_BUTTON_ID);
                            User32.SetWindowText(hWndOpenButton, Res.Get(StringId.InsertImageButton));

                            mainTabControl.InitFocusManager();
                            mainTabControl.AddFocusableControls(tabs.GetAccessibleControls());
                            foreach (InsertImageSource tabPage in imageSources)
                                mainTabControl.AddFocusableControl(tabPage.ImageSelectionControls);

                            state = STATE.FILE;

                            return IntPtr.Zero;
                        }

                    case WM.SIZE:
                        {
                            ManipulatePanels();
                            return IntPtr.Zero;
                        }
                    // WM_NOTIFY - we're only interested in the CDN_SELCHANGE notification message:
                    // we grab the currently-selected filename and copy it into the buffer
                    case WM.NOTIFY:
                        {
                            IntPtr ipNotify = new IntPtr(lParam);
                            OfNotify ofNot = (OfNotify)Marshal.PtrToStructure(ipNotify, typeof(OfNotify));
                            Int16 code = (short)ofNot.hdr.code;
                            if (code == CommonDlgNotification.SelChange)
                            {
                                UpdateChosenImage(false);
                                //CheckOptions(false);
                            }
                            else if (code == CommonDlgNotification.InitDone)
                            {
                                listener = new CommandListener(_hWndParent, this, (int)User32.GetDlgCtrlID(_cancelButton.Handle));
                            }
                            else if (code == CommonDlgNotification.FileOk)
                            {
                                // update the image path (need to do this if the user selected
                                // a file by simply typing in the filepath text box)
                                UpdateChosenImage(true);

                                // ok to insert
                                _insertFile = true;
                            }
                            else if ((code == CommonDlgNotification.FolderChange) && (state == STATE.WEB))
                            {
                                // If the user hits the OK button while there is no valid selection
                                // within the File panel, the file dialog sends a CommonDlgNotification.FolderChange
                                // We use this combined with other relevant state to trigger the closing
                                // of the Image dialog
                                HitOpen();
                            }
                            return IntPtr.Zero;
                        }
                }
            }
            catch (Exception ex)
            {
                UnexpectedErrorMessage.Show(ex);
                return new IntPtr(1);
            }
            finally
            {
                GC.KeepAlive(this);
            }
        }

        private void UpdateChosenImage(bool checkFileName)
        {
            _chosenImage.Clear();

            // get the newly-selected image
            IntPtr pImageBuffer = Marshal.AllocCoTaskMem(_MAX_PATH * 2);

            User32.SendMessage(_hWndParent, CommonDlgMessage.GetFilePath, _UINT_MAX_PATH_PLUS_NULL, pImageBuffer);
            string image = Marshal.PtrToStringUni(pImageBuffer);
            Marshal.FreeCoTaskMem(pImageBuffer);

            // copy the string into the path buffer
            UnicodeEncoding ue = new UnicodeEncoding();
            byte[] pathBytes = ue.GetBytes(image);
            Marshal.Copy(pathBytes, 0, _fileNameBuffer, pathBytes.Length);
            _chosenImage.Add(image);

            if (checkFileName)
            {
                // first see if there is a url entered in the text box
                IntPtr hWndFileName = User32.GetDlgItem(_hWndParent, _FILE_NAME_ID);
                StringBuilder windowTextBuffer = new StringBuilder(User32.GetWindowTextLength(hWndFileName) + 1);
                User32.GetWindowText(hWndFileName, windowTextBuffer, windowTextBuffer.Capacity);
                string fileName = windowTextBuffer.ToString().Trim();
                if (UrlHelper.IsUrl(fileName))
                {
                    _chosenImage.Clear();
                    _chosenImage.Add(fileName);
                }
                else
                {
                    IntPtr pPathBuffer = Marshal.AllocCoTaskMem(_MAX_PATH * 2);

                    User32.SendMessage(_hWndParent, CommonDlgMessage.GetFolderPath, _UINT_MAX_PATH_PLUS_NULL, pPathBuffer);
                    string path = Marshal.PtrToStringUni(pImageBuffer);
                    Marshal.FreeCoTaskMem(pPathBuffer);
                    //grrr bug 441665--user might have altered image but the path still has the old image
                    if (fileName != String.Empty)
                    {
                        //case: new path entered in file name
                        try
                        {
                            FileAttributes attr = File.GetAttributes(fileName);
                            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                            {
                                return;
                            }
                        }
                        catch
                        {
                        }

                        //case: new path + filename entered in file name
                        //case: new image entered in file name

                        _chosenImage.Clear();
                        List<string> files = ParseMultipleFileName(fileName);

                        foreach (string file in files)
                        {
                            fileName = Path.Combine(path, file);
                            DirectoryInfo pathInfo = new DirectoryInfo(Path.GetDirectoryName(fileName));

                            if (File.Exists(fileName))
                            {
                                _chosenImage.Add(fileName);
                            }
                            else
                            {
                                try
                                {
                                    FileInfo[] matchingFiles =
                                        FindFilenameWithExtension(pathInfo, Path.GetFileName(fileName));

                                    if (matchingFiles.Length == 0)
                                    {
                                        _errorFileName = file;
                                        _chosenImage.Clear();
                                        Trace.WriteLine("File(s) not found: " + fileName);
                                        return;
                                    }
                                    if (matchingFiles.Length == 1)
                                    {
                                        _chosenImage.Add(matchingFiles[0].FullName);
                                    }
                                    else
                                    {
                                        return;
                                        // rename the selected image in the open dialog, then press enter to accept
                                        //_chosenImage = String.Empty;
                                    }
                                }
                                catch
                                {
                                    _errorFileName = file;
                                    _chosenImage.Clear();
                                    Trace.WriteLine("File(s) not found: " + fileName);
                                    return;
                                }
                            }
                        }
                    }
                }
            }
            GC.KeepAlive(this);
        }

        private static List<string> ParseMultipleFileName(string fileName)
        {
            List<string> files = new List<string>();

            if (!fileName.Contains("\""))
            {
                files.Add(fileName);
                return files;
            }

            bool insideQuote = false;
            StringBuilder currentFileName = new StringBuilder();
            for (int i = 0; i < fileName.Length; i++)
            {
                if (fileName[i] == '\"')
                {
                    if (insideQuote)
                    {
                        string filePath = currentFileName.ToString().Trim();
                        if (!string.IsNullOrEmpty(filePath))
                            files.Add(filePath);
                        currentFileName.Remove(0, currentFileName.Length);
                    }
                    insideQuote = !insideQuote;
                    continue;
                }

                if (insideQuote)
                {
                    currentFileName.Append(fileName[i]);
                }
                else
                {
                    Trace.Assert(char.IsWhiteSpace(fileName[i]), "asdfasd");
                }

            }

            return files;
        }

        private static FileInfo[] FindFilenameWithExtension(DirectoryInfo dir, string filenameWithoutExtension)
        {
            ArrayList results = new ArrayList();
            string fullPath = Path.Combine(dir.FullName, filenameWithoutExtension);
            foreach (FileInfo fi in dir.GetFiles(filenameWithoutExtension + ".*"))
            {
                if (IsSamePlusExtension(fullPath, fi.FullName))
                    results.Add(fi);
            }
            return (FileInfo[])results.ToArray(typeof(FileInfo));
        }

        private static bool IsSamePlusExtension(string filename, string filenamePlusExtension)
        {
            Debug.Assert(Path.IsPathRooted(filename), "Filename must be absolute path");
            Debug.Assert(Path.IsPathRooted(filenamePlusExtension), "FilenamePlusExtension must be absolute path");

            if (filenamePlusExtension.Length <= filename.Length + 1)
                return false;
            if (!filenamePlusExtension.ToLower(CultureInfo.InvariantCulture).StartsWith(filename.ToLower(CultureInfo.InvariantCulture)))
                return false;
            if (filenamePlusExtension[filename.Length] != '.')
                return false;
            if (filenamePlusExtension.Substring(filename.Length + 1).IndexOf('.') >= 0)
                return false;
            return true;
        }

        private void tabs_SelectedTabNumberChanged(object sender, EventArgs e)
        {
            int tabChosen = ((TabLightweightControl)sender).SelectedTabNumber;
            if (tabChosen == 0)
            {
                SetFromFileActive();
            }
            else
            {
                SetActiveControl(imageSources[tabChosen - 1]);
            }
        }

        public bool NotifyOfEnterKey()
        {
            return HitOpen();
        }

        public bool OfferUpEnterKey(int cmdId)
        {
            //argh, buttons aren't getting the enter key
            //see if the active image source wants to handle the enter
            return (activeImageSource != null && activeImageSource.HandleEnter(cmdId));
        }

        public bool HitClose()
        {
            _cancelButton.PerformClick();
            return true;
        }

        private void ManipulatePanels()
        {
            RECT rcTemp = new RECT();
            User32.GetWindowRect(_hWndParent, ref rcTemp);
            int newWidth = rcTemp.Width - _extraWindowWidth;
            int newHeight = rcTemp.Height;

            mainTabControl.Size = new Size(newWidth, TABS_HEIGHT);
            tabs.VirtualBounds = new Rectangle(0, 5, newWidth, TABS_HEIGHT - 5);
            _panelImage.Location = new Point(0, TABS_HEIGHT);
            _panelImage.Width = newWidth;
            _panelImage.Height = newHeight - TABS_HEIGHT - BUTTONS_HEIGHT - _extraWindowHeight + 11;

            int panelLeft = newWidth - (int)(0.5 * _extraWindowWidth) - _buttonPanel.Width;
            if (IsRTL)
                panelLeft -= 8;
            _buttonPanel.Location = new Point(panelLeft, _panelImage.Bounds.Bottom);

            IntPtr hWndOpenButton = User32.GetDlgItem(_hWndParent, _OPEN_BUTTON_ID);

            int left = _buttonPanel.Left - _buttonPanel.Width - 8;

            User32.MoveWindow(
                hWndOpenButton, left,
                _panelImage.Bounds.Bottom,
                _buttonPanel.Width, 23, true);

            _cancelButton.Invalidate();

            if (activeImageSource != null)
            {
                activeImageSource.Repaint(new Size(newWidth, _panelImage.Height));
            }
            GC.KeepAlive(this);
        }

        public void SetActiveControl(InsertImageSource source)
        {
            //if coming from file tab, hide all that stuff
            if (state != STATE.WEB)
            {
                foreach (IntPtr thing in controlsToShowHideOnTabSwitch)
                {
                    User32.ShowWindow(thing, SW.HIDE);
                }
            }

            //set the new source info
            activeImageSource = source;
            _panelImage.Controls.Clear();
            _panelImage.Controls.Add(activeImageSource.ImageSelectionControls);
            ManipulatePanels();
            _panelImage.Visible = true;
            activeImageSource.TabSelected();

            state = STATE.WEB;
        }

        private void SetFromFileActive()
        {
            //clear out all web image tab info
            _panelImage.Visible = false;
            activeImageSource = null;

            //show file image info
            foreach (IntPtr thing in controlsToShowHideOnTabSwitch)
            {
                User32.ShowWindow(thing, SW.SHOW);
            }
            ForceLargeThumbnail(_hWndContent);
            User32.SendMessage(_hWndFileName, WM.SETFOCUS, new UIntPtr(1), IntPtr.Zero);
            state = STATE.FILE;
            GC.KeepAlive(this);
        }

        private void _cancelButton_Click(object sender, EventArgs e)
        {
            User32.PostMessage(_hWndParent, WM.CLOSE, UIntPtr.Zero, IntPtr.Zero);
            GC.KeepAlive(this);
        }

        //returns whether the message has been processed by us or goes back to OS
        private bool HitOpen()
        {
            _insertFile = false;
            if (state == STATE.WEB)
            {
                if (activeImageSource.ValidateSelection())
                {
                    activeImageSource.Selected = true;
                    CloseDialogAndInsertFile();
                }
                return true;
            }
            else if (state == STATE.FILE)
            {
                UpdateChosenImage(true);
                if (_chosenImage.Count == 0)
                {
                    if (string.IsNullOrEmpty(_errorFileName))
                    {
                        DisplayMessage.Show(MessageId.NoImageSelected);
                    }
                    else
                    {
                        DisplayMessage.Show(MessageId.FileNotFound, _errorFileName);
                    }

                    return true;
                }
                else if (VerifyExistAndNoShortcut(_chosenImage))
                {
                    CloseDialogAndInsertFile();
                    return true;
                }
                else if (_chosenImage.Count == 1 && UrlHelper.IsUrl(_chosenImage[0]))
                {
                    CloseDialogAndInsertFile();
                    return true;
                }
            }
            return false;
        }

        private bool VerifyExistAndNoShortcut(List<string> chosenImage)
        {
            foreach (string file in chosenImage)
            {
                if (!File.Exists(file) || new FileInfo(file).Extension.ToLower(CultureInfo.InvariantCulture) == ".lnk")
                    return false;
            }
            return true;
        }

        private void CloseDialogAndInsertFile()
        {
            _insertFile = true;
            User32.PostMessage(_hWndParent, WM.CLOSE, UIntPtr.Zero, IntPtr.Zero);
            GC.KeepAlive(this);
        }

        /// <summary>
        /// Layout the content of the OpenFileDialog, according to the overall size of the dialog
        /// </summary>
        private void AdjustControlLocations()
        {
            DialogHelper.EnumerateChildWindowsDelegate delProcess = new DialogHelper.EnumerateChildWindowsDelegate(MoveWindow);
            DialogHelper.EnumerateChildWindows(_hWndParent, delProcess, false, controlsToShowHideOnTabSwitch);
            //expand the file name and type windows
            ExpandFileTextBox(User32.GetDlgItem(_hWndParent, _FILE_NAME_ID));
            ExpandFileTextBox(User32.GetDlgItem(_hWndParent, _FILE_TYPE_ID));
            GC.KeepAlive(this);
        }

        public bool MoveWindow(IntPtr hWndControl, object a)
        {
            Rectangle rcContent = new Rectangle(0, 0, 0, 0);

            // The content window may not be present when the dialog first appears
            // also, the corner resizer moves itself, so don't move it again
            if (hWndControl != IntPtr.Zero && hWndControl != User32.GetDlgItem(_hWndParent, _RESIZING_CORNER_ID))
            {
                // Find the dimensions of the control
                RECT rc = new RECT();

                User32.GetWindowRect(hWndControl, ref rc);
                POINT pnt = new POINT();
                pnt.x = rc.left;
                pnt.y = rc.top;
                User32.MapWindowPoints((IntPtr)null, _hWndParent, ref pnt, 1);
                rcContent.X = pnt.x;
                rcContent.Width = rc.Width;
                rcContent.Y = pnt.y + TABS_HEIGHT; //move it down
                rcContent.Height = rc.Height;
                if (IsRTL)
                {
                    RECT rcParent = new RECT();
                    User32.GetWindowRect(_hWndParent, ref rcParent);
                    rcContent.X = rcParent.left + rcParent.Width - rc.right - _extraWindowWidth;
                }
                User32.MoveWindow(hWndControl, rcContent.Left, rcContent.Top, rcContent.Width, rcContent.Height, true);
                //building up a list of the controls
                controlsToShowHideOnTabSwitch.Add(hWndControl);
            }
            GC.KeepAlive(this);
            return true;
        }

        public void ExpandFileTextBox(IntPtr hWndControl)
        {
            Rectangle rcContent = new Rectangle(0, 0, 0, 0);
            if (hWndControl != IntPtr.Zero)
            {
                RECT rc = new RECT();
                User32.GetWindowRect(hWndControl, ref rc);
                POINT pnt = new POINT();
                pnt.x = rc.left;
                pnt.y = rc.top;
                User32.MapWindowPoints((IntPtr)null, _hWndParent, ref pnt, 1);

                rcContent.X = pnt.x;
                rcContent.Y = pnt.y;
                rcContent.Height = rc.Height;
                rcContent.Width = 549 - rcContent.X;
                if (IsRTL)
                {
                    RECT rcParent = new RECT();
                    User32.GetWindowRect(_hWndParent, ref rcParent);
                    rcContent.X = rcParent.left + rcParent.Width - rc.right - (int)(0.5 * _extraWindowWidth);
                    rcContent.Width = 545 - rcContent.X;
                }
                User32.MoveWindow(hWndControl, rcContent.Left, rcContent.Top, rcContent.Width, rcContent.Height, true);
            }
            GC.KeepAlive(this);
        }

        #region public properties

        /// <summary>
        /// returns the path currently selected by the user inside the OpenFileDialog
        /// </summary>
        public string[] SelectedPaths
        {
            get
            {
                if (state == STATE.FILE)
                {
                    return _chosenImage.ToArray();
                }
                else
                {
                    return new string[1] { activeImageSource.FileName };
                }
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Free any unmanaged memory used by this instance of OpenFileDialog
        /// </summary>
        /// <param name="disposing">true if called by Dispose, false otherwise</param>
        public void Dispose(bool disposing)
        {
            if (disposing)
            {
                GC.SuppressFinalize(this);
            }

            Marshal.FreeCoTaskMem(_fileNameBuffer);
            Marshal.FreeCoTaskMem(_fileTitleBuffer);
            Marshal.FreeCoTaskMem(_directoryBuffer);
            Marshal.FreeCoTaskMem(_ipTemplate);
        }

        #endregion

        private void ForceLargeThumbnail(IntPtr hContentWindow)
        {
            // This hack is based on the knowledge that within the standard open file dialog
            // there is a control w/ class "SHELLDLL_DefView" which implements standard
            // shell right-pane viewing. Documentation for the existence of this window can
            // be found at: http://msdn.microsoft.com/msdnmag/issues/04/03/CQA/

            // This control implements a set of WM_COMMAND messages which correspond to
            // menu items on its view menu. By using Spy++ you can reverse engineer this
            // enumeration (this is also described in the article referenced above).
            // NOTE: there is at least one report from a developer of this technique not working
            // (see comments at http://www.thecodeproject.com/cs/miscctrl/FileDialogExtender.asp).
            // it is very likely that this technique is fragile across OS version and/or
            // installed shell customizations.

            UIntPtr viewType = FCIDM_SHVIEW.LARGEICON;

            User32.SendMessage(hContentWindow, WM_COMMAND, viewType, IntPtr.Zero);
            GC.KeepAlive(this);
        }

        public void CompleteInitialization()
        {
            // get handle of the special control
            _hWndContent = User32.GetDlgItem(_hWndParent, _CONTENT_PANEL_ID);
            _hWndFileName = User32.GetDlgItem(_hWndParent, _FILE_NAME_ID);

            //all controls are present, do any necessary fixing and forcing
            ForceLargeThumbnail(_hWndContent);

            //fix up the control list for appropriate show/hide
            IntPtr hWndContentBlank = User32.GetDlgItem(_hWndParent, _BLANK_PANEL_ID);
            IntPtr hWndHelpButton = User32.GetDlgItem(_hWndParent, _HELP_BUTTON_ID);
            IntPtr hWndCornerResizer = User32.GetDlgItem(_hWndParent, _RESIZING_CORNER_ID);
            IntPtr hWndReadyOnly = User32.GetDlgItem(_hWndParent, _READY_ONLY_ID);
            controlsToShowHideOnTabSwitch.Remove(hWndContentBlank);
            controlsToShowHideOnTabSwitch.Remove(hWndHelpButton);
            controlsToShowHideOnTabSwitch.Remove(hWndCornerResizer);
            controlsToShowHideOnTabSwitch.Remove(hWndReadyOnly);
            controlsToShowHideOnTabSwitch.Add(_hWndContent);

            //hide the cancel button, we will use our own
            IntPtr hWndOpenButton = User32.GetDlgItem(_hWndParent, _OPEN_BUTTON_ID);
            IntPtr hWndCancelButton = User32.GetDlgItem(_hWndParent, _CANCEL_BUTTON_ID);
            User32.ShowWindow(hWndCancelButton, SW.HIDE);
            controlsToShowHideOnTabSwitch.Remove(hWndOpenButton);
            controlsToShowHideOnTabSwitch.Remove(hWndCancelButton);

            GC.KeepAlive(this);
        }

        // wm command message
        private const uint WM_COMMAND = 0x0111;

        // reverse-engineered command codes for SHELLDLL_DefView
        private class FCIDM_SHVIEW
        {
            public static readonly UIntPtr LARGEICON = new UIntPtr(0x7029);
            public static readonly UIntPtr SMALLICON = new UIntPtr(0x702A);
            public static readonly UIntPtr MEDIUMICON = new UIntPtr(0x7028);
            public static readonly UIntPtr LIST = new UIntPtr(0x702B);
            public static readonly UIntPtr REPORT = new UIntPtr(0x702C);
            public static readonly UIntPtr THUMBNAIL = new UIntPtr(0x702D);
            public static readonly UIntPtr TILE = new UIntPtr(0x702E);
        }

        private class CommandListener : NativeWindow
        {
            private InsertImageDialog _insertImageDialog;
            private int _cancelButtonId;

            public CommandListener(IntPtr hImageDialog, InsertImageDialog insertImageDialog, int cancelButtonId)
            {
                AssignHandle(hImageDialog);
                _insertImageDialog = insertImageDialog;
                _cancelButtonId = cancelButtonId & 0xffff;
            }

            protected override void WndProc(ref Message m)
            {
                if (m.Msg == WM_COMMAND)
                {
                    int id = MessageHelper.LOWORDToInt32(m.WParam);
                    if (id == _OPEN_BUTTON_ID)
                    {
                        //check for insertable file
                        if (_insertImageDialog.NotifyOfEnterKey())
                        {
                            m.Result = IntPtr.Zero;
                            return;
                        }
                    }
                    else if (id == _cancelButtonId)
                    {
                        if (_insertImageDialog.HitClose())
                        {
                            m.Result = IntPtr.Zero;
                            return;
                        }
                    }
                    else if (_insertImageDialog.OfferUpEnterKey(id))
                    {
                        m.Result = IntPtr.Zero;
                        return;
                    }
                }
                try
                {
                    base.WndProc(ref m);
                }
                catch (Exception e)
                {
                    Trace.Fail("insert image dialog WndProc Exception", e.ToString());
                }
            }

            private const int WM_COMMAND = 0x0111;
        }

        private class ThumbnailReadinessListener : NativeWindow
        {
            private InsertImageDialog _insertImageDialog;

            public ThumbnailReadinessListener(IntPtr hImageDialog, InsertImageDialog insertImageDialog)
            {
                AssignHandle(hImageDialog);
                _insertImageDialog = insertImageDialog;
            }

            protected override void WndProc(ref Message m)
            {
                if (m.Msg == WM_ENTERIDLE && m.WParam == MSGF_DIALOGBOX)
                {
                    if (_dialogHandle == IntPtr.Zero)
                    {
                        _dialogHandle = m.LParam;
                        _insertImageDialog.CompleteInitialization();
                    }
                }
                base.WndProc(ref m);
            }

            private IntPtr _dialogHandle = IntPtr.Zero;

            private const int WM_ENTERIDLE = 0x0121;
            private static readonly IntPtr MSGF_DIALOGBOX = IntPtr.Zero;
        }
    }

}
