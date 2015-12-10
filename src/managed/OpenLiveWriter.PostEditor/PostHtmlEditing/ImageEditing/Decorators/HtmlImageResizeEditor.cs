// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using mshtml;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.Extensibility.ImageEditing;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.ImageEditing.Decorators
{
    public class HtmlImageResizeEditor : ImageDecoratorEditor
    {
        public class SizeCommand
        {
            private CommandId _widthId;
            private CommandId _heightId;

            private readonly Size MinSize = new Size(1, 1);
            private readonly Size MaxSize = new Size(9999, 9999);
            private const int Increment = 1;
            private const uint DecimalPlaces = 0;
            private const string RepresentativeString = "9999"; // @RIBBON TODO: Should be localizable
            private const string FormatString = "";

            private Size sizeValue;
            public SizeCommand(CommandManager commandManager, CommandId widthId, CommandId heightId, int width, int height)
            {
                _widthId = widthId;
                _heightId = heightId;

                sizeValue = new Size(width, height);

                commandManager.BeginUpdate();

                commandManager.Add(
                    new SpinnerCommand(_widthId, MinSize.Width, MaxSize.Width, width, Increment, DecimalPlaces, RepresentativeString, FormatString),
                    command_ExecuteWithArgs);

                commandManager.Add(
                    new SpinnerCommand(_heightId, MinSize.Height, MaxSize.Height, height, Increment, DecimalPlaces, RepresentativeString, FormatString),
                    command_ExecuteWithArgs);

                commandManager.EndUpdate();
            }

            public Size Value
            {
                get { return sizeValue; }
                set
                {
                    sizeValue = value;
                    FireSizeChanged();
                }
            }

            void command_ExecuteWithArgs(object sender, ExecuteEventHandlerArgs args)
            {
                Command command = (Command)sender;
                Debug.Assert(command.CommandId == _widthId || command.CommandId == _heightId);

                int value = Convert.ToInt32(args.GetDecimal(command.CommandId.ToString()));

                if (command.CommandId == _widthId)
                {
                    sizeValue.Width = value;
                }
                else if (command.CommandId == _heightId)
                {
                    sizeValue.Height = value;
                }

                FireSizeChanged();
            }

            public event EventHandler SizeChanged;
            private void FireSizeChanged()
            {
                if (SizeChanged != null)
                    SizeChanged(this, EventArgs.Empty);
            }
        }

        private SizeCommand sizeCommand;
        private IContainer components = null;
        protected ImageSizeControl imageSizeControl;

        public HtmlImageResizeEditor(CommandManager commandManager)
        {
            sizeCommand = new SizeCommand(commandManager, CommandId.FormatImageAdjustWidth, CommandId.FormatImageAdjustHeight, 1, 1);
            sizeCommand.SizeChanged += new EventHandler(imageSizeControl_ImageSizeChanged);

            // This call is required by the Windows Form Designer.
            InitializeComponent();

            //mnemonics are not supported in the sidebar (since they interfere
            //with typing in the editor when displayed, or with the main menu if they
            //conflict.
            imageSizeControl.RemoveMnemonics();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Height = imageSizeControl.PreferredHeight;
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

                sizeCommand.SizeChanged -= new EventHandler(imageSizeControl_ImageSizeChanged);
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
            this.imageSizeControl = new OpenLiveWriter.PostEditor.PostHtmlEditing.ImageSizeControl();
            this.SuspendLayout();
            //
            // imageSizeControl
            //
            this.imageSizeControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.imageSizeControl.Location = new System.Drawing.Point(0, 0);
            this.imageSizeControl.Name = "imageSizeControl";
            this.imageSizeControl.Size = new System.Drawing.Size(244, 100);
            this.imageSizeControl.TabIndex = 1;

            // @RIBBON TODO: Use the logic that actually does the resizing in ImageSizeControl

            this.imageSizeControl.ImageSizeChanged += new System.EventHandler(this.imageSizeControl_ImageSizeChanged);
            //
            // HtmlImageResizeEditor
            //
            this.Controls.Add(this.imageSizeControl);
            this.Name = "HtmlImageResizeEditor";
            this.Size = new System.Drawing.Size(244, 100);
            this.ResumeLayout(false);

        }
        #endregion

        protected override void LoadEditor()
        {
            base.LoadEditor();

            ImageResizeSettings = new HtmlImageResizeDecoratorSettings(Settings, EditorContext.ImgElement);
            Size imageSize = ImageResizeSettings.ImageSize;

            sizeCommand.Value = imageSize;

            if (EditorContext.SourceImageSize == new Size(Int32.MaxValue, Int32.MaxValue))
            {
                //The source image size is unknown, so calculate the actual image size by removing
                //the size attributes, checking the size, and then placing the size attributes back
                object oldHeight = EditorContext.ImgElement.getAttribute("height", 2);
                object oldWidth = EditorContext.ImgElement.getAttribute("width", 2);
                EditorContext.ImgElement.removeAttribute("width", 0);
                EditorContext.ImgElement.removeAttribute("height", 0);
                int width = ((IHTMLImgElement)EditorContext.ImgElement).width;
                int height = ((IHTMLImgElement)EditorContext.ImgElement).height;

                if (oldHeight != null)
                    EditorContext.ImgElement.setAttribute("height", oldHeight, 0);
                if (oldWidth != null)
                    EditorContext.ImgElement.setAttribute("width", oldWidth, 0);
                imageSizeControl.LoadImageSize(imageSize, new Size(width, height), EditorContext.ImageRotation);
            }
            else
            {
                imageSizeControl.LoadImageSize(imageSize, EditorContext.SourceImageSize, EditorContext.ImageRotation);
            }
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
        }

        internal HtmlImageResizeDecoratorSettings ImageResizeSettings;

        public override Size GetPreferredSize()
        {
            return new Size(244, 76);
        }

        protected override void OnSaveSettings()
        {
            ImageResizeSettings.SetImageSize(imageSizeControl.ImageSize, imageSizeControl.ImageBoundsSize);
        }

        private void imageSizeControl_ImageSizeChanged(object sender, EventArgs e)
        {
            SaveSettingsAndApplyDecorator();
        }
    }
}

