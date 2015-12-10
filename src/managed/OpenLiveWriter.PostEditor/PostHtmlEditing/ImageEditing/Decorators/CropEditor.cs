// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Extensibility.ImageEditing;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.ImageEditing.Decorators
{
    public partial class CropEditor : ImageDecoratorEditor
    {
        private CropDecoratorSettings.StateToken originalState;
        private Bitmap bitmap;

        public CropEditor()
        {
            InitializeComponent();

            Bitmap rotateFrameBitmap =
                ResourceHelper.LoadAssemblyResourceBitmap("PostHtmlEditing.ImageEditing.Images.RotateFrame.png");
            Bitmap rotateFrameDisabled = ImageHelper.MakeDisabled(rotateFrameBitmap);
            buttonRotate.Initialize(rotateFrameBitmap, rotateFrameDisabled);

            lblAspectRatio.Text = Res.Get(StringId.CropAspectRatioLabel);
            buttonRotate.Text = Res.Get(StringId.CropRotateFrame);
            buttonOK.Text = Res.Get(StringId.OKButtonText);
            buttonCancel.Text = Res.Get(StringId.CancelButton);
            chkGrid.Text = Res.Get(StringId.CropShowGridlines);
            btnRemoveCrop.Text = Res.Get(StringId.CropRemoveCrop);

            cbAspectRatio.AccessibleName = ControlHelper.ToAccessibleName(Res.Get(StringId.CropAspectRatioLabel));

            imageCropControl.CropRectangleChanged += imageCropControl_CropRectangleChanged;
            imageCropControl.AspectRatioChanged += imageCropControl_AspectRatioChanged;

            cbAspectRatio.Items.Add(new AspectRatioItem("Original", Res.Get(StringId.CropAspectOriginal), null));
            cbAspectRatio.Items.Add(new AspectRatioItem("Custom", Res.Get(StringId.CropAspectCustom), null));
            cbAspectRatio.Items.Add(new AspectRatioItem("16x9", Res.Get(StringId.CropAspect16x9), 16.0 / 9.0));
            cbAspectRatio.Items.Add(new AspectRatioItem("8.5x11", Res.Get(StringId.CropAspect85x11), 11.0 / 8.5));
            cbAspectRatio.Items.Add(new AspectRatioItem("8x10", Res.Get(StringId.CropAspect8x10), 10.0 / 8.0));
            cbAspectRatio.Items.Add(new AspectRatioItem("5x7", Res.Get(StringId.CropAspect5x7), 7.0 / 5.0));
            cbAspectRatio.Items.Add(new AspectRatioItem("4x6", Res.Get(StringId.CropAspect4x6), 6.0 / 4.0));
            cbAspectRatio.Items.Add(new AspectRatioItem("4x3", Res.Get(StringId.CropAspect4x3), 4.0 / 3.0));
            cbAspectRatio.Items.Add(new AspectRatioItem("3.5x5", Res.Get(StringId.CropAspect35x5), 5 / 3.5));
            cbAspectRatio.Items.Add(new AspectRatioItem("Square", Res.Get(StringId.CropAspectSquare), 1.0));

            Text = Res.Get(StringId.CropDialogTitle);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Control | Keys.Up:
                case Keys.Control | Keys.Down:
                case Keys.Control | Keys.Left:
                case Keys.Control | Keys.Right:
                case Keys.Shift | Keys.Up:
                case Keys.Shift | Keys.Down:
                case Keys.Shift | Keys.Left:
                case Keys.Shift | Keys.Right:
                    if (imageCropControl.ProcessCommandKey(ref msg, keyData))
                    {
                        imageCropControl.Select();
                        return true;
                    }
                    break;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void OnLoad(EventArgs e)
        {
            Parent.Dock = DockStyle.Fill;

            Form form = FindForm();
            form.AcceptButton = buttonOK;
            form.CancelButton = buttonCancel;
            form.MinimumSize = new Size(536, 320);
            form.ShowIcon = false;
            //form.MaximizeBox = true;
            form.Closing += delegate
                                {
                                    if (form.DialogResult == DialogResult.Cancel)
                                    {
                                        originalState.Restore();
                                        EditorContext.ApplyDecorator();
                                    }
                                    else
                                    {
                                        // This forces the linked image to be updated as well
                                        SaveSettingsAndApplyDecorator();
                                    }
                                };

            base.OnLoad(e);

            if (EditorContext.EnforcedAspectRatio == null)
            {
                int width = cbAspectRatio.Left - lblAspectRatio.Right;

                DisplayHelper.AutoFitSystemLabel(lblAspectRatio, 0, int.MaxValue);

                DisplayHelper.AutoFitSystemCombo(cbAspectRatio, 0, int.MaxValue, false);
                buttonRotate.Width = buttonRotate.GetPreferredWidth();

                bool isButtonRotateVisible = buttonRotate.Visible;
                buttonRotate.Visible = true;
                LayoutHelper.DistributeHorizontally(width, lblAspectRatio, cbAspectRatio, buttonRotate);
                buttonRotate.Visible = isButtonRotateVisible;
                if (isButtonRotateVisible && (cbAspectRatio.Height + 2) > buttonRotate.Height)
                {
                    buttonRotate.Height = cbAspectRatio.Height + 2;
                }
            }
            else
            {
                lblAspectRatio.Visible =
                    cbAspectRatio.Visible =
                    buttonRotate.Visible = false;
            }

            DisplayHelper.AutoFitSystemCheckBox(chkGrid, 0, int.MaxValue);
            DisplayHelper.AutoFitSystemButton(btnRemoveCrop);
            LayoutHelper.FixupOKCancel(buttonOK, buttonCancel);
            chkGrid.Left = buttonCancel.Right - chkGrid.Width;

            panel1.Height = Math.Max(buttonRotate.Bottom, cbAspectRatio.Bottom) + 3;
            imageCropControl.Top = panel1.Bottom;

            imageCropControl.Select();

            //int minWidth = buttonRotate.Right + width + (form.ClientSize.Width - buttonOK.Left) + SystemInformation.FrameBorderSize.Width * 2;
            //form.MinimumSize = new Size(minWidth, form.MinimumSize.Height);
        }

        public override Size GetPreferredSize()
        {
            return new Size(640, 480);
        }

        public override FormBorderStyle FormBorderStyle
        {
            get
            {
                return FormBorderStyle.Sizable;
            }
        }

        protected override void LoadEditor()
        {
            bitmap = (Bitmap)State;
            Size origSize = bitmap.Size;
            bitmap.RotateFlip(EditorContext.ImageRotation);
            imageCropControl.Bitmap = bitmap;
            ((AspectRatioItem)cbAspectRatio.Items[0]).AspectRatio = bitmap.Width / (double)bitmap.Height;

            CropDecoratorSettings settings = new CropDecoratorSettings(Settings);
            originalState = settings.CreateStateToken();

            Rectangle? cropRectangle = settings.CropRectangle;

            if (EditorContext.EnforcedAspectRatio != null)
            {
                Rectangle rect;
                if (cropRectangle != null)
                    rect = RectangleHelper.RotateFlip(origSize, cropRectangle.Value, EditorContext.ImageRotation);
                else
                    rect = new Rectangle(Point.Empty, bitmap.Size);

                rect = RectangleHelper.EnforceAspectRatio(rect, EditorContext.EnforcedAspectRatio.Value);
                imageCropControl.CropRectangle = rect;
                imageCropControl.AspectRatio = EditorContext.EnforcedAspectRatio.Value;
            }
            else if (cropRectangle != null)
            {
                string savedAspectRatioId = settings.AspectRatioId;
                double? savedAspectRatio = settings.AspectRatio;
                if (savedAspectRatioId != null)
                {
                    foreach (AspectRatioItem item in cbAspectRatio.Items)
                    {
                        if (item.Id == savedAspectRatioId)
                        {
                            // doubles can't be accurately compared after they've been round-tripped
                            // to strings, due to lossy conversion to/from strings.
                            if ((float)(item.AspectRatio ?? 0.0) != (float)(savedAspectRatio ?? 0.0))
                            {
                                if (item.AspectRatio == null || savedAspectRatio == null)
                                    continue;
                                if (item.AspectRatio.Value != 1 / savedAspectRatio.Value)
                                    continue;
                            }

                            cbAspectRatio.SelectedItem = item;
                            imageCropControl.AspectRatio = savedAspectRatio;
                            break;
                        }
                    }
                }
                if (cbAspectRatio.SelectedIndex == -1)
                    cbAspectRatio.SelectedIndex = 1;
                imageCropControl.CropRectangle = RectangleHelper.RotateFlip(origSize, cropRectangle.Value, EditorContext.ImageRotation);
            }
            else
            {
                cbAspectRatio.SelectedIndex = 1;
                UpdatePreview(true);
            }
        }

        private void UpdatePreview(bool forced)
        {
            using (ImageDecoratorDirective.SuppressLinked())
            {
                SaveSettingsAndApplyDecorator(forced);
            }
        }

        protected override void OnSaveSettings()
        {
            base.OnSaveSettings();
            CropDecoratorSettings settings = new CropDecoratorSettings(Settings);
            if (imageCropControl.Enabled)
            {
                settings.CropRectangle = RectangleHelper.UndoRotateFlip(imageCropControl.Bitmap.Size, imageCropControl.CropRectangle, EditorContext.ImageRotation);
                settings.AspectRatioId = ((AspectRatioItem)(cbAspectRatio.SelectedItem ?? cbAspectRatio.Items[1])).Id;
                settings.AspectRatio = imageCropControl.AspectRatio;
            }
            else
            {
                // indicates that crop should be removed
                settings.CropRectangle = null;
                settings.AspectRatioId = null;
                settings.AspectRatio = null;
            }
        }

        private void btnRemoveCrop_Click(object sender, EventArgs e)
        {
            imageCropControl.Enabled = false;
            SaveSettingsAndApplyDecorator();
            FindForm().DialogResult = DialogResult.OK;
        }

        private void imageCropControl_CropRectangleChanged(object sender, EventArgs e)
        {
            imageCropControl_AspectRatioChanged(sender, e);
            UpdatePreview(false);
        }

        private void imageCropControl_AspectRatioChanged(object sender, EventArgs e)
        {
            if (EditorContext.EnforcedAspectRatio == null)
            {
                Rectangle rect = imageCropControl.CropRectangle;
                double? aspectRatio = imageCropControl.AspectRatio;
                if (rect.Width == rect.Height || aspectRatio == 1.0)
                    buttonRotate.Enabled = false;
                else
                    buttonRotate.Enabled = true;

                buttonRotate.Visible = aspectRatio != 1.0;
            }
        }

        private void cbAspectRatio_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbAspectRatio.SelectedItem != null)
            {
                double currentEffectiveAspectRatio = imageCropControl.CropRectangle.Width /
                                                     (double)imageCropControl.CropRectangle.Height;

                double? aspectRatio = ((AspectRatioItem)cbAspectRatio.SelectedItem).AspectRatio;
                if (aspectRatio != null && cbAspectRatio.SelectedIndex != 0 && aspectRatio.Value != 1.0)
                {
                    if (aspectRatio < 1.0 ^ currentEffectiveAspectRatio < 1.0)
                        aspectRatio = 1.0 / aspectRatio;
                }
                imageCropControl.AspectRatio = aspectRatio;
            }
            else
                imageCropControl.AspectRatio = null;
        }

        private void buttonRotate_Click(object sender, EventArgs e)
        {
            if (!buttonRotate.Enabled)
                return;

            Size size = imageCropControl.CropRectangle.Size;

            if (imageCropControl.AspectRatio != null)
            {
                imageCropControl.AspectRatio = 1 / imageCropControl.AspectRatio.Value;
            }
            else
            {
                imageCropControl.CropRectangleChanged -= imageCropControl_CropRectangleChanged;
                try
                {
                    imageCropControl.AspectRatio = size.Height / (double)size.Width;
                }
                finally
                {
                    imageCropControl.CropRectangleChanged += imageCropControl_CropRectangleChanged;
                }
                imageCropControl.AspectRatio = null;
                UpdatePreview(false);
            }
        }

        private class AspectRatioItem : IEquatable<AspectRatioItem>
        {
            private readonly string id;
            private readonly string label;
            private double? aspectRatio;

            public AspectRatioItem(string id, string label, double? aspectRatio)
            {
                this.id = id;
                this.label = label;
                this.aspectRatio = aspectRatio;
            }

            public string Id
            {
                get { return id; }
            }

            public double? AspectRatio
            {
                get { return aspectRatio; }
                set { aspectRatio = value; }
            }

            public override string ToString()
            {
                return label;
            }

            public static bool operator !=(AspectRatioItem aspectRatioItem1, AspectRatioItem aspectRatioItem2)
            {
                return !Equals(aspectRatioItem1, aspectRatioItem2);
            }

            public static bool operator ==(AspectRatioItem aspectRatioItem1, AspectRatioItem aspectRatioItem2)
            {
                return Equals(aspectRatioItem1, aspectRatioItem2);
            }

            public bool Equals(AspectRatioItem aspectRatioItem)
            {
                if (aspectRatioItem == null) return false;
                return Equals(label, aspectRatioItem.label) && Equals(aspectRatio, aspectRatioItem.aspectRatio);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(this, obj)) return true;
                return Equals(obj as AspectRatioItem);
            }

            public override int GetHashCode()
            {
                return label.GetHashCode() + 29 * aspectRatio.GetHashCode();
            }
        }

        private void chkGrid_CheckedChanged(object sender, EventArgs e)
        {
            imageCropControl.GridLines = chkGrid.Checked;
        }
    }
}
