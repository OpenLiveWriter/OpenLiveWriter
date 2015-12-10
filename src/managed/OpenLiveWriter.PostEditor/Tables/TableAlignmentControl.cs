// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.Tables
{

    public class TableAlignmentControl : System.Windows.Forms.UserControl
    {
        private System.Windows.Forms.Label labelCaption;
        private System.Windows.Forms.ComboBox comboBoxValue;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public TableAlignmentControl()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            // TODO: Add any initialization after the InitializeComponent call
        }

        public static void AdjustSizes(params TableAlignmentControl[] controls)
        {
            int maxCaptionWidth = 0;
            int maxComboWidth = 0;
            foreach (TableAlignmentControl c in controls)
            {
                maxCaptionWidth = Math.Max(maxCaptionWidth,
                    DisplayHelper.AutoFitSystemLabel(c.labelCaption, 0, int.MaxValue));
                maxComboWidth = Math.Max(maxComboWidth,
                    DisplayHelper.AutoFitSystemCombo(c.comboBoxValue, 0, int.MaxValue, false));

            }

            foreach (TableAlignmentControl c in controls)
            {
                c.comboBoxValue.Left = c.labelCaption.Left + maxCaptionWidth + (int)DisplayHelper.ScaleX(8);
                c.comboBoxValue.Width = maxComboWidth;
                if (c.Width < c.comboBoxValue.Right)
                    c.Width = c.comboBoxValue.Right + 1;
            }
        }

        protected void Initialize(AlignmentEditingProfile profile)
        {
            labelCaption.Text = profile.Label;
            comboBoxValue.Items.AddRange(profile.Options);
            comboBoxValue.SelectedIndex = 0;
        }

        public void NaturalizeHeight()
        {
            Height = comboBoxValue.Bottom;
        }

        protected AlignmentOption SelectedOption
        {
            get
            {
                return comboBoxValue.SelectedItem as AlignmentOption;
            }
            set
            {
                comboBoxValue.SelectedItem = value;
            }
        }

        protected void AddOption(AlignmentOption option)
        {
            if (!comboBoxValue.Items.Contains(option))
                comboBoxValue.Items.Add(option);
        }

        protected void RemoveOption(AlignmentOption option)
        {
            if (comboBoxValue.Items.Contains(option))
                comboBoxValue.Items.Remove(option);
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

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.labelCaption = new System.Windows.Forms.Label();
            this.comboBoxValue = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            //
            // labelCaption
            //
            this.labelCaption.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelCaption.Location = new System.Drawing.Point(0, 3);
            this.labelCaption.Name = "labelCaption";
            this.labelCaption.Size = new System.Drawing.Size(113, 17);
            this.labelCaption.TabIndex = 0;
            this.labelCaption.Text = "Horizontal alignment:";
            //
            // comboBoxValue
            //
            this.comboBoxValue.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxValue.Location = new System.Drawing.Point(115, 0);
            this.comboBoxValue.Name = "comboBoxValue";
            this.comboBoxValue.Size = new System.Drawing.Size(86, 21);
            this.comboBoxValue.TabIndex = 1;
            //
            // TableAlignmentControl
            //
            this.Controls.Add(this.comboBoxValue);
            this.Controls.Add(this.labelCaption);
            this.Name = "TableAlignmentControl";
            this.Size = new System.Drawing.Size(208, 21);
            this.ResumeLayout(false);

        }
        #endregion

        protected class AlignmentOption
        {
            public AlignmentOption(string caption, object value)
            {
                _caption = caption;
                _value = value;
            }

            public AlignmentOption(object value)
            {
                _value = value;
            }

            public string Caption { get { return _caption; } }
            public object Value { get { return _value; } }

            public override string ToString()
            {
                return Caption;
            }

            public override bool Equals(object obj)
            {
                return (obj as AlignmentOption).Value.Equals(_value);
            }

            public override int GetHashCode()
            {
                return _value.GetHashCode();
            }

            private string _caption;
            private object _value;
        }

        protected class AlignmentEditingProfile
        {
            public AlignmentEditingProfile(string label, AlignmentOption[] options)
            {
                _label = label;
                _options = options;
            }

            public string Label { get { return _label; } }
            public AlignmentOption[] Options { get { return _options; } }

            private string _label;
            private AlignmentOption[] _options;
        }
    }

    public class HorizontalAlignmentControl : TableAlignmentControl
    {
        public HorizontalAlignmentControl()
            : base()
        {
            Initialize(new AlignmentEditingProfile(Res.Get(StringId.TableAlignHorizontal), new AlignmentOption[]
            {
                new AlignmentOption(Res.Get(StringId.TableAlignHorizontalLeft), HorizontalAlignment.Left),
                new AlignmentOption(Res.Get(StringId.TableAlignHorizontalCenter), HorizontalAlignment.Center),
                new AlignmentOption(Res.Get(StringId.TableAlignHorizontalRight), HorizontalAlignment.Right)
            }));
        }

        public HorizontalAlignment HorizontalAlignment
        {
            get
            {
                return (HorizontalAlignment)SelectedOption.Value;
            }
            set
            {
                AlignmentOption option = new AlignmentOption(value);

                // dynamically add mixed alignment if necessary
                if (option.Equals(MixedAlignment))
                    AddOption(MixedAlignment);

                SelectedOption = option;
            }
        }

        private readonly AlignmentOption MixedAlignment = new AlignmentOption("", HorizontalAlignment.Mixed);
    }

    public class VerticalAlignmentControl : TableAlignmentControl
    {
        public VerticalAlignmentControl()
            : base()
        {
            Initialize(new AlignmentEditingProfile(Res.Get(StringId.TableAlignVertical), new AlignmentOption[]
            {
                new AlignmentOption(Res.Get(StringId.TableAlignVerticalTop), VerticalAlignment.Top),
                new AlignmentOption(Res.Get(StringId.TableAlignVerticalMiddle), VerticalAlignment.Middle),
                new AlignmentOption(Res.Get(StringId.TableAlignVerticalBottom), VerticalAlignment.Bottom)
            }));
        }

        public VerticalAlignment VerticalAlignment
        {
            get
            {
                return (VerticalAlignment)SelectedOption.Value;
            }
            set
            {
                AlignmentOption option = new AlignmentOption(value);

                // dynamically add mixed alignment if necessary
                if (option.Equals(MixedAlignment))
                    AddOption(MixedAlignment);

                SelectedOption = option;
            }
        }

        private readonly AlignmentOption MixedAlignment = new AlignmentOption("", VerticalAlignment.Mixed);

    }

}
