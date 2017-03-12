// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using mshtml;
using OpenLiveWriter.Api;
using OpenLiveWriter.ApplicationFramework.Skinning;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Extensibility.ImageEditing;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.Controls;
using OpenLiveWriter.HtmlEditor;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Localization.Bidi;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing
{
    /// <summary>
    /// Summary description for ImageDecoratorsControl.
    /// </summary>
    public class ImageDecoratorsControl : LightweightControlContainerControl, ImageDecoratorEditorContext
    {
        private ColumnHeader columnHeaderDecorators;
        private ListView listViewDecoratorsTable;
        private CommandBarButtonEntry commandBarButtonEntryAdd;
        private IContainer components;
        private Command commandAddMenu;
        private Command commandImageRemove;

        private ImageDecoratorsCommandBarLightweightControl commandBarLightweightControl;
        private CommandBarDefinition commandBarDefinition;
        private ImageDecoratorsManager _decoratorsManager;
        private bool _loaded;

        public ImageDecoratorsControl()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();
            this.columnHeaderDecorators.Text = Res.Get(StringId.ImgSBEffect);
            ColorizedResources.Instance.ColorizationChanged += new EventHandler(RefreshColors);
            RefreshColors(this, EventArgs.Empty);

            listViewDecoratorsTable.AccessibleName = ControlHelper.ToAccessibleName(Res.Get(StringId.ImgSBAppliedEffects));
            AccessibleName = ControlHelper.ToAccessibleName(Res.Get(StringId.ImgSBEffect));
        }

        private void InitFocusAndAccessibility()
        {
            InitFocusManager();
            AddFocusableControls(commandBarLightweightControl.GetAccessibleControls());
            AddFocusableControl(listViewDecoratorsTable);
        }

        private void RefreshColors(object sender, EventArgs args)
        {
            BackColor = ColorizedResources.Instance.SidebarGradientBottomColor;
        }

        public CommandManager CommandManager
        {
            get
            {
                return _decoratorsManager.CommandManager;
            }
        }

        public ImageDecoratorsManager DecoratorsManager
        {
            get
            {
                Debug.Assert(DesignMode || _decoratorsManager != null, "ImageDecoratorsManager was not set");
                return _decoratorsManager;
            }
            set
            {
                _decoratorsManager = value;
                SimpleTextEditorCommandHelper.UseNativeBehaviors(CommandManager, listViewDecoratorsTable);
                InitializeCommands();
                //InitializeToolbar();
            }
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ColorizedResources.Instance.ColorizationChanged -= new EventHandler(RefreshColors);
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.listViewDecoratorsTable = new System.Windows.Forms.ListView();
            this.columnHeaderDecorators = new System.Windows.Forms.ColumnHeader();
            this.commandBarButtonEntryAdd = new OpenLiveWriter.ApplicationFramework.CommandBarButtonEntry(this.components);
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            this.SuspendLayout();
            //
            // listViewDecoratorsTable
            //
            this.listViewDecoratorsTable.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewDecoratorsTable.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                                                                                                      this.columnHeaderDecorators});
            this.listViewDecoratorsTable.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.listViewDecoratorsTable.HideSelection = false;
            this.listViewDecoratorsTable.Location = new System.Drawing.Point(0, 25);
            this.listViewDecoratorsTable.MultiSelect = false;
            this.listViewDecoratorsTable.Name = "listViewDecoratorsTable";
            this.listViewDecoratorsTable.RightToLeftLayout = BidiHelper.IsRightToLeft;
            this.listViewDecoratorsTable.Size = new System.Drawing.Size(156, 75);
            this.listViewDecoratorsTable.TabIndex = 0;
            this.listViewDecoratorsTable.View = System.Windows.Forms.View.Details;
            this.listViewDecoratorsTable.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listViewDecoratorsTable_KeyDown);
            this.listViewDecoratorsTable.SelectedIndexChanged += new System.EventHandler(this.listViewDecoratorsTable_SelectedIndexChanged);
            //
            // columnHeaderDecorators
            //
            this.columnHeaderDecorators.Text = "Effect";
            this.columnHeaderDecorators.Width = 135;
            //
            // commandBarButtonEntryAdd
            //
            this.commandBarButtonEntryAdd.CommandIdentifier = "AddDecorator";
            //
            // ImageDecoratorsControl
            //
            this.Controls.Add(this.listViewDecoratorsTable);
            this.Name = "ImageDecoratorsControl";
            this.Size = new System.Drawing.Size(156, 108);
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();
            this.ResumeLayout(false);

        }
        #endregion

        private void InitializeCommands()
        {
            commandContextManager = new CommandContextManager(_decoratorsManager.CommandManager);
            commandContextManager.BeginUpdate();

            commandAddMenu = new Command(CommandId.AddDecorator);
            _decoratorsManager.CommandManager.Add(commandAddMenu);
            addCommandContextMenuDefinition = new CommandContextMenuDefinition(components);
            if (_decoratorsManager != null)
            {
                for (int i = 0; i < _decoratorsManager.ImageDecoratorGroups.Length; i++)
                {
                    ImageDecoratorGroup imageDecoratorGroup = _decoratorsManager.ImageDecoratorGroups[i];

                    foreach (ImageDecorator imageDecorator in imageDecoratorGroup.ImageDecorators)
                    {
                        if (!imageDecorator.IsHidden) //don't show hidden decorators in the command list
                        {
                            Command ImageDecoratorApplyCommand = imageDecorator.Command;
                            MenuDefinitionEntryCommand imageDecoratorMenuEntry = new MenuDefinitionEntryCommand(components);
                            imageDecoratorMenuEntry.CommandIdentifier = ImageDecoratorApplyCommand.Identifier;
                            addCommandContextMenuDefinition.Entries.Add(imageDecoratorMenuEntry);

                            ImageDecoratorApplyCommand.Execute += new EventHandler(imageDecoratorApplyCommand_Execute);
                            ImageDecoratorApplyCommand.Tag = imageDecorator.Id;
                        }
                    }
                }
            }

            commandImageRemove = new Command(CommandId.RemoveDecorator);
            commandImageRemove.Execute += new EventHandler(commandRemoveMenu_Execute);
            commandContextManager.AddCommand(commandImageRemove, CommandContext.Normal);

            _decoratorsManager.CommandManager.SuppressEvents = true;
            try
            {
                commandContextManager.EndUpdate();
            }
            finally
            {
                _decoratorsManager.CommandManager.SuppressEvents = true;
            }
        }
        CommandContextManager commandContextManager;
        CommandContextMenuDefinition addCommandContextMenuDefinition;
        private CommandBarButtonEntry commandBarButtonEntryAddMenu;
        private CommandBarButtonEntry commandBarButtonEntryRemoveMenu;
        private CommandBarLabelEntry commandBarLabel;

        private void InitializeToolbar()
        {
            commandBarButtonEntryAddMenu = new CommandBarButtonEntry(components);
            commandBarButtonEntryAddMenu.CommandIdentifier = commandAddMenu.Identifier;

            commandBarButtonEntryRemoveMenu = new CommandBarButtonEntry(components);
            commandBarButtonEntryRemoveMenu.CommandIdentifier = commandImageRemove.Identifier;

            commandBarLabel = new CommandBarLabelEntry(components);
            commandBarLabel.Text = Res.Get(StringId.ImgSBEffectsLabel);

            commandBarDefinition = new CommandBarDefinition(components);
            commandBarDefinition.RightCommandBarEntries.Add(commandBarButtonEntryAddMenu);
            commandBarDefinition.RightCommandBarEntries.Add(commandBarButtonEntryRemoveMenu);

            commandBarDefinition.LeftCommandBarEntries.Add(commandBarLabel);

            commandBarLightweightControl = new ImageDecoratorsCommandBarLightweightControl(components);
            commandBarLightweightControl.LightweightControlContainerControl = this;

            commandAddMenu.CommandBarButtonContextMenuDefinition = addCommandContextMenuDefinition;

            commandBarLightweightControl.CommandManager = _decoratorsManager.CommandManager;
            commandBarLightweightControl.CommandBarDefinition = commandBarDefinition;

            InitFocusAndAccessibility();
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);

            //	If we have a command bar lightweight control, lay it out.
            if (commandBarLightweightControl != null)
            {
                //	If a command bar definition has been supplied, layout the command bar
                //	lightweight control. Otherwise, hide it.
                if (commandBarLightweightControl.CommandBarDefinition != null)
                {
                    //	Make the command bar visible before getting height.
                    commandBarLightweightControl.Visible = true;
                    defaultHeight = commandBarLightweightControl.DefaultVirtualSize.Height;
                    if (defaultHeight == 0)
                        defaultHeight = 22;
                    commandBarLightweightControl.VirtualBounds = new Rectangle(0, 0, Width, defaultHeight);
                }
            }
        }
        int defaultHeight;

        internal void SynchronizeImageDecorators()
        {
            ArrayList decoratorsList = new ArrayList();
            foreach (string decoratorId in ImageDecoratorsList.GetImageDecoratorIds())
            {
                ImageDecorator decorator = _decoratorsManager.GetImageDecorator(decoratorId);
                if (!decorator.IsHidden)
                    decoratorsList.Add(decorator);
            }

            bool needsReload = false;
            if (decoratorsList.Count != listViewDecoratorsTable.Items.Count)
            {
                needsReload = true;
            }
            else
            {
                for (int i = 0; i < decoratorsList.Count && !needsReload; i++)
                {
                    ImageDecorator decorator = (ImageDecorator)decoratorsList[i];
                    if (listViewDecoratorsTable.Items[i].Tag != decorator)
                        needsReload = true;
                }
            }

            if (needsReload)
            {
                ReloadImageDecorators();
            }
        }

        internal void LoadImageDecorators(ImagePropertiesInfo imageInfo)
        {
            _imageInfo = imageInfo;
            ReloadImageDecorators();
            //this is needed to clear/change the visible editor
            OnSelectedImageDecoratorChanged(EventArgs.Empty);
        }
        ImagePropertiesInfo _imageInfo;

        internal ImageDecoratorsList ImageDecoratorsList
        {
            get
            {
                return _imageInfo.ImageDecorators;
            }
        }

        public event EventHandler ImageDecoratorsChanged;
        protected virtual void OnImageDecoratorsChanged(EventArgs evt)
        {
            if (ImageDecoratorsChanged != null)
                ImageDecoratorsChanged(this, evt);
        }

        private void imageDecoratorApplyCommand_Execute(object sender, EventArgs e)
        {
            Command command = (Command)sender;
            string decoratorId = (string)command.Tag;
            if (!ImageDecoratorsList.ContainsDecorator(decoratorId))
            {
                ImageDecoratorsList.AddDecorator(decoratorId);
            }
            ReloadImageDecorators();

            SelectedImageDecorator = _decoratorsManager.GetImageDecorator(decoratorId);

            //force a UI refresh before the decorator gets applied
            Update();

            OnImageDecoratorsChanged(EventArgs.Empty);
        }

        private void commandRemoveMenu_Execute(object sender, EventArgs e)
        {
            OnDeleteCommand();
        }

        private void listViewDecoratorsTable_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                OnDeleteCommand();
            }
        }

        private void OnDeleteCommand()
        {
            ImageDecorator decorator = SelectedImageDecorator;
            if (decorator != null)
            {
                ImageDecoratorsList.RemoveDecorator(decorator);
                ReloadImageDecorators();
                OnImageDecoratorsChanged(EventArgs.Empty);
                //this is needed to clear/change the visible editor. switching editors is disabled in the RefreshImageDecorators() call
                OnSelectedImageDecoratorChanged(EventArgs.Empty);
            }
        }

        private void ReloadImageDecorators()
        {
            ListView.SelectedIndexCollection selectedIndices = listViewDecoratorsTable.SelectedIndices;
            int selectedIndex = selectedIndices.Count > 0 ? selectedIndices[0] : 0;

            using (new UpdateListViewEntries(listViewDecoratorsTable, new EventHandler(listViewDecoratorsTable_SelectedIndexChanged)))
            {
                listViewDecoratorsTable.Items.Clear();
                if (ImageDecoratorsList != null)
                {
                    foreach (string decoratorId in ImageDecoratorsList.GetImageDecoratorIds())
                    {
                        ImageDecorator decorator = _decoratorsManager.GetImageDecorator(decoratorId);
                        if (!decorator.IsHidden)
                        {
                            ListViewItem listItem = new ListViewItem(decorator.DecoratorName);
                            listItem.Tag = decorator;
                            listViewDecoratorsTable.Items.Add(listItem);
                        }
                    }
                }

                if (listViewDecoratorsTable.Items.Count > 0)
                {
                    //reselect the last selected index
                    selectedIndex = Math.Min(listViewDecoratorsTable.Items.Count - 1, selectedIndex);
                    listViewDecoratorsTable.Items[selectedIndex].Selected = true;
                }
            }
        }

        private class UpdateListViewEntries : IDisposable
        {
            private ListView _listView;
            private EventHandler _handler;

            public UpdateListViewEntries(ListView listView, EventHandler handler)
            {
                _listView = listView;
                _handler = handler;
                _listView.SelectedIndexChanged -= handler;
                _listView.BeginUpdate();
            }

            public void Dispose()
            {
                _listView.EndUpdate();
                _listView.SelectedIndexChanged += _handler;
            }
        }

        public event EventHandler SelectedImageDecoratorChanged;
        protected virtual void OnSelectedImageDecoratorChanged(EventArgs e)
        {
            if (SelectedImageDecoratorChanged != null)
                SelectedImageDecoratorChanged(this, e);
        }

        public ImageDecorator SelectedImageDecorator
        {
            get
            {
                ListView.SelectedListViewItemCollection selectedItems = listViewDecoratorsTable.SelectedItems;
                if (selectedItems.Count > 0)
                {
                    return selectedItems[0].Tag as ImageDecorator;
                }
                else
                    return null;
            }
            set
            {
                ImageDecorator decorator = value;
                foreach (ListViewItem item in listViewDecoratorsTable.Items)
                {
                    ImageDecorator decoratorItem = (ImageDecorator)item.Tag;
                    if (decoratorItem.Id == decorator.Id)
                    {
                        item.Selected = true;
                    }
                }
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            _loaded = true;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            //fix bug 291280 - on .NET 2.0, the anchoring is hosed up for this control, so size it manually
            if (_loaded)
            {
                listViewDecoratorsTable.Size = new System.Drawing.Size(this.Size.Width, listViewDecoratorsTable.Size.Height);
                listViewDecoratorsTable.Left = 0;
            }

        }

        #region ImageDecoratorEditorContext Members

        public IProperties Settings
        {
            get
            {
                ImageDecorator selectImageDecorator = SelectedImageDecorator;
                if (selectImageDecorator != null)
                    return ImageDecoratorsList.GetImageDecoratorSettings(selectImageDecorator);
                else
                    return null;
            }
        }

        public void ApplyDecorator()
        {
            OnImageDecoratorsChanged(EventArgs.Empty);
        }

        private void listViewDecoratorsTable_SelectedIndexChanged(object sender, EventArgs e)
        {
            OnSelectedImageDecoratorChanged(EventArgs.Empty);
        }

        public Size SourceImageSize
        {
            get { return _imageInfo.ImageSourceSize; }
        }

        public Uri SourceImageUri
        {
            get
            {
                return _imageInfo.ImageSourceUri;
            }
        }

        public IHTMLElement ImgElement
        {
            get { return _imageInfo.ImgElement; }
        }

        public RotateFlipType ImageRotation
        {
            get
            {
                return _imageInfo.ImageRotation;
            }
            set
            {
                _imageInfo.ImageRotation = value;
            }
        }

        public IImageDecoratorUndoUnit CreateUndoUnit()
        {
            Debug.Assert(_hostEditorContext != null, "HostEditorContext not set!");
            if (_hostEditorContext != null)
                return _hostEditorContext.CreateUndoUnit();
            else
                return null;
        }

        public float? EnforcedAspectRatio
        {
            get { return _imageInfo.EnforcedAspectRatio; }
        }

        public ImageDecoratorEditorContext HostEditorContext { set { _hostEditorContext = value; } }
        private ImageDecoratorEditorContext _hostEditorContext;

        #endregion
    }
}
