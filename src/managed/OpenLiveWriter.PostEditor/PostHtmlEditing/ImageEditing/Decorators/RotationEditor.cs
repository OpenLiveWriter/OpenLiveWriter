// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Extensibility.ImageEditing;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.ImageEditing.Decorators
{
    public class RotationEditor : ImageDecoratorEditor
    {
        private IContainer components = null;

        public RotationEditor()
        {
            Res.Get("RotationEditor");

            // This call is required by the Windows Form Designer.
            InitializeComponent();

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
            this.buttonClockwise = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // buttonClockwise
            //
            this.buttonClockwise.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonClockwise.Location = new System.Drawing.Point(0, 0);
            this.buttonClockwise.Name = "buttonClockwise";
            this.buttonClockwise.Size = new System.Drawing.Size(108, 23);
            this.buttonClockwise.TabIndex = 0;
            this.buttonClockwise.Text = "Rotate Clockwise";
            this.buttonClockwise.Click += new System.EventHandler(this.buttonClockwise_Click);
            //
            // RotationEditor
            //
            this.Controls.Add(this.buttonClockwise);
            this.Name = "RotationEditor";
            this.Size = new System.Drawing.Size(220, 56);
            this.ResumeLayout(false);

        }
        #endregion

        protected override void LoadEditor()
        {
            base.LoadEditor();
        }

        private Button buttonClockwise;

        public override Size GetPreferredSize()
        {
            return new Size(244, 48);
        }

        private void buttonClockwise_Click(object sender, EventArgs e)
        {

            RotateFlipType newRotation = ImageDecoratorUtils.GetFlipTypeRotatedCW(EditorContext.ImageRotation);
            EditorContext.ImageRotation = newRotation;

            using (new WaitCursor())
            {
                EditorContext.ApplyDecorator();
            }
        }
    }
}

