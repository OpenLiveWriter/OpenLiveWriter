using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using LocUtil;

namespace LocEdit
{
    public partial class MainForm : Form
    {
        private string _loadedFile;

        public MainForm()
        {
            InitializeComponent();
        }

        public void LoadFile(string filePath)
        {
            _loadedFile = filePath;
            Text = $"LocEdit: {filePath}";

            dataGridView.Rows.Clear();
            using (CsvParser csvParser = new CsvParser(new StreamReader(new FileStream(Path.GetFullPath(filePath), FileMode.Open, FileAccess.Read, FileShare.ReadWrite), Encoding.Default), true))
            {
                foreach (string[] line in csvParser)
                {
                    string value = line[1];
                    value = value.Replace(((char)8230) + "", "..."); // undo ellipses
                    string comment = (line.Length > 2) ? line[2] : "";

                    dataGridView.Rows.Add(new LocDataGridViewRow(line[0], value, comment));
                }
            }

        }

        public void FindNext(string query)
        {
            int y = dataGridView.CurrentCell != null ? dataGridView.CurrentCell.RowIndex + 1 : 0;
            dataGridView.ClearSelection();

            while(y < dataGridView.Rows.Count - 1)
            {
                if (((string)dataGridView.Rows[y].Cells[0].Value).Contains(query))
                {
                    dataGridView.CurrentCell = dataGridView.Rows[y].Cells[0];
                    return;
                }
                y++;
            }

            MessageBox.Show("No results found, returning to start.");
            if (dataGridView.Rows.Count == 0 || dataGridView.Rows[0].Cells.Count == 0) return;

            dataGridView.CurrentCell = dataGridView.Rows[0].Cells[0];
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.O))
            {
                ShowOpenDialog();
                return true;
            }

            if (keyData == (Keys.Control | Keys.F))
            {
                textBoxFind.Focus();
                return true;
            }


            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void ShowOpenDialog()
        {
            using (OpenFileDialog openFileDialog1 = new OpenFileDialog())
            {
                openFileDialog1.Filter = "Localization CSV File (*.csv)|*.csv";

                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    LoadFile(openFileDialog1.FileName);
                }
            }
        }

        private void TableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void ToolStripButtonLoad_Click(object sender, EventArgs e)
            => ShowOpenDialog();

        private void ButtonFind_Click(object sender, EventArgs e)
            => FindNext(textBoxFind.Text);

        private void TextBoxFind_KeyUp(object sender, KeyEventArgs e)
        {
            if(textBoxFind.Focused && e.KeyCode == Keys.Enter) FindNext(textBoxFind.Text);
        }
    }

    internal class LocDataGridViewRow : DataGridViewRow
    {
        private DataGridViewCell keyCell;
        private DataGridViewCell valueCell;
        private DataGridViewCell commentCell;

        public LocDataGridViewRow(string key, string value, string comment) : base()
        {
            keyCell = new DataGridViewTextBoxCell() { Value = key };
            valueCell = new DataGridViewTextBoxCell() { Value = value };
            commentCell = new DataGridViewTextBoxCell() { Value = comment };

            Cells.Add(keyCell);
            Cells.Add(valueCell);
            Cells.Add(commentCell);
        }
    }
}
