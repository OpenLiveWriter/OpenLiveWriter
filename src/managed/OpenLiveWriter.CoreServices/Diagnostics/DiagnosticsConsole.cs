// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices.Threading;

namespace OpenLiveWriter.CoreServices.Diagnostics
{
    /// <summary>
    /// DiagnosticsConsole class for displaying Trace and Debug output.
    /// </summary>
    public class DiagnosticsConsole : BaseForm
    {
        #region Private Delegates

        #endregion Private Delegates

        #region Static & Constant Declarations

        /// <summary>
        /// Fail text.
        /// </summary>
        private const string StackTraceText = "(open to view stack trace...)";

        #endregion Static & Constant Declarations

        #region Designer Generated Code

        private IContainer components;
        private ColumnHeader columnHeaderSequence;
        private ColumnHeader columnHeaderTime;
        private Label labelLog;
        private ListView listViewLog;
        private ContextMenu contextMenuLog;
        private MenuItem menuItemCopy;
        private MenuItem menuItemClear;
        private MenuItem menuItemSelectAll;
        private Button buttonClear;
        private Button buttonSelectAll;
        private Button buttonCopy;
        private ColumnHeader columnHeaderText;
        private Button buttonReload;
        private Button buttonApplyFilter;
        private TextBox textBoxInclude;
        private Label labelInclude;
        private Label labelExclude;
        private TextBox textBoxExclude;
        private GroupBox groupBoxFilter;
        private MenuItem menuItemReload;
        private MenuItem menuItemSep1;
        private ColumnHeader columnHeaderCategory;
        private MenuItem menuItemOpen;
        private MenuItem menuItemSep2;
        private EmptyComponent emptyComponent;

        #endregion Designer Generated Code

        #region Private Member Variables

        /// <summary>
        /// The BufferingTraceListener where we get events from.
        /// </summary>
        private BufferingTraceListener bufferingTraceListener;

        /// <summary>
        /// The font we use to draw the log.
        /// </summary>
        private Font itemFont = new Font("Verdana", 8.25F, FontStyle.Regular, GraphicsUnit.Point, ((Byte)(0)));

        /// <summary>
        /// The first event index being displayed.
        /// </summary>
        private int firstEventIndex;

        /// <summary>
        /// The last event index being displayed.
        /// </summary>
        private int lastEventIndex;

        /// <summary>
        /// The include regex text used to create includeRegex.
        /// </summary>
        private string includeRegexText = String.Empty;

        /// <summary>
        /// The exclude regex text used to create excludeRegex.
        /// </summary>
        private string excludeRegexText = String.Empty;

        /// <summary>
        /// The include regex.
        /// </summary>
        private Regex includeRegex;

        /// <summary>
        /// The exclude regex.
        /// </summary>
        private Regex excludeRegex;

        /// <summary>
        /// The thread that this DiagnosticsConsole is running on.
        /// </summary>
        private Thread thread;

        #endregion Private Member Variables

        #region Class Initialization & Termination

        /// <summary>
        /// Initializes a new instance of the DiagnosticsConsole class.
        /// </summary>
        public DiagnosticsConsole()
        {
            RightToLeft = RightToLeft.No;
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the DiagnosticsConsole class.
        /// </summary>
        /// <param name="bufferingTraceListener">The BufferingTraceListener where we get events from.</param>
        /// <param name="title">The title.</param>
        public DiagnosticsConsole(BufferingTraceListener bufferingTraceListener, string title) : this()
        {
            Text = String.Format(CultureInfo.InvariantCulture, "{0} - {1} Diagnostics Console", ApplicationEnvironment.ProductNameQualified, title);
            this.bufferingTraceListener = bufferingTraceListener;
            this.bufferingTraceListener.Changed += new EventHandler(bufferingTraceListener_Changed);
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #endregion Class Initialization & Termination

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(DiagnosticsConsole));
            this.listViewLog = new System.Windows.Forms.ListView();
            this.columnHeaderSequence = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderTime = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderCategory = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderText = new System.Windows.Forms.ColumnHeader();
            this.contextMenuLog = new System.Windows.Forms.ContextMenu();
            this.menuItemOpen = new System.Windows.Forms.MenuItem();
            this.menuItemSep1 = new System.Windows.Forms.MenuItem();
            this.menuItemCopy = new System.Windows.Forms.MenuItem();
            this.menuItemClear = new System.Windows.Forms.MenuItem();
            this.menuItemReload = new System.Windows.Forms.MenuItem();
            this.menuItemSep2 = new System.Windows.Forms.MenuItem();
            this.menuItemSelectAll = new System.Windows.Forms.MenuItem();
            this.labelLog = new System.Windows.Forms.Label();
            this.buttonClear = new System.Windows.Forms.Button();
            this.buttonSelectAll = new System.Windows.Forms.Button();
            this.buttonCopy = new System.Windows.Forms.Button();
            this.buttonReload = new System.Windows.Forms.Button();
            this.groupBoxFilter = new System.Windows.Forms.GroupBox();
            this.labelExclude = new System.Windows.Forms.Label();
            this.textBoxExclude = new System.Windows.Forms.TextBox();
            this.labelInclude = new System.Windows.Forms.Label();
            this.textBoxInclude = new System.Windows.Forms.TextBox();
            this.buttonApplyFilter = new System.Windows.Forms.Button();
            this.emptyComponent = new OpenLiveWriter.CoreServices.Diagnostics.EmptyComponent(this.components);
            this.groupBoxFilter.SuspendLayout();
            this.SuspendLayout();
            //
            // listViewLog
            //
            this.listViewLog.Activation = System.Windows.Forms.ItemActivation.TwoClick;
            this.listViewLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewLog.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                                                                                          this.columnHeaderSequence,
                                                                                          this.columnHeaderTime,
                                                                                          this.columnHeaderCategory,
                                                                                          this.columnHeaderText});
            this.listViewLog.ContextMenu = this.contextMenuLog;
            this.listViewLog.FullRowSelect = true;
            this.listViewLog.GridLines = true;
            this.listViewLog.HideSelection = false;
            this.listViewLog.Location = new System.Drawing.Point(10, 112);
            this.listViewLog.Name = "listViewLog";
            this.listViewLog.Size = new System.Drawing.Size(612, 356);
            this.listViewLog.TabIndex = 2;
            this.listViewLog.View = System.Windows.Forms.View.Details;
            this.listViewLog.ItemActivate += new System.EventHandler(this.listViewLog_ItemActivate);
            //
            // columnHeaderSequence
            //
            this.columnHeaderSequence.Text = "#";
            this.columnHeaderSequence.Width = 45;
            //
            // columnHeaderTime
            //
            this.columnHeaderTime.Text = "Time";
            this.columnHeaderTime.Width = 105;
            //
            // columnHeaderCategory
            //
            this.columnHeaderCategory.Text = "Category";
            this.columnHeaderCategory.Width = 80;
            //
            // columnHeaderText
            //
            this.columnHeaderText.Text = "Text";
            this.columnHeaderText.Width = 1000;
            //
            // contextMenuLog
            //
            this.contextMenuLog.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                           this.menuItemOpen,
                                                                                           this.menuItemSep1,
                                                                                           this.menuItemCopy,
                                                                                           this.menuItemClear,
                                                                                           this.menuItemReload,
                                                                                           this.menuItemSep2,
                                                                                           this.menuItemSelectAll});
            //
            // menuItemOpen
            //
            this.menuItemOpen.Index = 0;
            this.menuItemOpen.Text = "Open...";
            this.menuItemOpen.Click += new System.EventHandler(this.menuItemOpen_Click);
            //
            // menuItemSep1
            //
            this.menuItemSep1.Index = 1;
            this.menuItemSep1.Text = "-";
            //
            // menuItemCopy
            //
            this.menuItemCopy.Index = 2;
            this.menuItemCopy.Shortcut = System.Windows.Forms.Shortcut.CtrlC;
            this.menuItemCopy.Text = "&Copy";
            this.menuItemCopy.Click += new System.EventHandler(this.menuItemCopy_Click);
            //
            // menuItemClear
            //
            this.menuItemClear.Index = 3;
            this.menuItemClear.Shortcut = System.Windows.Forms.Shortcut.CtrlW;
            this.menuItemClear.Text = "Clea&r";
            this.menuItemClear.Click += new System.EventHandler(this.menuItemClear_Click);
            //
            // menuItemReload
            //
            this.menuItemReload.Index = 4;
            this.menuItemReload.Shortcut = System.Windows.Forms.Shortcut.CtrlR;
            this.menuItemReload.Text = "&Reload";
            this.menuItemReload.Click += new System.EventHandler(this.menuItemReload_Click);
            //
            // menuItemSep2
            //
            this.menuItemSep2.Index = 5;
            this.menuItemSep2.Text = "-";
            //
            // menuItemSelectAll
            //
            this.menuItemSelectAll.Index = 6;
            this.menuItemSelectAll.Shortcut = System.Windows.Forms.Shortcut.CtrlA;
            this.menuItemSelectAll.Text = "Select &All";
            this.menuItemSelectAll.Click += new System.EventHandler(this.menuItemSelectAll_Click);
            //
            // labelLog
            //
            this.labelLog.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelLog.Location = new System.Drawing.Point(12, 92);
            this.labelLog.Name = "labelLog";
            this.labelLog.Size = new System.Drawing.Size(218, 18);
            this.labelLog.TabIndex = 1;
            this.labelLog.Text = "&Log of Trace and Debug events:";
            //
            // buttonClear
            //
            this.buttonClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonClear.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonClear.Location = new System.Drawing.Point(548, 478);
            this.buttonClear.Name = "buttonClear";
            this.buttonClear.TabIndex = 6;
            this.buttonClear.Text = "Clea&r";
            this.buttonClear.Click += new System.EventHandler(this.buttonClear_Click);
            //
            // buttonSelectAll
            //
            this.buttonSelectAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonSelectAll.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonSelectAll.Location = new System.Drawing.Point(10, 478);
            this.buttonSelectAll.Name = "buttonSelectAll";
            this.buttonSelectAll.TabIndex = 3;
            this.buttonSelectAll.Text = "Select &All";
            this.buttonSelectAll.Click += new System.EventHandler(this.buttonSelectAll_Click);
            //
            // buttonCopy
            //
            this.buttonCopy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonCopy.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonCopy.Location = new System.Drawing.Point(92, 478);
            this.buttonCopy.Name = "buttonCopy";
            this.buttonCopy.TabIndex = 4;
            this.buttonCopy.Text = "&Copy";
            this.buttonCopy.Click += new System.EventHandler(this.buttonCopy_Click);
            //
            // buttonReload
            //
            this.buttonReload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonReload.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonReload.Location = new System.Drawing.Point(466, 478);
            this.buttonReload.Name = "buttonReload";
            this.buttonReload.TabIndex = 5;
            this.buttonReload.Text = "&Reload";
            this.buttonReload.Click += new System.EventHandler(this.buttonReload_Click);
            //
            // groupBoxFilter
            //
            this.groupBoxFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxFilter.Controls.Add(this.labelExclude);
            this.groupBoxFilter.Controls.Add(this.textBoxExclude);
            this.groupBoxFilter.Controls.Add(this.labelInclude);
            this.groupBoxFilter.Controls.Add(this.textBoxInclude);
            this.groupBoxFilter.Controls.Add(this.buttonApplyFilter);
            this.groupBoxFilter.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBoxFilter.Location = new System.Drawing.Point(10, 10);
            this.groupBoxFilter.Name = "groupBoxFilter";
            this.groupBoxFilter.Size = new System.Drawing.Size(612, 70);
            this.groupBoxFilter.TabIndex = 0;
            this.groupBoxFilter.TabStop = false;
            this.groupBoxFilter.Text = "&Filter";
            this.groupBoxFilter.Layout += new System.Windows.Forms.LayoutEventHandler(this.groupBoxFiltering_Layout);
            //
            // labelExclude
            //
            this.labelExclude.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelExclude.Location = new System.Drawing.Point(258, 20);
            this.labelExclude.Name = "labelExclude";
            this.labelExclude.Size = new System.Drawing.Size(100, 18);
            this.labelExclude.TabIndex = 2;
            this.labelExclude.Text = "&Exclude (regex):";
            //
            // textBoxExclude
            //
            this.textBoxExclude.Location = new System.Drawing.Point(258, 38);
            this.textBoxExclude.Name = "textBoxExclude";
            this.textBoxExclude.Size = new System.Drawing.Size(240, 20);
            this.textBoxExclude.TabIndex = 3;
            this.textBoxExclude.Text = "";
            this.textBoxExclude.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxExclude_KeyPress);
            //
            // labelInclude
            //
            this.labelInclude.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelInclude.Location = new System.Drawing.Point(10, 20);
            this.labelInclude.Name = "labelInclude";
            this.labelInclude.Size = new System.Drawing.Size(100, 18);
            this.labelInclude.TabIndex = 0;
            this.labelInclude.Text = "&Include (regex):";
            //
            // textBoxInclude
            //
            this.textBoxInclude.Location = new System.Drawing.Point(10, 38);
            this.textBoxInclude.Name = "textBoxInclude";
            this.textBoxInclude.Size = new System.Drawing.Size(240, 20);
            this.textBoxInclude.TabIndex = 1;
            this.textBoxInclude.Text = "";
            this.textBoxInclude.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxInclude_KeyPress);
            //
            // buttonApplyFilter
            //
            this.buttonApplyFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonApplyFilter.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonApplyFilter.Location = new System.Drawing.Point(526, 36);
            this.buttonApplyFilter.Name = "buttonApplyFilter";
            this.buttonApplyFilter.TabIndex = 4;
            this.buttonApplyFilter.Text = "&Apply";
            this.buttonApplyFilter.Click += new System.EventHandler(this.buttonApplyFilter_Click);
            //
            // DiagnosticsConsole
            //
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
            this.ClientSize = new System.Drawing.Size(632, 510);
            this.Controls.Add(this.listViewLog);
            this.Controls.Add(this.groupBoxFilter);
            this.Controls.Add(this.buttonReload);
            this.Controls.Add(this.buttonCopy);
            this.Controls.Add(this.buttonSelectAll);
            this.Controls.Add(this.buttonClear);
            this.Controls.Add(this.labelLog);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "DiagnosticsConsole";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Diagnostics Console";
            this.groupBoxFilter.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Runs the dialog.
        /// </summary>
        public void Run()
        {
            //	If the thread has not been created, create and start it.
            if (thread == null)
            {
                thread = ThreadHelper.NewThread(new ThreadStart(ConsoleThreadStart), "DiagnosticsConsole", true, true, true);
                thread.Start();
            }
        }

        #endregion

        #region Protected Event Overrides

        /// <summary>
        /// Raises the Load event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnLoad(e);

            //	Layout the filter group box.
            groupBoxFilter.PerformLayout();

            //	Perform an initial load of the entries that have occurred before this DiagnosticsConsole
            //	was loaded.
            LoadEntries();
        }

        #endregion Protected Event Overrides

        #region Private Event Handlers

        /// <summary>
        /// bufferingTraceListener_Changed event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void bufferingTraceListener_Changed(object sender, EventArgs e)
        {
            if (!InvokeRequired)
                LoadEntries();
            else
                BeginInvoke(new InvokeInUIThreadDelegate(LoadEntries));
        }

        /// <summary>
        /// buttonApplyFilter_Click event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void buttonApplyFilter_Click(object sender, EventArgs e)
        {
            ApplyFilter();
        }

        /// <summary>
        /// buttonClear_Click event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void buttonClear_Click(object sender, EventArgs e)
        {
            Clear();
        }

        /// <summary>
        /// buttonCopy_Click event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void buttonCopy_Click(object sender, EventArgs e)
        {
            Copy();
        }

        /// <summary>
        /// buttonReload_Click event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void buttonReload_Click(object sender, EventArgs e)
        {
            Reload();
        }

        /// <summary>
        /// buttonSelectAll_Click event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void buttonSelectAll_Click(object sender, EventArgs e)
        {
            SelectAll();
        }

        /// <summary>
        /// groupBoxFiltering_Layout event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void groupBoxFiltering_Layout(object sender, LayoutEventArgs e)
        {
            int layoutWidth = (buttonApplyFilter.Left - textBoxInclude.Left) - 20;

            textBoxInclude.Width = layoutWidth / 2;
            textBoxExclude.Width = layoutWidth / 2;
            textBoxExclude.Left = textBoxInclude.Right + 10;
            labelExclude.Left = textBoxExclude.Left;
        }

        /// <summary>
        /// listViewLog_ItemActivate event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void listViewLog_ItemActivate(object sender, EventArgs e)
        {
            Open();
        }

        /// <summary>
        /// menuItemClear_Click event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void menuItemClear_Click(object sender, EventArgs e)
        {
            Clear();
        }

        /// <summary>
        /// menuItemCopy_Click event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void menuItemCopy_Click(object sender, EventArgs e)
        {
            Copy();
        }

        /// <summary>
        /// menuItemOpen_Click event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void menuItemOpen_Click(object sender, EventArgs e)
        {
            Open();
        }

        /// <summary>
        /// menuItemReload_Click event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void menuItemReload_Click(object sender, EventArgs e)
        {
            Reload();
        }

        /// <summary>
        /// menuItemSelectAll_Click event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void menuItemSelectAll_Click(object sender, EventArgs e)
        {
            SelectAll();
        }

        /// <summary>
        /// textBoxExclude_KeyPress event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An ItemEntityNavigateEventArgs that contains the event data.</param>
        private void textBoxExclude_KeyPress(object sender, KeyPressEventArgs e)
        {
            //	Pressing Enter in the terms text box initiates the search.
            if (e.KeyChar == '\r')
            {
                e.Handled = true;
                ApplyFilter();
            }
        }

        /// <summary>
        /// textBoxInclude_KeyPress event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An ItemEntityNavigateEventArgs that contains the event data.</param>
        private void textBoxInclude_KeyPress(object sender, KeyPressEventArgs e)
        {
            //	Pressing Enter in the terms text box initiates the search.
            if (e.KeyChar == '\r')
            {
                e.Handled = true;
                ApplyFilter();
            }
        }

        #endregion Private Event Handlers

        #region Private Methods

        /// <summary>
        /// Loads entries.
        /// </summary>
        private void LoadEntries()
        {
            lock (this)
            {
                //	Update the include and exclude regular expressions.
                UpdateIncludeExcludeRegex();

                //	Obtain the set of new entries we don't already have.
                BufferingTraceListenerEntry[] bufferingTraceListenerEntryArray = bufferingTraceListener.GetEntries(ref lastEventIndex);
                if (bufferingTraceListenerEntryArray == null || bufferingTraceListenerEntryArray.Length == 0)
                    return;

                //	A long operation to follow, so prevent updates.
                listViewLog.SuspendLayout();
                listViewLog.BeginUpdate();

                //	Add the new entries.
                foreach (BufferingTraceListenerEntry bufferingTraceListenerEntry in bufferingTraceListenerEntryArray)
                {
                    //	Check the include regex, if there is one.  Exclude the entry as needed.
                    if (includeRegex != null)
                        if (!includeRegex.IsMatch(bufferingTraceListenerEntry.Category + " " + bufferingTraceListenerEntry.Text))
                            continue;

                    //	Check the exclude regex, if there is one.  Exclude the entry as needed.
                    if (excludeRegex != null)
                        if (excludeRegex.IsMatch(bufferingTraceListenerEntry.Category + " " + bufferingTraceListenerEntry.Text))
                            continue;

                    //	Sequence number.
                    ListViewItem.ListViewSubItem listViewSubItemSequenceNumber = new ListViewItem.ListViewSubItem();
                    listViewSubItemSequenceNumber.Text = bufferingTraceListenerEntry.SequenceNumberString;
                    listViewSubItemSequenceNumber.Font = itemFont;

                    //	Time.
                    ListViewItem.ListViewSubItem listViewSubItemTime = new ListViewItem.ListViewSubItem();
                    listViewSubItemTime.Text = bufferingTraceListenerEntry.DateTimeString;
                    listViewSubItemTime.Font = itemFont;

                    //	Category.
                    ListViewItem.ListViewSubItem listViewSubItemCategory = new ListViewItem.ListViewSubItem();
                    listViewSubItemCategory.Text = bufferingTraceListenerEntry.Category;
                    listViewSubItemCategory.Font = itemFont;

                    //	Determine the text to display and the color.
                    string text;
                    Color color;
                    if (bufferingTraceListenerEntry.Category == ErrText.FailText)
                    {
                        color = Color.Red;
                        text = String.Format(CultureInfo.InvariantCulture, "{0} {1}", bufferingTraceListenerEntry.Text, StackTraceText);
                    }
                    else
                    {
                        color = SystemColors.WindowText;
                        text = bufferingTraceListenerEntry.Text;
                    }

                    //	Make multi-line text pretty.
                    text.Replace('\r', ' ');
                    text.Replace('\n', ' ');

                    //	Text.
                    ListViewItem.ListViewSubItem listViewSubItemText = new ListViewItem.ListViewSubItem();
                    listViewSubItemText.Text = text;
                    listViewSubItemText.Font = itemFont;

                    ListViewItem.ListViewSubItem[] listViewSubItemArray = new ListViewItem.ListViewSubItem[] {  listViewSubItemSequenceNumber,
                                                                                                                listViewSubItemTime,
                                                                                                                listViewSubItemCategory,
                                                                                                                listViewSubItemText };
                    //	Add the item.
                    ListViewItem listViewItem = new ListViewItem(listViewSubItemArray, -1);
                    listViewItem.Tag = bufferingTraceListenerEntry;
                    listViewItem.ForeColor = color;
                    listViewLog.Items.Add(listViewItem);
                }

                //	Scroll to the bottom.
                if (listViewLog.Items.Count > 0)
                    listViewLog.EnsureVisible(listViewLog.Items.Count - 1);

                //	Re-enable updates.
                listViewLog.EndUpdate();
                listViewLog.ResumeLayout();
            }
        }

        /// <summary>
        /// Updates the include and exclude regular expressions.
        /// </summary>
        private void UpdateIncludeExcludeRegex()
        {
            //	Get the include regular expression text.
            string newIncludeRegexText = textBoxInclude.Text.Trim();
            if (newIncludeRegexText != null && newIncludeRegexText.Length == 0)
                newIncludeRegexText = String.Empty;

            //	Get the exclude regular expression text.
            string newExcludeRegexText = textBoxExclude.Text.Trim();
            if (newExcludeRegexText != null && newExcludeRegexText.Length == 0)
                newExcludeRegexText = String.Empty;

            //	Update the include regular expressions.
            if (newIncludeRegexText != includeRegexText)
            {
                includeRegexText = newIncludeRegexText;
                if (includeRegexText == String.Empty)
                    includeRegex = null;
                else
                    includeRegex = new Regex(includeRegexText, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline);
            }

            //	Update the exclude regular expressions.
            if (newExcludeRegexText != excludeRegexText)
            {
                excludeRegexText = newExcludeRegexText;
                if (excludeRegexText == String.Empty)
                    excludeRegex = null;
                else
                    excludeRegex = new Regex(excludeRegexText, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline);
            }
        }

        /// <summary>
        /// Clears the log.
        /// </summary>
        private void Clear()
        {
            lock (this)
            {
                firstEventIndex = lastEventIndex;
                listViewLog.Items.Clear();
            }
        }

        /// <summary>
        /// Applies the filter.
        /// </summary>
        private void ApplyFilter()
        {
            listViewLog.SuspendLayout();
            listViewLog.BeginUpdate();

            //	Clear the log.
            listViewLog.Items.Clear();

            //	Reset the sequence number.
            lastEventIndex = firstEventIndex;

            LoadEntries();

            listViewLog.EndUpdate();
            listViewLog.ResumeLayout();
        }

        /// <summary>
        /// Opens the selected entries.
        /// </summary>
        private void Open()
        {
            OpenForm openForm = new OpenForm(SelectedEntriesString());
            using (openForm)
                openForm.ShowDialog();
        }

        /// <summary>
        /// Reloads the log.
        /// </summary>
        private void Reload()
        {
            lock (this)
            {

                listViewLog.SuspendLayout();
                listViewLog.BeginUpdate();

                //	Clear the log.
                listViewLog.Items.Clear();

                //	Reset the sequence number.
                firstEventIndex = lastEventIndex = 0;

                LoadEntries();

                listViewLog.EndUpdate();
                listViewLog.ResumeLayout();
            }
        }

        /// <summary>
        /// Selects all entries in the log.
        /// </summary>
        private void SelectAll()
        {
            lock (this)
            {
                listViewLog.SuspendLayout();
                listViewLog.BeginUpdate();
                foreach (ListViewItem listViewItem in listViewLog.Items)
                    listViewItem.Selected = true;
                listViewLog.EndUpdate();
                listViewLog.ResumeLayout();
            }
        }

        /// <summary>
        /// Copies all selected entries in the log.
        /// </summary>
        private void Copy()
        {
            //	Instantiate the data object.
            DataObject dataObject = new DataObject();

            //	Set a Text format with the selected entries string.
            dataObject.SetData(DataFormats.Text, SelectedEntriesString());

            //	Finally, set the data object on the clipboard.
            Clipboard.SetDataObject(dataObject, true);
        }

        /// <summary>
        /// Gets a string containing the selected entries.
        /// </summary>
        /// <returns>A string containing the selected entries.</returns>
        private string SelectedEntriesString()
        {
            //	Instantiate the text and html string builders.
            StringBuilder stringBuilder = new StringBuilder();

            //	Prevent updates while we work.
            lock (this)
            {
                //	Obtain the selected list.
                ListView.SelectedListViewItemCollection selectedListViewItemCollection = listViewLog.SelectedItems;

                //	If there are entries selected, copy them to the clipboard.
                if (selectedListViewItemCollection != null && selectedListViewItemCollection.Count != 0)
                {
                    //	Build the strings to be copied to the clipboard.
                    foreach (ListViewItem listViewItem in selectedListViewItemCollection)
                    {
                        //	Append an extra line in between entries.
                        if (stringBuilder.Length != 0)
                            stringBuilder.Append("\r\n");

                        //	Access the BufferingTraceListenerEntry tagged to the entry.
                        BufferingTraceListenerEntry bufferingTraceListenerEntry = (BufferingTraceListenerEntry)listViewItem.Tag;

                        //	Build the text line for the entry.
                        stringBuilder.AppendFormat("{0}\r\n", bufferingTraceListenerEntry.ToString());
                    }
                }
            }

            //	Done.
            return stringBuilder.ToString();
        }

        /// <summary>
        /// ThreadStart for the console.
        /// </summary>
        private void ConsoleThreadStart()
        {
            Application.Run(this);
        }

        #endregion
    }
}
