// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using OpenLiveWriter.BlogClient;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.HtmlEditor;
using OpenLiveWriter.HtmlEditor.Linking;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Extensibility.ImageEditing;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Localization.Bidi;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.ImageEditing.Decorators
{
    public class HtmlImageTargetEditor : ImageDecoratorEditor
    {
        private IContainer components = null;

        public HtmlImageTargetEditor()
        {
            // This call is required by the Windows Form Designer.
            InitializeComponent();
            this.buttonTargetOptions.Text = Res.Get(StringId.OptionsButton);

            imageFileSummaryFormat = TextFormatFlags.SingleLine | TextFormatFlags.ExpandTabs | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix;

            imageFileSizeFormat = TextFormatFlags.SingleLine | TextFormatFlags.ExpandTabs | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix;

            comboBoxLinkTargets.AccessibleName = ControlHelper.ToAccessibleName(Res.Get(StringId.LinkLinkTo));
        }
        TextFormatFlags imageFileSummaryFormat;
        private Button buttonTargetOptions;
        TextFormatFlags imageFileSizeFormat;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            DisplayHelper.AutoFitSystemButton(buttonTargetOptions);
            // HACK: for some reason this button scales particularly inaccurately
            int extraPadding = (int)DisplayHelper.ScaleX(6);
            buttonTargetOptions.Width += extraPadding;
            buttonTargetOptions.Left -= extraPadding;

            using (new AutoGrow(this, AnchorStyles.Bottom, false))
            {
                LayoutHelper.FitControlsBelow(3, comboBoxLinkTargets);
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // HACK: 722879
            // THis button does not always run its 'clicked' event when it has focus and you press enter, space does
            // always seem to work though.
            if (buttonTargetOptions.Focused && keyData == Keys.Enter)
            {
                buttonTargetOptions.PerformClick();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
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

        #region Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.buttonTargetOptions = new System.Windows.Forms.Button();
            this.comboBoxLinkTargets = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            //
            // buttonTargetOptions
            //
            this.buttonTargetOptions.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonTargetOptions.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonTargetOptions.Location = new System.Drawing.Point(167, 35);
            this.buttonTargetOptions.Name = "buttonTargetOptions";
            this.buttonTargetOptions.Size = new System.Drawing.Size(66, 22);
            this.buttonTargetOptions.TabIndex = 1;
            this.buttonTargetOptions.Text = "Options...";
            this.buttonTargetOptions.Click += new System.EventHandler(this.buttonTargetOptions_Click);
            //
            // comboBoxLinkTargets
            //
            this.comboBoxLinkTargets.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxLinkTargets.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxLinkTargets.Location = new System.Drawing.Point(0, 0);
            this.comboBoxLinkTargets.Name = "comboBoxLinkTargets";
            this.comboBoxLinkTargets.Size = new System.Drawing.Size(232, 21);
            this.comboBoxLinkTargets.TabIndex = 0;
            this.comboBoxLinkTargets.SelectedIndexChanged += new System.EventHandler(this.comboBoxLinkTargets_SelectedIndexChanged);
            //
            // HtmlImageTargetEditor
            //
            this.Controls.Add(this.buttonTargetOptions);
            this.Controls.Add(this.comboBoxLinkTargets);
            this.Name = "HtmlImageTargetEditor";
            this.Size = new System.Drawing.Size(232, 118);
            this.ResumeLayout(false);

        }
        #endregion

        protected override void LoadEditor()
        {
            base.LoadEditor();
            HtmlImageTargetSettings = new HtmlImageTargetDecoratorSettings(EditorContext.Settings, EditorContext.ImgElement);
            LoadLinkTargetsCombo();

            string imgUrl = (string)EditorContext.ImgElement.getAttribute("src", 2);
            LinkToSourceImageEnabled = UrlHelper.IsFileUrl(imgUrl) && GlobalEditorOptions.SupportsFeature(ContentEditorFeature.SupportsImageClickThroughs);

            if (ImageEditingContext.EditorOptions.DhtmlImageViewer != null)
            {
                imageViewer = DhtmlImageViewers.GetImageViewer(ImageEditingContext.EditorOptions.DhtmlImageViewer);
            }
        }
        private ComboBox comboBoxLinkTargets;
        private HtmlImageTargetDecoratorSettings HtmlImageTargetSettings;

        public override Size GetPreferredSize()
        {
            return new Size(264, 176);
        }

        protected override void OnSaveSettings()
        {
            //NOTE: the settings for this control are changed directly in the
            //comboBoxLinkTargets_SelectedIndexChanged method, which isn't
            //really correct and needs to be changed so that we persist settings
            //here instead.
            base.OnSaveSettings();
        }

        private LinkTargetType SelectedLinkTarget
        {
            get
            {
                OptionItem option = (OptionItem)comboBoxLinkTargets.SelectedItem;
                if (option != null)
                {
                    LinkTargetType targetType = (LinkTargetType)option.ItemValue;
                    return targetType;
                }
                return LinkTargetType.NONE;
            }
        }

        private void LoadLinkTargetsCombo()
        {
            LinkTargetType targetType = HtmlImageTargetSettings.LinkTarget;
            this.comboBoxLinkTargets.Items.Clear();
            if (LinkToSourceImageEnabled)
                comboBoxLinkTargets.Items.Add(new OptionItem(Res.Get(StringId.LinkToSource), LinkTargetType.IMAGE));
            comboBoxLinkTargets.Items.Add(new OptionItem(Res.Get(StringId.LinkToURL), LinkTargetType.URL));
            comboBoxLinkTargets.Items.Add(new OptionItem(Res.Get(StringId.LinkToNone), LinkTargetType.NONE));

            comboBoxLinkTargets.SelectedItem = new OptionItem("", targetType);

            //comboBoxLinkTargets.Visible = comboBoxLinkTargets.Items.Count > 1;
        }

        private void ClearTargetSummaryLabel()
        {
            linkToSummaryText = null;
            linkToSizeText = null;
            Invalidate();
        }

        private void LoadTargetSummaryLabel()
        {
            if (HtmlImageTargetSettings.LinkTarget == LinkTargetType.IMAGE)
            {
                linkToSummaryText = Path.GetFileName(EditorContext.SourceImageUri.LocalPath);
                linkToSizeText = String.Format(CultureInfo.CurrentCulture, Res.Get(StringId.DimensionsFormat), HtmlImageTargetSettings.ImageSize.Width, HtmlImageTargetSettings.ImageSize.Height);
                linkToImageViewer = "";
                if (HtmlImageTargetSettings.DhtmlImageViewer != null && HtmlImageTargetSettings.DhtmlImageViewer == ImageEditingContext.EditorOptions.DhtmlImageViewer)
                {
                    if (HtmlImageTargetSettings.LinkOptions.UseImageViewer && HtmlImageTargetSettings.DhtmlImageViewer != "Windows Live Spaces")
                    {
                        string viewerName = DhtmlImageViewers.GetLocalizedName(HtmlImageTargetSettings.DhtmlImageViewer);

                        if (!string.IsNullOrEmpty(HtmlImageTargetSettings.LinkOptions.ImageViewerGroupName))
                        {
                            linkToImageViewer =
                                string.Format(CultureInfo.InvariantCulture,
                                              Res.Get(StringId.ImageViewerDisplayFormatGroup),
                                              viewerName,
                                              HtmlImageTargetSettings.LinkOptions.ImageViewerGroupName);
                        }
                        else
                        {
                            linkToImageViewer =
                                string.Format(CultureInfo.InvariantCulture,
                                              Res.Get(StringId.ImageViewerDisplayFormatSingle),
                                              viewerName);
                        }
                    }
                }
            }
            else if (HtmlImageTargetSettings.LinkTarget == LinkTargetType.URL)
            {
                linkToImageViewer = String.Empty;
                linkToSummaryText = HtmlImageTargetSettings.LinkTargetUrl;
                if (UrlHelper.IsUrl(linkToSummaryText))
                {
                    try
                    {
                        //attempt to shorten the URI string into a path-ellipsed format.
                        Uri sourceUri = new Uri(HtmlImageTargetSettings.LinkTargetUrl);
                        linkToSummaryText = String.Format(CultureInfo.InvariantCulture, "{0}://{1}", sourceUri.Scheme, sourceUri.Host);
                        string[] segments = sourceUri.Segments;
                        if (segments.Length > 2)
                        {
                            linkToSummaryText += "/...";
                            if (segments[segments.Length - 2].EndsWith("/"))
                                linkToSummaryText += "/";
                            linkToSummaryText += segments[segments.Length - 1];
                        }
                        else
                            linkToSummaryText += String.Join("", segments);

                        if (sourceUri.Query != null)
                            linkToSummaryText += sourceUri.Query;
                    }
                    catch (Exception) { }
                }

                linkToSizeText = null;
            }
            else
            {
                linkToSummaryText = null;
                linkToSizeText = null;
            }

            PerformLayout();
            Invalidate();
        }
        string linkToSummaryText = String.Empty;
        string linkToSizeText = "1024x768";
        string linkToImageViewer = String.Empty;

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);

            int summaryY = buttonTargetOptions.Bottom + ScaleY(summaryTextTopMargin);
            summaryTextRect = new Rectangle(
                0, summaryY,
                ClientRectangle.Width,
                ClientRectangle.Height - summaryY);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            BidiGraphics g = new BidiGraphics(e.Graphics, summaryTextRect);

            //paint the background over the last text paint
            using (Brush backBrush = new SolidBrush(BackColor))
            {
                g.FillRectangle(backBrush, summaryTextRect);
            }

            Color textColor = Color.DarkGray;
            Size sizeTextSize;
            if (linkToSizeText != null)
                sizeTextSize = g.MeasureText(linkToSizeText, Font, summaryTextRect.Size, imageFileSizeFormat);
            else
                sizeTextSize = Size.Empty;

            Size summaryTextSize = g.MeasureText(linkToSummaryText, Font, new Size(summaryTextRect.Size.Width - sizeTextSize.Width - ScaleX(summarySizeSpacing), sizeTextSize.Height), imageFileSummaryFormat);

            Rectangle summaryTextLayoutRect = new Rectangle(new Point(summaryTextRect.X, summaryTextRect.Y), summaryTextSize);
            Rectangle sizeTextLayoutRect = new Rectangle(new Point(summaryTextLayoutRect.Right + summarySizeSpacing, summaryTextRect.Y), sizeTextSize);
            Rectangle imageViewerTextLayoutRect = new Rectangle(summaryTextRect.X, sizeTextLayoutRect.Bottom + ScaleY(3), ClientRectangle.Width, (int)Math.Ceiling(Font.GetHeight(e.Graphics)));
            g.DrawText(linkToSummaryText, this.Font, summaryTextLayoutRect, textColor, imageFileSummaryFormat);
            g.DrawText(linkToSizeText, this.Font, sizeTextLayoutRect, textColor, imageFileSummaryFormat);
            if (linkToImageViewer != "")
                g.DrawText(linkToImageViewer, this.Font, imageViewerTextLayoutRect, textColor, imageFileSummaryFormat);
        }
        Rectangle summaryTextRect;
        private const int summaryTextTopMargin = 10;
        private const int summarySizeSpacing = 5;

        public bool LinkToSourceImageEnabled
        {
            get
            {
                return _linkToSourceImageEnabled;
            }
            set
            {
                if (_linkToSourceImageEnabled != value)
                {
                    _linkToSourceImageEnabled = value;
                    LoadLinkTargetsCombo();
                }
            }
        }

        internal IBlogPostImageEditingContext ImageEditingContext
        {
            get { return imageEditingContext; }
            set { imageEditingContext = value; }
        }

        public bool _linkToSourceImageEnabled;

        private void comboBoxLinkTargets_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (suppressComboBoxLinkTargetsSelectionChanged)
                return;
            if (EditorState == ControlState.Loaded)
            {
                using (new WaitCursor())
                {
                    using (IImageDecoratorUndoUnit undo = EditorContext.CreateUndoUnit())
                    {
                        bool commitChanges = true;
                        LinkTargetType target = SelectedLinkTarget;
                        if (target == LinkTargetType.URL && !comboBoxLinkTargets.DroppedDown)
                        {
                            ClearTargetSummaryLabel();
                            commitChanges = EditTargetOptions() == DialogResult.OK;
                        }

                        if (commitChanges)
                        {
                            HtmlImageTargetSettings.LinkTarget = target;
                            SaveSettingsAndApplyDecorator();

                            //note: this is optionally committed because the EditTargetOptions() method
                            //makes changes to the settings that persist in the DOM.  This is bad and we should
                            //stop doing that, and save in the OnSaveSettings() method instead. Then we won't
                            //need this stupid optional commit.
                            undo.Commit();
                        }
                        else
                        {
                            //rollback to the previous selected item
                            suppressComboBoxLinkTargetsSelectionChanged = true;
                            try
                            {
                                comboBoxLinkTargets.SelectedItem = new OptionItem("", HtmlImageTargetSettings.LinkTarget);
                            }
                            finally
                            {
                                suppressComboBoxLinkTargetsSelectionChanged = false;
                            }
                        }
                    }
                }
            }

            LoadTargetSummaryLabel();
            buttonTargetOptions.Enabled = !SelectedLinkTarget.Equals(LinkTargetType.NONE);
        }
        private bool suppressComboBoxLinkTargetsSelectionChanged = false;
        private IBlogPostImageEditingContext imageEditingContext;
        private ImageViewer imageViewer;

        private void buttonTargetOptions_Click(object sender, EventArgs e)
        {
            using (new WaitCursor())
            {
                using (IImageDecoratorUndoUnit undo = EditorContext.CreateUndoUnit())
                {
                    if (EditTargetOptions() == DialogResult.OK)
                    {
                        SaveSettingsAndApplyDecorator();
                        LoadTargetSummaryLabel();
                        undo.Commit();
                    }
                }
            }
        }

        private DialogResult EditTargetOptions()
        {
            using (LinkToOptionsForm linkOptionsForm = new LinkToOptionsForm())
            {
                if (SelectedLinkTarget == LinkTargetType.IMAGE)
                {
                    using (ImageTargetEditorControl editor = new ImageTargetEditorControl())
                    {
                        editor.LoadImageSize(HtmlImageTargetSettings.ImageSize, EditorContext.SourceImageSize, EditorContext.ImageRotation);
                        editor.LinkOptions = HtmlImageTargetSettings.LinkOptions;
                        editor.EditorOptions = ImageEditingContext.EditorOptions;
                        linkOptionsForm.EditorControl = editor;

                        HtmlImageTargetSettings.DhtmlImageViewer = ImageEditingContext.EditorOptions.DhtmlImageViewer;

                        DialogResult result = linkOptionsForm.ShowDialog(this);
                        if (result == DialogResult.OK)
                        {
                            HtmlImageTargetSettings.ImageSize = editor.ImageSize;
                            HtmlImageTargetSettings.DhtmlImageViewer = ImageEditingContext.EditorOptions.DhtmlImageViewer;
                            HtmlImageTargetSettings.LinkOptions = editor.LinkOptions;
                            HtmlImageTargetSettings.ImageSizeName = editor.ImageBoundsSize;
                        }
                        return result;
                    }
                }
                else if (SelectedLinkTarget == LinkTargetType.URL)
                {
                    using (HyperlinkForm hyperlinkForm = new HyperlinkForm(EditorContext.CommandManager, GlobalEditorOptions.SupportsFeature(ContentEditorFeature.ShowAllLinkOptions)))
                    {
                        hyperlinkForm.ContainsImage = true;
                        hyperlinkForm.EditStyle = HtmlImageTargetSettings.LinkTargetUrl != null && HtmlImageTargetSettings.LinkTargetUrl != String.Empty;
                        hyperlinkForm.NewWindow = HtmlImageTargetSettings.LinkOptions.ShowInNewWindow;
                        if (HtmlImageTargetSettings.LinkTitle != String.Empty)
                            hyperlinkForm.LinkTitle = HtmlImageTargetSettings.LinkTitle;
                        if (HtmlImageTargetSettings.LinkRel != String.Empty)
                            hyperlinkForm.Rel = HtmlImageTargetSettings.LinkRel;
                        if (HtmlImageTargetSettings.LinkTargetUrl != null && HtmlImageTargetSettings.LinkTarget != LinkTargetType.IMAGE)
                        {
                            hyperlinkForm.Hyperlink = HtmlImageTargetSettings.LinkTargetUrl;
                        }

                        DialogResult result = hyperlinkForm.ShowDialog(FindForm());
                        if (result == DialogResult.OK)
                        {
                            HtmlImageTargetSettings.LinkTargetUrl = hyperlinkForm.Hyperlink;
                            HtmlImageTargetSettings.UpdateImageLinkOptions(hyperlinkForm.LinkTitle, hyperlinkForm.Rel, hyperlinkForm.NewWindow);
                            HtmlImageTargetSettings.LinkOptions = new LinkOptions(hyperlinkForm.NewWindow, false, null);
                        }
                        return result;
                    }
                }

                return DialogResult.Abort;
            }
        }
    }
}
