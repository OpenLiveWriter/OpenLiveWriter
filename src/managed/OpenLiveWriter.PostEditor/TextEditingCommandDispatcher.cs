// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Diagnostics;
using OpenLiveWriter.HtmlEditor;
using OpenLiveWriter.HtmlEditor.Controls;
using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.Interop.Com.Ribbon;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Localization.Bidi;
using OpenLiveWriter.PostEditor.Commands;

namespace OpenLiveWriter.PostEditor
{
    public delegate void TextEditingFocusHandler();
    public class TextEditingCommandDispatcher : IDisposable
    {
        private class FontFamilyCommand : GalleryCommand<string>, IRepresentativeString
        {
            private IHtmlEditorCommandSource _postEditor;
            public FontFamilyCommand()
                : base(CommandId.FontFamily)
            {
                _invalidateGalleryRepresentation = true;
                ExecuteWithArgs += new ExecuteEventHandler(FontFamilyCommand_ExecuteWithArgs);
            }

            void FontFamilyCommand_ExecuteWithArgs(object sender, ExecuteEventHandlerArgs args)
            {
                try
                {
                    _postEditor.ApplyFontFamily(items[args.GetInt(CommandId.ToString())].Label);
                }
                catch (Exception ex)
                {
                    Trace.Fail("Exception thrown when applying font family: " + ex);
                }
            }

            public override void LoadItems()
            {
                if (items.Count == 0)
                {
                    // @RIBBON TODO: Render preview images of each font family
                    string currentFontFamily = _postEditor.SelectionFontFamily;
                    selectedItem = _postEditor.SelectionFontFamily;
                    using (InstalledFontCollection fontCollection = new InstalledFontCollection())
                    {

                        FontFamily[] fontFamilies = fontCollection.Families;

                        for (int i = 0; i < fontFamilies.Length; i++)
                        {
                            items.Add(new GalleryItem(fontFamilies[i].GetName(0), null, fontFamilies[i].Name));

                            // We determine the selected index based on the font family name in English.
                            if (currentFontFamily == fontFamilies[i].Name)
                                selectedIndex = i;
                        }
                    }
                    base.LoadItems();
                }
                else
                {
                    // Note: that the font family drop down will not reflect changes to the set of
                    // Note: installed fonts made after initialization.
                    selectedIndex = INVALID_INDEX;
                    string fontName = _postEditor.SelectionFontFamily;
                    if (string.IsNullOrEmpty(fontName))
                    {
                        selectedItem = String.Empty;
                    }
                    else
                    {
                        try
                        {
                            // The font's primary name need not always be in english.
                            // The name returned by mshtml could be in culture neutral, so find the primary name
                            // since that is what we set as the cookie for GalleryItem
                            FontFamily fontFamily = new FontFamily(fontName);
                            selectedItem = fontFamily.Name;
                        }
                        catch
                        {
                            selectedItem = String.Empty;
                        }
                    }
                }
            }

            internal void RegisterPostEditor(IHtmlEditorCommandSource postEditor)
            {
                _postEditor = postEditor;
            }

            #region Implementation of IRepresentativeString

            public string RepresentativeString
            {
                get { return "Times New Roman"; }
            }

            #endregion
        }

        private class FontSizeCommand : GalleryCommand<int>, IRepresentativeString
        {
            private IHtmlEditorCommandSource _postEditor;
            public FontSizeCommand()
                : base(CommandId.FontSize)
            {
                AllowExecuteOnInvalidIndex = true;
                _invalidateGalleryRepresentation = true;
                ExecuteWithArgs += new ExecuteEventHandler(FontSizeCommand_ExecuteWithArgs);

                foreach (int size in _fontSizes)
                    items.Add(new GalleryItem(size.ToString(CultureInfo.InvariantCulture), null, size));
                UpdateInvalidationState(PropertyKeys.ItemsSource, InvalidationState.Pending);
            }

            void FontSizeCommand_ExecuteWithArgs(object sender, ExecuteEventHandlerArgs args)
            {
                try
                {
                    int index = args.GetInt(CommandId.ToString());
                    if (index != INVALID_INDEX)
                    {
                        int fontSize = _fontSizes[index];

                        _postEditor.ApplyFontSize(fontSize);

                        SetSelectedItem(fontSize);
                    }
                    else
                    {
                        // User edited the combo box edit field, but their input didn't correspond to a valid choice.
                        // We just need to update the gallery based on the current selection.
                        InvalidateSelectedItemProperties();
                    }
                }
                catch (Exception ex)
                {
                    Trace.Fail("Exception thrown when applying font size: " + ex);
                }
            }

            private static int[] _fontSizes = new int[] { 8, 10, 12, 14, 18, 24, 36 };

            private float _currentFontSize;
            public override void LoadItems()
            {
                _currentFontSize = _postEditor.SelectionFontSize;

                // Just update the selected item.
                for (int i = 0; i < _fontSizes.Length; i++)
                {
                    if (_currentFontSize == _fontSizes[i])
                        selectedIndex = i;
                }

                InvalidateSelectedItemProperties();
            }

            public override void SetSelectedItem(int selectedItem)
            {
                _currentFontSize = _postEditor.SelectionFontSize;
                base.SetSelectedItem(selectedItem);
            }

            public override string StringValue
            {
                get
                {
                    if (_currentFontSize != 0)
                    {
                        // Don't include the decimal places if it's just going add ".0";
                        // We allow for up to 1 decimal place, but we don't want to show it
                        // if it is just a zero.
                        double rounded = Math.Round(_currentFontSize, 1, MidpointRounding.AwayFromZero);
                        string format = (Math.Truncate(rounded) == rounded) ? "F0" : "F1";

                        return rounded.ToString(format, CultureInfo.InvariantCulture);
                    }

                    return String.Empty;
                }
            }

            internal void RegisterPostEditor(IHtmlEditorCommandSource postEditor)
            {
                _postEditor = postEditor;
            }

            #region Implementation of IRepresentativeString

            public string RepresentativeString
            {
                get { return Convert.ToString(_fontSizes[_fontSizes.Length - 1].ToString("F1", CultureInfo.InvariantCulture), CultureInfo.InvariantCulture); }
            }

            #endregion
        }

        private class FontHighlightColorPickerCommand : FontColorPickerCommand
        {
            internal FontHighlightColorPickerCommand(Color color)
                : base(CommandId.FontBackgroundColor, color)
            {
            }

            public override Bitmap SmallImage
            {
                get
                {
                    Bitmap bitmap = base.SmallImage;
                    using (Graphics g = Graphics.FromImage(bitmap))
                    {
                        using (SolidBrush brush = new SolidBrush(this.SelectedColor))
                        {
                            g.FillRectangle(brush, 0, 12, 16, 4);
                        }
                    }

                    return bitmap;
                }
                set
                {
                    base.SmallImage = value;
                }
            }

        }

        /// <summary>
        /// This command corresponds to a ColorPickerDropDown in ribbon markup.
        /// </summary>
        private class FontColorPickerCommand : PreviewCommand, IColorPickerCommand
        {
            // Note: These numbers need to match the corresponding attributes in the ribbon markup.
            private const uint _numStandardColorsRows = 6;
            private const uint _numColumns = 5;

            public FontColorPickerCommand(CommandId commandId, Color color)
                : base(commandId)
            {
                _selectedColorType = SwatchColorType.RGB;
                _selectedColor = color;

                OnStartPreview += new EventHandler(FontColorPickerCommand_OnStartPreview);
                OnCancelPreview += new EventHandler(FontColorPickerCommand_OnCancelPreview);

                UpdateInvalidationState(PropertyKeys.Color, InvalidationState.Pending);
                UpdateInvalidationState(PropertyKeys.ColorType, InvalidationState.Pending);
            }

            private SwatchColorType _savedSelectedColorType;
            private Color _savedSelectedColor;

            void FontColorPickerCommand_OnStartPreview(object sender, EventArgs e)
            {
                _savedSelectedColorType = _selectedColorType;
                _savedSelectedColor = _selectedColor;
            }

            void FontColorPickerCommand_OnCancelPreview(object sender, EventArgs e)
            {
                SetSelectedColor(_savedSelectedColor, _savedSelectedColorType);
            }

            private ColorPickerColor[] _standardColors = new ColorPickerColor[]
                                                             {
                                                                 new ColorPickerColor(Color.FromArgb(255, 255, 255), StringId.ColorWhite),
                                                                 new ColorPickerColor(Color.FromArgb(255, 0,   0), StringId.ColorVibrantRed),
                                                                 new ColorPickerColor(Color.FromArgb(192, 80,  77), StringId.ColorProfessionalRed),
                                                                 new ColorPickerColor(Color.FromArgb(209, 99,  73), StringId.ColorEarthyRed),
                                                                 new ColorPickerColor(Color.FromArgb(221, 132, 132), StringId.ColorPastelRed),

                                                                 new ColorPickerColor(Color.FromArgb(204, 204, 204), StringId.ColorLightGray),
                                                                 new ColorPickerColor(Color.FromArgb(255, 192, 0), StringId.ColorVibrantOrange),
                                                                 new ColorPickerColor(Color.FromArgb(247, 150, 70), StringId.ColorProfessionalOrange),
                                                                 new ColorPickerColor(Color.FromArgb(209, 144, 73), StringId.ColorEarthyOrange),
                                                                 new ColorPickerColor(Color.FromArgb(243, 164, 71), StringId.ColorPastelOrange),

                                                                 new ColorPickerColor(Color.FromArgb(165, 165, 165), StringId.ColorMediumGray),
                                                                 new ColorPickerColor(Color.FromArgb(255, 255, 0), StringId.ColorVibrantYellow),
                                                                 new ColorPickerColor(Color.FromArgb(155, 187, 89), StringId.ColorProfessionalGreen),
                                                                 new ColorPickerColor(Color.FromArgb(204, 180, 0), StringId.ColorEarthyYellow),
                                                                 new ColorPickerColor(Color.FromArgb(223, 206, 4), StringId.ColorPastelYellow),

                                                                 new ColorPickerColor(Color.FromArgb(102, 102, 102), StringId.ColorDarkGray),
                                                                 new ColorPickerColor(Color.FromArgb(0,   255, 0), StringId.ColorVibrantGreen),
                                                                 new ColorPickerColor(Color.FromArgb(75,  172, 198), StringId.ColorProfessionalAqua),
                                                                 new ColorPickerColor(Color.FromArgb(143, 176, 140), StringId.ColorEarthyGreen),
                                                                 new ColorPickerColor(Color.FromArgb(165, 181, 146), StringId.ColorPastelGreen),

                                                                 new ColorPickerColor(Color.FromArgb(51,  51,  51), StringId.ColorCharcoal),
                                                                 new ColorPickerColor(Color.FromArgb(0,   0,   255), StringId.ColorVibrantBlue),
                                                                 new ColorPickerColor(Color.FromArgb(79,  129, 189), StringId.ColorProfessionalBlue),
                                                                 new ColorPickerColor(Color.FromArgb(100, 107, 134), StringId.ColorEarthyBlue),
                                                                 new ColorPickerColor(Color.FromArgb(128, 158, 194), StringId.ColorPastelBlue),

                                                                 new ColorPickerColor(Color.FromArgb(0,   0,   0), StringId.ColorBlack),
                                                                 new ColorPickerColor(Color.FromArgb(155, 0,   211), StringId.ColorVibrantPurple),
                                                                 new ColorPickerColor(Color.FromArgb(128, 100, 162), StringId.ColorProfessionalPurple),
                                                                 new ColorPickerColor(Color.FromArgb(158, 124, 124), StringId.ColorEarthyBrown),
                                                                 new ColorPickerColor(Color.FromArgb(156, 133, 192), StringId.ColorPastelPurple),
        };

            #region Overrides of ColorPickerCommand

            public string[] StandardColorsTooltips
            {
                get
                {
                    string[] tooltips = new string[NumStandardColorsRows * NumColumns];
                    for (int row = 0; row < NumStandardColorsRows; row++)
                    {
                        for (int column = 0; column < NumColumns; column++)
                        {
                            int idx = Convert.ToInt32(row * NumColumns + column);
                            tooltips[idx] = Res.Get(_standardColors[idx].StringId);
                        }
                    }
                    return tooltips;
                }
            }

            public uint[] StandardColors
            {
                get
                {
                    uint[] colors = new uint[NumStandardColorsRows * NumColumns];
                    for (int row = 0; row < NumStandardColorsRows; row++)
                    {
                        for (int column = 0; column < NumColumns; column++)
                        {
                            int idx = Convert.ToInt32(row * NumColumns + column);
                            colors[idx] = Convert.ToUInt32(ColorHelper.ColorToBGR(_standardColors[idx].Color));
                        }
                    }
                    return colors;
                }
            }

            public uint NumStandardColorsRows
            {
                get { return _numStandardColorsRows; }
            }

            public uint NumColumns
            {
                get { return _numColumns; }
            }

            private Color _selectedColor;
            public Color SelectedColor { get { return _selectedColor; } }
            public int SelectedColorAsBGR
            {
                get { return ColorHelper.ColorToBGR(_selectedColor); }
            }

            private SwatchColorType _selectedColorType = SwatchColorType.RGB;
            public SwatchColorType SelectedColorType
            {
                get { return _selectedColorType; }
            }

            private bool _automatic;
            public bool Automatic
            {
                get { return _automatic; }
                set { _automatic = value; }
            }

            public void SetSelectedColor(Color color, SwatchColorType colorType)
            {
                _selectedColor = color;
                _selectedColorType = colorType;

                UpdateInvalidationState(PropertyKeys.Color, InvalidationState.Pending);
                UpdateInvalidationState(PropertyKeys.ColorType, InvalidationState.Pending);

                Invalidate();
            }

            public override void GetPropVariant(PropertyKey key, PropVariantRef currentValue, ref PropVariant value)
            {
                if (key == PropertyKeys.AutomaticColorLabel ||
                         key == PropertyKeys.NoColorLabel ||
                         key == PropertyKeys.MoreColorsLabel)
                {
                    value.SetString((string)currentValue.PropVariant.Value);
                }
                else if (key == PropertyKeys.Color)
                {
                    value.SetUInt((uint)SelectedColorAsBGR);
                }
                else if (key == PropertyKeys.ColorType)
                {
                    value.SetUInt(Convert.ToUInt32(SelectedColorType, CultureInfo.InvariantCulture));
                }
                else if (key == PropertyKeys.StandardColors)
                {
                    value.SetUIntVector(StandardColors);
                }
                else if (key == PropertyKeys.StandardColorsTooltips)
                {
                    value.SetStringVector(StandardColorsTooltips);
                }
                else
                    base.GetPropVariant(key, currentValue, ref value);
            }

            #endregion

            public override int PerformExecute(CommandExecutionVerb verb, PropertyKeyRef key, PropVariantRef currentValue, IUISimplePropertySet commandExecutionProperties)
            {
                SwatchColorType colorType = (SwatchColorType)Convert.ToInt32(currentValue.PropVariant.Value, CultureInfo.InvariantCulture);
                ExecuteEventHandlerArgs args = new ExecuteEventHandlerArgs();

                switch (colorType)
                {
                    case SwatchColorType.NoColor:
                        break;
                    case SwatchColorType.RGB:
                        PropVariant color;
                        commandExecutionProperties.GetValue(ref PropertyKeys.Color, out color);

                        args.Add("Automatic", false);
                        args.Add("SelectedColor", ColorHelper.BGRToColor(Convert.ToInt32(color.Value, CultureInfo.InvariantCulture)));
                        args.Add("SwatchColorType", (int)colorType);

                        break;
                    case SwatchColorType.Automatic:
                        Debug.Assert(false, "Automatic is not implemented.");
                        args.Add("Automatic", true);
                        break;
                    default:
                        break;
                }

                PerformExecuteWithArgs(verb, args);
                return HRESULT.S_OK;
            }
        }

        private readonly CommandManager CommandManager;

        public TextEditingCommandDispatcher(IWin32Window owner, IHtmlStylePicker stylePicker, CommandManager commandManager)
        {
            CommandManager = commandManager;
            _owner = owner;
            _stylePicker = stylePicker;
            InitializeCommands();
        }

        public void RegisterPostEditor(IHtmlEditorCommandSource postEditor, IHtmlEditorComponentContextDelegate componentContext, TextEditingFocusHandler focusCallback)
        {
            _postEditor = postEditor;
            _focusCallback = focusCallback;
            RegisterSimpleTextEditor(postEditor);
            commandFontSize.RegisterPostEditor(_postEditor);
            commandFontSize.ComponentContext = componentContext;
            commandFontSize.Invalidate();

            commandFontFamily.RegisterPostEditor(_postEditor);
            commandFontFamily.ComponentContext = componentContext;
            commandFontFamily.Invalidate();

            fontColorPickerCommand.ComponentContext = componentContext;
            highlightColorPickerCommand.ComponentContext = componentContext;
            highlightColorPickerCommand.Invalidate();
        }

        public void RegisterSimpleTextEditor(ISimpleTextEditorCommandSource simpleTextEditor)
        {
            // add to our list of editors
            _simpleTextEditors.Add(simpleTextEditor);

            // subscribe to command state changed event
            simpleTextEditor.CommandStateChanged += new EventHandler(simpleTextEditor_CommandStateChanged);
            simpleTextEditor.AggressiveCommandStateChanged += new EventHandler(simpleTextEditor_AggressiveCommandStateChanged);
        }

        public void ManageCommands()
        {
            CommandManager.BeginUpdate();
            try
            {
                // @RIBBON TODO: Perhaps we could be more efficient than just entirely invalidating here...
                commandFontFamily.Enabled =
                    commandFontSize.Enabled =
                    fontColorPickerCommand.Enabled =
                    highlightColorPickerCommand.Enabled = PostEditor.CanApplyFormatting(CommandId.Bold);
                commandFontFamily.Invalidate();
                commandFontSize.Invalidate();

                fontColorPickerCommand.SetSelectedColor(Color.FromArgb(PostEditor.SelectionForeColor), SwatchColorType.RGB);

                EditorTextAlignment alignment = PostEditor.GetSelectionAlignment();
                _commandAlignLeft.Enabled = PostEditor.CanApplyFormatting(CommandId.AlignLeft);
                _commandAlignLeft.Latched = alignment == EditorTextAlignment.Left;
                _commandAlignCenter.Enabled = PostEditor.CanApplyFormatting(CommandId.AlignCenter);
                _commandAlignCenter.Latched = alignment == EditorTextAlignment.Center;
                _commandAlignRight.Enabled = PostEditor.CanApplyFormatting(CommandId.AlignRight);
                _commandAlignRight.Latched = alignment == EditorTextAlignment.Right;
                _commandAlignJustify.Enabled = PostEditor.CanApplyFormatting(null);
                _commandAlignJustify.Latched = alignment == EditorTextAlignment.Justify;

                PostEditor.CommandManager.Invalidate(CommandId.AlignLeft);
                PostEditor.CommandManager.Invalidate(CommandId.AlignCenter);
                PostEditor.CommandManager.Invalidate(CommandId.AlignRight);
                PostEditor.CommandManager.Invalidate(CommandId.Justify);

                foreach (TextEditingCommand textEditingCommand in _textEditingCommands)
                    textEditingCommand.Manage();
            }
            finally
            {
                CommandManager.EndUpdate();
            }
        }

        public void AggressiveManageCommands()
        {
            CommandManager.BeginUpdate();
            try
            {
                foreach (TextEditingCommand textEditingCommand in _textEditingCommands)
                    if (textEditingCommand.ManageAggressively)
                        textEditingCommand.Manage();
            }
            finally
            {
                CommandManager.EndUpdate();
            }
        }

        public void Dispose()
        {
            foreach (ISimpleTextEditorCommandSource commandSource in _simpleTextEditors)
            {
                commandSource.CommandStateChanged -= new EventHandler(simpleTextEditor_CommandStateChanged);
                commandSource.AggressiveCommandStateChanged -=
                    new EventHandler(simpleTextEditor_AggressiveCommandStateChanged);
            }

            if (components != null)
                components.Dispose();
        }

        private void InitializeCommands()
        {
            CommandManager.BeginUpdate();

            InitializeCommand(new UndoCommand());
            InitializeCommand(new RedoCommand());
            InitializeCommand(new CutCommand());
            InitializeCommand(new CopyCommand());
            InitializeCommand(new PasteCommand());
            if (GlobalEditorOptions.SupportsFeature(ContentEditorFeature.SpecialPaste))
                InitializeCommand(new PasteSpecialCommand());
            InitializeCommand(new ClearCommand());
            InitializeCommand(new SelectAllCommand());
            InitializeCommand(new BoldCommand());
            InitializeCommand(new ItalicCommand());
            InitializeCommand(new UnderlineCommand());
            InitializeCommand(new StrikethroughCommand());
            InitializeCommand(new StyleCommand(_stylePicker, new TextEditingFocusHandler(FocusEditor)));
            InitializeCommand(new AlignLeftCommand());
            InitializeCommand(new AlignCenterCommand());
            InitializeCommand(new AlignJustifyCommand());
            InitializeCommand(new AlignRightCommand());
            InitializeCommand(new NumbersCommand());
            InitializeCommand(new BulletsCommand());
            InitializeCommand(new BlockquoteCommand());
            InitializeCommand(new PrintCommand());
            InitializeCommand(new PrintPreviewCommand());
            InitializeCommand(new IndentCommand());
            InitializeCommand(new OutdentCommand());
            InitializeCommand(new LTRTextBlockCommand());
            InitializeCommand(new RTLTextBlockCommand());
            InitializeCommand(new InsertLinkCommand());
            InitializeCommand(new FindCommand());
            InitializeCommand(new CheckSpellingCommand());
            InitializeCommand(new EditLinkCommand());
            InitializeCommand(new RemoveLinkCommand());
            InitializeCommand(new RemoveLinkAndClearFormattingCommand());
            InitializeCommand(new OpenLinkCommand());
            InitializeCommand(new AddToGlossaryCommand());
            InitializeCommand(new SuperscriptCommand());
            InitializeCommand(new SubscriptCommand());
            InitializeCommand(new ClearFormattingCommand());

            commandFontSize = new FontSizeCommand();
            CommandManager.Add(commandFontSize);

            commandFontFamily = new FontFamilyCommand();
            CommandManager.Add(commandFontFamily);

            fontColorPickerCommand = new FontColorPickerCommand(CommandId.FontColorPicker, Color.Black);
            CommandManager.Add(fontColorPickerCommand, fontColorPickerCommand_Execute);

            highlightColorPickerCommand = new FontHighlightColorPickerCommand(Color.Yellow);
            CommandManager.Add(highlightColorPickerCommand, highlightColorPickerCommand_Execute);

            _commandAlignLeft = FindEditingCommand(CommandId.AlignLeft);
            _commandAlignCenter = FindEditingCommand(CommandId.AlignCenter);
            _commandAlignRight = FindEditingCommand(CommandId.AlignRight);
            _commandAlignJustify = FindEditingCommand(CommandId.Justify);

            CommandManager.EndUpdate();

            // notify all of our commands that initialization is complete
            foreach (TextEditingCommand textEditingCommand in _textEditingCommands)
                textEditingCommand.OnAllCommandsInitialized();
        }

        void fontColorPickerCommand_Execute(object sender, ExecuteEventHandlerArgs e)
        {
            //if (fontColorPickerCommand.Automatic)
            //    PostEditor.ApplyAutomaticFontForeColor();
            //else
            Color color = e.GetColor("SelectedColor");
            PostEditor.ApplyFontForeColor(color.ToArgb());
        }

        void highlightColorPickerCommand_Execute(object sender, ExecuteEventHandlerArgs e)
        {
            Color? color = null;
            if (e.HasArg("SelectedColor"))
            {
                color = e.GetColor("SelectedColor");
                highlightColorPickerCommand.SetSelectedColor(color.Value, SwatchColorType.RGB);
            }
            else
            {
                highlightColorPickerCommand.SetSelectedColor(Color.White, SwatchColorType.NoColor);
            }

            PostEditor.ApplyFontBackColor(color != null ? (int?)color.Value.ToArgb() : null);
        }

        private void InitializeCommand(TextEditingCommand textEditingCommand)
        {
            // create the command instance
            Command command = textEditingCommand.CreateCommand();

            // hookup the command implementation to the dispatcher and command instance
            textEditingCommand.SetContext(this, command);

            // add optional context menu text override
            if (textEditingCommand.ContextMenuText != null)
                command.MenuText = textEditingCommand.ContextMenuText;

            // hookup the command to its execute handler
            command.Execute += new EventHandler(textEditingCommand.Execute);
            command.ExecuteWithArgs += new ExecuteEventHandler(textEditingCommand.ExecuteWithArgs);

            command.CommandBarButtonContextMenuDefinition = textEditingCommand.CommandBarButtonContextMenuDefinition;

            // add to the command manager
            CommandManager.Add(command);

            // add to our internal list
            _textEditingCommands.Add(textEditingCommand);
        }

        private void FocusEditor()
        {
            _focusCallback();
        }

        private void editingCommand_BeforeShowInMenu(object sender, EventArgs ea)
        {
            ManageCommands();
        }

        private void simpleTextEditor_CommandStateChanged(object sender, EventArgs e)
        {
            ManageCommands();
        }

        private void simpleTextEditor_AggressiveCommandStateChanged(object sender, EventArgs e)
        {
            AggressiveManageCommands();
        }

        public void TitleFocusChanged()
        {
            ManageCommands();
        }

        private Command FindEditingCommand(CommandId commandId)
        {
            foreach (TextEditingCommand textEditingCommand in _textEditingCommands)
                if (textEditingCommand.CommandId == commandId)
                    return textEditingCommand.Command;

            // null if none found
            return null;
        }

        private ISimpleTextEditorCommandSource ActiveSimpleTextEditor
        {
            get
            {
                // if an editor has focus then it is considered active
                foreach (ISimpleTextEditorCommandSource simpleTextEditor in _simpleTextEditors)
                    if (simpleTextEditor.HasFocus)
                        return simpleTextEditor;

                // otherwise return the main content editor
                return _postEditor;
            }
        }

        private IHtmlEditorCommandSource PostEditor
        {
            get
            {
                return _postEditor;
            }
        }

        private class UndoCommand : TextEditingCommand
        {
            public override CommandId CommandId { get { return CommandId.Undo; } }

            protected override void Execute()
            {
                ActiveSimpleTextEditor.Undo();
            }

            public override void Manage()
            {
                Enabled = ActiveSimpleTextEditor.CanUndo;
            }

            public override bool ManageAggressively
            {
                get
                {
                    return true;
                }
            }

        }

        private class RedoCommand : TextEditingCommand
        {
            public override CommandId CommandId { get { return CommandId.Redo; } }

            protected override void Execute()
            {
                ActiveSimpleTextEditor.Redo();
            }

            public override void Manage()
            {
                Enabled = ActiveSimpleTextEditor.CanRedo;
            }

            public override bool ManageAggressively
            {
                get
                {
                    return true;
                }
            }

        }

        private class CutCommand : TextEditingCommand
        {
            public override CommandId CommandId { get { return CommandId.Cut; } }

            protected override void Execute()
            {
                ActiveSimpleTextEditor.Cut();
            }

            public override void Manage()
            {
                Enabled = ActiveSimpleTextEditor.CanCut;
            }
        }

        private class CopyCommand : TextEditingCommand
        {
            public override CommandId CommandId { get { return CommandId.CopyCommand; } }

            protected override void Execute()
            {
                ActiveSimpleTextEditor.Copy();
            }

            public override void Manage()
            {
                Enabled = ActiveSimpleTextEditor.CanCopy;
            }
        }

        private class PasteCommand : TextEditingCommand
        {
            public override CommandId CommandId { get { return CommandId.Paste; } }

            protected override void Execute()
            {
                using (ApplicationPerformance.LogEvent("Paste"))
                    ActiveSimpleTextEditor.Paste();
            }

            public override void Manage()
            {
                Enabled = ActiveSimpleTextEditor.CanPaste;
            }
        }

        private class PasteSpecialCommand : TextEditingCommand
        {
            public override CommandId CommandId { get { return CommandId.PasteSpecial; } }

            public override string ContextMenuText { get { return Command.MenuText; } }

            protected override void Execute()
            {
                PostEditor.PasteSpecial();
            }

            public override void Manage()
            {
                // For some reason the next line does
                // not cause the main menu to be rebuilt. This causes the
                // Paste Special command to not show up in the main menu.
                //
                // this.Command.On = PostEditor.AllowPasteSpecial ;

                Enabled = PostEditor.AllowPasteSpecial && PostEditor.CanPasteSpecial;
            }
        }

        private class ClearCommand : TextEditingCommand
        {
            public override CommandId CommandId { get { return CommandId.Clear; } }

            protected override void Execute()
            {
                ActiveSimpleTextEditor.Clear();
            }

            public override void Manage()
            {
                Enabled = ActiveSimpleTextEditor.CanClear;
            }
        }

        private class SelectAllCommand : TextEditingCommand
        {
            public override CommandId CommandId { get { return CommandId.SelectAll; } }

            protected override void Execute()
            {
                ActiveSimpleTextEditor.SelectAll();
            }

            public override void Manage()
            {
                Enabled = PostEditor.FullyEditableRegionActive;
            }
        }

        private abstract class LatchedTextEditingCommand : TextEditingCommand
        {
            // The ribbon maintains internal state about a command's latched value.
            // Normally, the toggle behavior (i.e. clicking on an unlatched command causes it to become latched) is what we want.
            // However, there are some commands that should not behave like toggle buttons in source mode.
            // We need to invalidate the latched property (BooleanValue) when we execute for these commands.
            protected override void Execute()
            {
                Command.Invalidate(new[] { PropertyKeys.BooleanValue });
            }
        }

        private class BoldCommand : LatchedTextEditingCommand
        {
            public override CommandId CommandId { get { return CommandId.Bold; } }

            protected override void Execute()
            {
                PostEditor.ApplyBold();
                base.Execute();
            }

            public override void Manage()
            {
                Latched = PostEditor.SelectionBold;
                Enabled = PostEditor.CanApplyFormatting(CommandId);
                PostEditor.CommandManager.Invalidate(CommandId);
            }

            public override Command CreateCommand()
            {
                return new LetterCommand(CommandId, Res.Get(StringId.ToolbarBoldLetter)[0], FontStyle.Bold);
            }
        }

        private class SuperscriptCommand : LatchedTextEditingCommand
        {
            public override CommandId CommandId { get { return CommandId.Superscript; } }

            protected override void Execute()
            {
                PostEditor.ApplySuperscript();
                base.Execute();
            }

            public override void Manage()
            {
                Latched = PostEditor.SelectionSuperscript;
                Enabled = PostEditor.CanApplyFormatting(CommandId);
                PostEditor.CommandManager.Invalidate(CommandId);
            }

            public override Command CreateCommand()
            {
                return new OverridableCommand(CommandId);
            }
        }

        private class SubscriptCommand : LatchedTextEditingCommand
        {
            public override CommandId CommandId { get { return CommandId.Subscript; } }

            protected override void Execute()
            {
                PostEditor.ApplySubscript();
                base.Execute();
            }

            public override void Manage()
            {
                Latched = PostEditor.SelectionSubscript;
                Enabled = PostEditor.CanApplyFormatting(CommandId);
                PostEditor.CommandManager.Invalidate(CommandId);
            }

            public override Command CreateCommand()
            {
                return new OverridableCommand(CommandId);
            }
        }

        private class ClearFormattingCommand : TextEditingCommand
        {
            public override CommandId CommandId { get { return CommandId.ClearFormatting; } }

            protected override void Execute()
            {
                PostEditor.ClearFormatting();
                ManageAll();
            }

            public override void Manage()
            {
                Enabled = PostEditor.CanApplyFormatting(CommandId);
            }

            public override Command CreateCommand()
            {
                return new OverridableCommand(CommandId);
            }
        }

        internal class LetterCommand : OverridableCommand, CommandBarButtonLightweightControl.ICustomButtonBitmapPaint
        {
            private char _letter;
            private FontStyle _fontStyle;

            public LetterCommand(CommandId commandId, char letter, FontStyle fontStyle)
                : base(commandId)
            {
                _letter = letter;
                _fontStyle = fontStyle;
            }

            public int Width { get { return 16; } }
            public int Height { get { return 16; } }

            private static bool assertOnFontFamilyFailure = true;
            public void Paint(BidiGraphics g, Rectangle bounds, CommandBarButtonLightweightControl.DrawState drawState)
            {
                FontFamily fontFamily = new FontFamily(GenericFontFamilies.Serif);
                try
                {
                    string fontFamilyStr = Res.Get(StringId.ToolbarFontStyleFontFamily);
                    if (fontFamilyStr != null && fontFamilyStr.Length > 0)
                        fontFamily = new FontFamily(fontFamilyStr);
                }
                catch (Exception e)
                {
                    if (assertOnFontFamilyFailure)
                    {
                        assertOnFontFamilyFailure = false;
                        Trace.WriteLine("Failed to load font family: " + e.ToString());
                    }
                }

                using (Font f = new Font(fontFamily, Res.ToolbarFormatButtonFontSize, FontStyle.Bold | _fontStyle, GraphicsUnit.Pixel, 0))
                {
                    // Note: no high contrast mode support here
                    Color color;
                    if (!SystemInformation.HighContrast)
                    {
                        color = Color.FromArgb(54, 73, 98);
                        if (drawState == CommandBarButtonLightweightControl.DrawState.Disabled)
                            color = Color.FromArgb(202, 202, 202);

                    }
                    else
                    {
                        if (drawState == CommandBarButtonLightweightControl.DrawState.Disabled)
                            color = SystemColors.GrayText;
                        else
                            color = SystemColors.WindowText;
                    }

                    bounds.Y -= 1;
                    g.DrawText(_letter + "", f, bounds, color, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.PreserveGraphicsTranslateTransform | TextFormatFlags.NoPadding | TextFormatFlags.NoClipping);

                }
            }
        }

        private class ItalicCommand : LatchedTextEditingCommand
        {
            public override CommandId CommandId { get { return CommandId.Italic; } }

            protected override void Execute()
            {
                PostEditor.ApplyItalic();
                base.Execute();
            }

            public override void Manage()
            {
                Latched = PostEditor.SelectionItalic;
                Enabled = PostEditor.CanApplyFormatting(CommandId);
                PostEditor.CommandManager.Invalidate(CommandId);
            }

            public override Command CreateCommand()
            {
                return new LetterCommand(CommandId, Res.Get(StringId.ToolbarItalicLetter)[0], FontStyle.Italic);
            }
        }

        private class UnderlineCommand : LatchedTextEditingCommand
        {
            public override CommandId CommandId { get { return CommandId.Underline; } }

            protected override void Execute()
            {
                PostEditor.ApplyUnderline();
                base.Execute();
            }

            public override void Manage()
            {
                Latched = PostEditor.SelectionUnderlined;
                Enabled = PostEditor.CanApplyFormatting(CommandId);
                PostEditor.CommandManager.Invalidate(CommandId);
            }

            public override Command CreateCommand()
            {
                return new LetterCommand(CommandId, Res.Get(StringId.ToolbarUnderlineLetter)[0], FontStyle.Underline);
            }
        }

        private class StrikethroughCommand : LatchedTextEditingCommand
        {
            public override CommandId CommandId { get { return CommandId.Strikethrough; } }

            protected override void Execute()
            {
                PostEditor.ApplyStrikethrough();
                base.Execute();
            }

            public override void Manage()
            {
                Latched = PostEditor.SelectionStrikethrough;
                Enabled = PostEditor.CanApplyFormatting(CommandId);
                PostEditor.CommandManager.Invalidate(CommandId);
            }

            public override Command CreateCommand()
            {
                return new LetterCommand(CommandId, Res.Get(StringId.ToolbarStrikethroughLetter)[0], FontStyle.Strikeout);
            }
        }

        private class PrintCommand : TextEditingCommand
        {
            public override CommandId CommandId
            {
                get
                {
                    return CommandId.Print;
                }
            }

            public override void Manage()
            {
                Enabled = PostEditor.CanPrint;
            }

            protected override void Execute()
            {
                PostEditor.Print();
            }
        }

        private class PrintPreviewCommand : TextEditingCommand
        {
            public override CommandId CommandId
            {
                get
                {
                    return CommandId.PrintPreview;
                }
            }

            public override void Manage()
            {
                Enabled = PostEditor.CanPrint;
            }

            protected override void Execute()
            {
                PostEditor.PrintPreview();
            }
        }

        private class StyleCommand : TextEditingCommand
        {
            IHtmlStylePicker _stylePicker;
            TextEditingFocusHandler _focusCallback;
            public StyleCommand(IHtmlStylePicker stylePicker, TextEditingFocusHandler focusCallback)
            {
                _stylePicker = stylePicker;
                _focusCallback = focusCallback;
                _stylePicker.HtmlStyleChanged += new EventHandler(_stylePicker_StyleChanged);
            }
            public override CommandId CommandId { get { return CommandId.Style; } }

            protected override void Execute()
            {
                PostEditor.ApplyHtmlFormattingStyle(_stylePicker.SelectedStyle);

                //Bug 244868 - restore focus back to the editor when a new style is selected
                _focusCallback();
            }

            // @RIBBON TODO: Rationalize existing StyleCommand with SemanticHtmlStyleCommand

            public override void Manage()
            {
                bool enabled = PostEditor.CanApplyFormatting(CommandId.Style);
                Enabled = enabled;
                _stylePicker.Enabled = enabled;

                SemanticHtmlGalleryCommand semanticHtmlGalleryCommand = (SemanticHtmlGalleryCommand)PostEditor.CommandManager.Get(CommandId.SemanticHtmlGallery);
                if (semanticHtmlGalleryCommand != null)
                {
                    semanticHtmlGalleryCommand.SelectedStyle = PostEditor.SelectionStyleName;
                    semanticHtmlGalleryCommand.Enabled = enabled;
                }

                _stylePicker.SelectStyleByElementName(PostEditor.SelectionStyleName);
            }

            private void _stylePicker_StyleChanged(object sender, EventArgs e)
            {
                Command.PerformExecute();
            }

            public override bool ManageAggressively
            {
                get { return false; }
            }
        }

        private abstract class AlignCommand : LatchedTextEditingCommand
        {
            public AlignCommand(EditorTextAlignment alignment)
            {
                _alignment = alignment;
            }

            protected override void Execute()
            {
                // if we are already latched then this means remove formatting
                if (Command.Latched)
                {
                    PostEditor.ApplyAlignment(EditorTextAlignment.None);
                }
                else
                {
                    PostEditor.ApplyAlignment(_alignment);
                }
                base.Execute();
            }

            public override void Manage()
            {
                // Do nothing, we will explicitly manage them all together in ManageCommands
            }

            private EditorTextAlignment _alignment;
        }

        private class AlignLeftCommand : AlignCommand
        {
            public AlignLeftCommand() : base(EditorTextAlignment.Left) { }
            public override CommandId CommandId { get { return CommandId.AlignLeft; } }
            public override string ContextMenuText { get { return Command.MenuText; } }
        }

        private class AlignCenterCommand : AlignCommand
        {
            public AlignCenterCommand() : base(EditorTextAlignment.Center) { }
            public override CommandId CommandId { get { return CommandId.AlignCenter; } }
            public override string ContextMenuText { get { return Command.MenuText; } }
        }

        private class AlignJustifyCommand : AlignCommand
        {
            public AlignJustifyCommand() : base(EditorTextAlignment.Justify) { }
            public override CommandId CommandId { get { return CommandId.Justify; } }
            public override string ContextMenuText { get { return Command.MenuText; } }
        }

        private class AlignRightCommand : AlignCommand
        {
            public AlignRightCommand() : base(EditorTextAlignment.Right) { }
            public override CommandId CommandId { get { return CommandId.AlignRight; } }
            public override string ContextMenuText { get { return Command.MenuText; } }
        }

        private class NumbersCommand : LatchedTextEditingCommand
        {
            public override CommandId CommandId { get { return CommandId.Numbers; } }

            public override string ContextMenuText { get { return Command.MenuText; } }

            protected override void Execute()
            {
                PostEditor.ApplyNumbers();
                base.Execute();
            }

            public override void Manage()
            {
                Latched = PostEditor.SelectionNumbered;
                Enabled = PostEditor.CanApplyFormatting(CommandId);
                Command.Invalidate();
            }

            public override bool ManageAggressively
            {
                get { return false; }
            }
        }

        private class BulletsCommand : LatchedTextEditingCommand
        {
            public override CommandId CommandId { get { return CommandId.Bullets; } }

            public override string ContextMenuText { get { return Command.MenuText; } }

            protected override void Execute()
            {
                PostEditor.ApplyBullets();
                base.Execute();
            }

            public override void Manage()
            {
                Latched = PostEditor.SelectionBulleted;
                Enabled = PostEditor.CanApplyFormatting(CommandId);
                Command.Invalidate();
            }

            public override bool ManageAggressively
            {
                get { return false; }
            }
        }

        private class IndentCommand : TextEditingCommand
        {
            public override CommandId CommandId
            {
                get { return CommandId.Indent; }
            }

            protected override void Execute()
            {
                PostEditor.ApplyIndent();
            }

            public override void Manage()
            {
                if (Command.On)
                    Enabled = PostEditor.CanIndent;
            }
        }

        private class OutdentCommand : TextEditingCommand
        {
            public override CommandId CommandId
            {
                get { return CommandId.Outdent; }
            }

            protected override void Execute()
            {
                PostEditor.ApplyOutdent();
            }

            public override void Manage()
            {
                if (Command.On)
                    Enabled = PostEditor.CanOutdent;
            }
        }

        private class LTRTextBlockCommand : LatchedTextEditingCommand
        {
            public override CommandId CommandId { get { return CommandId.LTRTextBlock; } }

            protected override void Execute()
            {
                PostEditor.InsertLTRTextBlock();
                base.Execute();
            }

            public override void Manage()
            {
                bool rtlState = (PostEditor as ContentEditor).HasRTLFeatures || BidiHelper.IsRightToLeft;
                if (Command.On != rtlState)
                {
                    Command.On = rtlState;
                    PostEditor.CommandManager.OnChanged(EventArgs.Empty);
                }
                //latched is a semi-intensive check, so only do it if command is on/visible!
                if (Command.On)
                    Latched = PostEditor.SelectionIsLTR;
                Enabled = rtlState && PostEditor.CanApplyFormatting(CommandId);
            }
        }

        private class RTLTextBlockCommand : LatchedTextEditingCommand
        {
            public override CommandId CommandId { get { return CommandId.RTLTextBlock; } }

            protected override void Execute()
            {
                PostEditor.InsertRTLTextBlock();
                base.Execute();
            }

            public override void Manage()
            {
                bool rtlState = (PostEditor as ContentEditor).HasRTLFeatures || BidiHelper.IsRightToLeft;
                //Command.VisibleOnCommandBar = rtlState;
                if (Command.On != rtlState)
                {
                    Command.On = rtlState;
                    PostEditor.CommandManager.OnChanged(EventArgs.Empty);
                }
                //latched is a semi-intensive check, so only do it if command is on/visible!
                if (Command.On)
                    Latched = PostEditor.SelectionIsRTL;
                Enabled = rtlState && PostEditor.CanApplyFormatting(CommandId);
            }
        }

        private class BlockquoteCommand : LatchedTextEditingCommand
        {
            public override CommandId CommandId { get { return CommandId.Blockquote; } }

            protected override void Execute()
            {
                PostEditor.ApplyBlockquote();
                base.Execute();
            }

            public override void Manage()
            {
                Latched = PostEditor.SelectionBlockquoted;
                Enabled = PostEditor.CanApplyFormatting(CommandId);
                PostEditor.CommandManager.Invalidate(CommandId);
            }
        }

        private class InsertLinkCommand : TextEditingCommand
        {
            public override CommandId CommandId { get { return CommandId.InsertLink; } }

            protected override void Execute()
            {
                PostEditor.InsertLink();
            }

            public override void Manage()
            {
                Enabled = PostEditor.CanInsertLink;
            }
        }

        private class EditLinkCommand : TextEditingCommand
        {
            public override CommandId CommandId { get { return CommandId.EditLink; } }

            protected override void Execute()
            {
                PostEditor.InsertLink();
            }

            public override void Manage()
            {
            }

            // WinLive 276086: 'Edit hyperlink' context menu item shows up blank
            // A string was accidentally removed, it's too late now to add it back.
            // Fortunately, we have a duplicate string already in the resources.
            // Fix this in W5 MQ.
            public override string ContextMenuText
            {
                get
                {
                    return Res.Get(StringId.LinkEditHyperlink);
                }
            }
        }

        private class RemoveLinkAndClearFormattingCommand : TextEditingCommand
        {
            public override CommandId CommandId { get { return CommandId.RemoveLinkAndClearFormatting; } }

            protected override void Execute()
            {
                if (PostEditor.CanRemoveLink)
                    PostEditor.RemoveLink();
                if (PostEditor.CanApplyFormatting(CommandId.ClearFormatting))
                    PostEditor.ClearFormatting();
            }

            public override void Manage()
            {
                Enabled = PostEditor.CanRemoveLink || PostEditor.CanApplyFormatting(CommandId.ClearFormatting);
            }
        }

        private class RemoveLinkCommand : TextEditingCommand
        {
            public override CommandId CommandId { get { return CommandId.RemoveLink; } }

            protected override void Execute()
            {
                if (PostEditor.CanRemoveLink)
                    PostEditor.RemoveLink();
            }

            public override void Manage()
            {
                // tie enabled state to Insert Link -- it looks odd to have
                // Remove Link disabled on the command bar right next to
                // Insert Link -- the command no-ops in the case where it
                // is invalid for the current context (see Execute above)
                Enabled = PostEditor.CanInsertLink;
            }
        }

        private class AddToGlossaryCommand : TextEditingCommand
        {
            public override CommandId CommandId { get { return CommandId.AddToGlossary; } }

            protected override void Execute()
            {
                PostEditor.AddToGlossary();
            }

            public override void Manage()
            {
                // this command only appears on the context-menu for links,
                // so by default it is always enabled (if we don't do this
                // then it gets tied up in context-menu command management
                // funkiness, where sometimes it is enabled and sometimes
                // it is not
                Enabled = true;
            }
        }

        private class OpenLinkCommand : TextEditingCommand
        {
            public override CommandId CommandId { get { return CommandId.OpenLink; } }

            protected override void Execute()
            {
                PostEditor.OpenLink();
            }

            public override void Manage()
            {
            }
        }

        private class FindCommand : TextEditingCommand
        {
            public override CommandId CommandId { get { return CommandId.FindButton; } }

            protected override void Execute()
            {
                PostEditor.Find();
            }

            public override void Manage()
            {
                Enabled = PostEditor.CanFind;
            }
        }

        private class CheckSpellingCommand : TextEditingCommand
        {
            private bool _isExecuting;

            public override CommandId CommandId { get { return CommandId.CheckSpelling; } }

            protected override void Execute()
            {
                ExecuteWithArgs(new ExecuteEventHandlerArgs("OLECMDEXECOPT_DONTPROMPTUSER", "false"));
            }

            protected override void ExecuteWithArgs(ExecuteEventHandlerArgs args)
            {
                try
                {
                    if (!_isExecuting)
                    {
                        _isExecuting = true;

                        string doNotShow = args.GetString("OLECMDEXECOPT_DONTPROMPTUSER");
                        bool showFinishedUI = !StringHelper.ToBool(doNotShow, false);

                        if (!PostEditor.CheckSpelling())
                        {
                            args.Cancelled = true;
                            return;
                        }

                        if (showFinishedUI)
                        {
                            DisplayMessage.Show(MessageId.SpellCheckComplete, Owner);
                        }
                    }
                }
                finally
                {
                    _isExecuting = false;
                }

            }

            public override void Manage()
            {
                Enabled = !PostEditor.ReadOnly && Command.On;
            }

            public override Command CreateCommand()
            {
                return new DontPromptUserCommand(CommandId.CheckSpelling);
            }
        }

        internal class DontPromptUserCommand : Command
        {
            public DontPromptUserCommand(CommandId commandId)
                : base(commandId)
            {
            }

            public override int PerformExecute(CommandExecutionVerb verb, PropertyKeyRef key, PropVariantRef currentValue, IUISimplePropertySet commandExecutionProperties)
            {
                // Mail will call us with a special parameter sometimes
                // to let us know we should silence the spellchecker if possible
                Debug.Assert(CommandId == CommandId.CheckSpelling);
                if (commandExecutionProperties != null)
                {
                    PropVariant doNotPromptValue;
                    PropertyKey doNotPromptKey = new PropertyKey(3001, VarEnum.VT_BOOL);
                    int returnValue = commandExecutionProperties.GetValue(ref doNotPromptKey, out doNotPromptValue);
                    if (returnValue == 0)
                    {
                        ExecuteEventHandlerArgs eventArgs =
                            new ExecuteEventHandlerArgs("OLECMDEXECOPT_DONTPROMPTUSER",
                                                        doNotPromptValue.Value.ToString());
                        PerformExecuteWithArgs(eventArgs);

                        // The user cancelled, HRESULT.S_FALSE will tell Mail to stop sending the email
                        return eventArgs.Cancelled ? HRESULT.S_FALSE : HRESULT.S_OK;
                    }
                }
                return base.PerformExecute(verb, key, currentValue, commandExecutionProperties);
            }
        }

        private IWin32Window _owner;
        private IHtmlStylePicker _stylePicker;
        private IHtmlEditorCommandSource _postEditor;
        private TextEditingFocusHandler _focusCallback;
        private ArrayList _simpleTextEditors = new ArrayList();
        private IContainer components = new Container();
        private ArrayList _textEditingCommands = new ArrayList();
        private FontSizeCommand commandFontSize;
        private FontFamilyCommand commandFontFamily;
        private FontColorPickerCommand fontColorPickerCommand;
        private FontColorPickerCommand highlightColorPickerCommand;

        private Command _commandAlignLeft;
        private Command _commandAlignCenter;
        private Command _commandAlignRight;
        private Command _commandAlignJustify;

        /// <summary>
        /// Utility class used to make text editing command implementations fully self enclosed
        /// </summary>
        private abstract class TextEditingCommand
        {
            public TextEditingCommand()
            {
            }

            public void SetContext(TextEditingCommandDispatcher dispatcher, Command command)
            {
                _dispatcher = dispatcher;
                _command = command;
            }

            public virtual void OnAllCommandsInitialized() { }

            public abstract CommandId CommandId { get; }

            public virtual bool ManageAggressively { get { return false; } }

            public virtual string ContextMenuText { get { return null; } }

            public Command Command { get { return _command; } }

            public abstract void Manage();

            protected void ManageAll()
            {
                _dispatcher.ManageCommands();
            }

            public void Execute(object sender, EventArgs ea)
            {
                Execute();
                Manage();
            }

            public void ExecuteWithArgs(object sender, ExecuteEventHandlerArgs ea)
            {
                ExecuteWithArgs(ea);
                Manage();
            }

            protected abstract void Execute();
            protected virtual void ExecuteWithArgs(ExecuteEventHandlerArgs args)
            {
                // @RIBBON TODO: Unify the Execute and ExecuteWithArgs events.
                Execute();
            }

            protected bool Enabled { set { _command.Enabled = value; } }

            protected bool Latched { set { _command.Latched = value; } }

            protected ISimpleTextEditorCommandSource ActiveSimpleTextEditor { get { return _dispatcher.ActiveSimpleTextEditor; } }

            protected IHtmlEditorCommandSource PostEditor { get { return _dispatcher.PostEditor; } }

            protected IWin32Window Owner { get { return _dispatcher._owner; } }

            public virtual CommandContextMenuDefinition CommandBarButtonContextMenuDefinition
            {
                get { return null; }
            }

            protected Command FindCommand(CommandId commandId) { return _dispatcher.FindEditingCommand(commandId); }

            private TextEditingCommandDispatcher _dispatcher;
            private Command _command;

            public virtual Command CreateCommand()
            {
                return new OverridableCommand(CommandId);
            }
        }

        // Win Live 182580: Edit options in the ribbon like Font formatting etc should be disabled for photo albums
        // Here's the set of commands that we will disable when a photo album is selected.
        public static bool IsFontFormattingCommand(CommandId? id)
        {
            switch (id)
            {
                case CommandId.Bold:
                case CommandId.Italic:
                case CommandId.Underline:
                case CommandId.Strikethrough:
                case CommandId.Superscript:
                case CommandId.Subscript:
                case CommandId.FontColorPicker:
                case CommandId.FontBackgroundColor:
                case CommandId.ClearFormatting:
                case CommandId.FontSize:
                case CommandId.FontFamily:
                case CommandId.Numbers:
                case CommandId.Bullets:
                case CommandId.Indent:
                case CommandId.Outdent:
                    return true;
                default:
                    return false;
            }
        }
    }
}
