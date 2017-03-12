// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace BlogRunnerGui
{
    public enum DialogStyle { Open, Save }

    [DefaultEvent("PathChanged")]
    public partial class FileInput : UserControl
    {
        public FileInput()
        {
            InitializeComponent();
        }

        public event EventHandler PathChanged;

        [
            EditorBrowsable(EditorBrowsableState.Always),
            Browsable(true),
            DesignerSerializationVisibility(DesignerSerializationVisibility.Visible),
            Bindable(true)
        ]
        public override string Text
        {
            get { return label1.Text; }
            set { label1.Text = value; }
        }

        public string Path
        {
            get { return textBox1.Text; }
            set { textBox1.Text = value; }
        }

        public DialogStyle DialogStyle;

        protected override void OnGotFocus(EventArgs e)
        {
            textBox1.Focus();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FileDialog fd;
            if (DialogStyle == DialogStyle.Open)
                fd = openFileDialog1;
            else
                fd = saveFileDialog1;

            if (fd.ShowDialog(this) == DialogResult.OK)
            {
                textBox1.Text = fd.FileName;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            string path = textBox1.Text.Trim('"');
            if (path != textBox1.Text)
            {
                textBox1.Text = path;
                textBox1.SelectionStart = textBox1.TextLength;
                return;
            }

            if (PathChanged != null)
                PathChanged(this, EventArgs.Empty);
        }
    }
}
