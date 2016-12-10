// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
namespace BlogRunnerGui
{
    using System;
    using System.ComponentModel;
    using System.Windows.Forms;

    /// <summary>
    /// Enum DialogStyle
    /// </summary>
    public enum DialogStyle { Open, Save }

    /// <summary>
    /// Class FileInput.
    /// </summary>
    /// <seealso cref="System.Windows.Forms.UserControl" />
    [DefaultEvent(nameof(PathChanged))]
    public partial class FileInput : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileInput"/> class.
        /// </summary>
        public FileInput()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Occurs when [path changed].
        /// </summary>
        public event EventHandler PathChanged;

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        /// <value>The text.</value>
        [
            EditorBrowsable(EditorBrowsableState.Always),
            Browsable(true),
            DesignerSerializationVisibility(DesignerSerializationVisibility.Visible),
            Bindable(true)
        ]
        public override string Text
        {
            get
            {
                return this.label1.Text;
            }

            set
            {
                this.label1.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets the path.
        /// </summary>
        /// <value>The path.</value>
        public string Path
        {
            get
            {
                return this.textBox1.Text;
            }

            set
            {
                this.textBox1.Text = value;
            }
        }

        /// <summary>
        /// The dialog style
        /// </summary>
        public DialogStyle DialogStyle;

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.GotFocus" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnGotFocus(EventArgs e)
        {
            this.textBox1.Focus();
        }

        /// <summary>
        /// Handles the Click event of the button1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void button1_Click(object sender, EventArgs e)
        {
            FileDialog fd;
            if (this.DialogStyle == DialogStyle.Open)
            {
                fd = this.openFileDialog1;
            }
            else
            {
                fd = this.saveFileDialog1;
            }

            if (fd.ShowDialog(this) == DialogResult.OK)
            {
                this.textBox1.Text = fd.FileName;
            }
        }

        /// <summary>
        /// Handles the TextChanged event of the textBox1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            var path = this.textBox1.Text.Trim('"');
            if (path != this.textBox1.Text)
            {
                this.textBox1.Text = path;
                this.textBox1.SelectionStart = this.textBox1.TextLength;
                return;
            }

            this.PathChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
