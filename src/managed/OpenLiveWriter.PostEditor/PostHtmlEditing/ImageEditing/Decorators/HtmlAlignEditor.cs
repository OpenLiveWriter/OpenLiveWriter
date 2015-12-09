// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Extensibility.ImageEditing;
using OpenLiveWriter.Localization;
using OpenLiveWriter.PostEditor.Commands;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.ImageEditing.Decorators
{
    public class HtmlAlignEditor : ImageDecoratorEditor
    {
        private IContainer components = null;

        private ImagePickerControl imagePickerAlign;

        public HtmlAlignEditor(CommandManager commandManager)
        {
            // This call is required by the Windows Form Designer.
            InitializeComponent();
            imagePickerAlign.AccessibleName = Res.Get(StringId.Alignment);
            imagePickerAlign.Items.AddRange(new object[] {
                new AlignmentComboItem(Res.Get(StringId.ImgSBAlignInline), ImgAlignment.NONE, ResourceHelper.LoadAssemblyResourceBitmap(BUTTON_IMAGE_PATH + "WrapTextInlineEnabled.png")),
                new AlignmentComboItem(Res.Get(StringId.ImgSBAlignLeft), ImgAlignment.LEFT, ResourceHelper.LoadAssemblyResourceBitmap(BUTTON_IMAGE_PATH + "WrapTextLeftEnabled.png")),
                new AlignmentComboItem(Res.Get(StringId.ImgSBAlignRight), ImgAlignment.RIGHT, ResourceHelper.LoadAssemblyResourceBitmap(BUTTON_IMAGE_PATH + "WrapTextRightEnabled.png")),
                new AlignmentComboItem(Res.Get(StringId.ImgSBAlignCenter), ImgAlignment.CENTER, ResourceHelper.LoadAssemblyResourceBitmap(BUTTON_IMAGE_PATH + "WrapTextCenterEnabled.png"))
                });
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
            this.SuspendLayout();

            this.imagePickerAlign = new ImagePickerControl();
            this.imagePickerAlign.Name = "imagePickerAlign";
            this.imagePickerAlign.Dock = DockStyle.Fill;
            this.imagePickerAlign.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.imagePickerAlign.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.imagePickerAlign.IntegralHeight = false;
            this.imagePickerAlign.ItemHeight = 29;
            this.imagePickerAlign.SelectedIndexChanged += new EventHandler(imagePickerAlign_SelectedIndexChanged);

            //
            // HtmlAlignEditor
            //
            this.Controls.Add(this.imagePickerAlign);
            this.Name = "HtmlAlignEditor";
            this.Size = new System.Drawing.Size(208, 35);

            this.ResumeLayout(false);
        }
        #endregion

        protected override void LoadEditor()
        {
            base.LoadEditor();
            HtmlAlignSettings = new HtmlAlignDecoratorSettings(EditorContext.Settings, EditorContext.ImgElement);
            Alignment = HtmlAlignSettings.Alignment;
        }
        private HtmlAlignDecoratorSettings HtmlAlignSettings;

        public override Size GetPreferredSize()
        {
            return new Size(208, 35);
        }

        protected override void OnSaveSettings()
        {
            HtmlAlignSettings.Alignment = Alignment;
            base.OnSaveSettings();

            FireAlignmentChanged();
        }

        public event EventHandler AlignmentChanged;
        protected void FireAlignmentChanged()
        {
            if (AlignmentChanged != null)
                AlignmentChanged(this, EventArgs.Empty);
        }

        public ImgAlignment Alignment
        {
            get
            {
                return alignment;
            }
            private set
            {
                alignment = value;
                switch (value)
                {
                    case ImgAlignment.NONE:
                        imagePickerAlign.SelectedIndex = 0;
                        break;
                    case ImgAlignment.LEFT:
                        imagePickerAlign.SelectedIndex = 1;
                        break;
                    case ImgAlignment.RIGHT:
                        imagePickerAlign.SelectedIndex = 2;
                        break;
                    case ImgAlignment.CENTER:
                        imagePickerAlign.SelectedIndex = 3;
                        break;
                }
                SaveSettingsAndApplyDecorator();
            }
        }
        ImgAlignment alignment;

        public const string BUTTON_IMAGE_PATH = "PostHtmlEditing.ImageEditing.Images.";

        private void imagePickerAlign_SelectedIndexChanged(object sender, EventArgs e)
        {
            AlignmentComboItem selectedAlignment = imagePickerAlign.SelectedItem as AlignmentComboItem;
            if (selectedAlignment != null)
                Alignment = (ImgAlignment)selectedAlignment.ItemValue;
        }

        private class AlignmentComboItem : OptionItem, ImagePickerControl.IComboImageItem
        {
            public AlignmentComboItem(string text, ImgAlignment alignment, Image borderImage) : base(text, alignment)
            {
                image = borderImage;
            }
            #region ImageBorderItem Members

            public Image Image
            {
                get
                {
                    return image;
                }
            }
            private Image image;
            #endregion
        }
    }

    internal class OptionItem
    {
        internal string Text;
        internal object ItemValue;
        public OptionItem(string text, object val)
        {
            Text = text;
            ItemValue = val;
        }

        public override bool Equals(object obj)
        {
            if (obj is OptionItem)
            {
                OptionItem size = (OptionItem)obj;
                return size.ItemValue.Equals(ItemValue);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return ItemValue.GetHashCode();
        }

        public override string ToString()
        {
            return Text;
        }
    }
}

