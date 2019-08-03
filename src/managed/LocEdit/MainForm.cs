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

        private bool _modified = false;
        public bool Modified
        {
            get => _modified;
            set
            {
                _modified = value;
                Text = $"LocEdit: {_loadedFile}{(_modified ? "*" : "")}";
            }
        }

        public void LoadFile(string filePath)
        {
            _loadedFile = filePath;
            Modified = false;

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
            dataGridView.AutoResizeRows();

        }

        public void SaveFile(string filePath)
        {
            _loadedFile = filePath;
            Modified = false;

            var sb = new StringBuilder();
            sb.AppendLine("Name,Value,Comment");

            foreach(DataGridViewRow row in dataGridView.Rows)
            {
                if (row.Cells.Count < 3) continue;

                var key = (string)row.Cells[0].Value;
                var value = (string)row.Cells[1].Value;
                var comment = (string)row.Cells[2].Value;

                if (key == null || value == null) continue;

                sb.Append($"{Helpers.CsvizeString(key)},");
                sb.Append($"{Helpers.CsvizeString(value)},");
                sb.Append($"{Helpers.CsvizeString(comment == null ? "" : comment)}");
                sb.AppendLine();
            }

            File.WriteAllText(filePath, sb.ToString(), Encoding.Default);
        }

        public void FindNext(string query)
        {
            int y = dataGridView.CurrentCell != null ? dataGridView.CurrentCell.RowIndex + 1 : 0;
            dataGridView.ClearSelection();

            while(y < dataGridView.Rows.Count - 1)
            {
                if (((string)dataGridView.Rows[y].Cells[0].Value).ToLower().Contains(query.ToLower()))
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

            if (keyData == (Keys.Control | Keys.S))
            {
                ShowSaveDialog();
                return true;
            }

            if (keyData == (Keys.Control | Keys.F))
            {
                textBoxFind.Focus();
                return true;
            }

            if (keyData == (Keys.Control | Keys.A))
            {
                if(dataGridView.CurrentRow != null) dataGridView.CurrentRow.Selected = true;
                return true;
            }

            if (keyData == Keys.F3)
            {
                FindNext(textBoxFind.Text);
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void ShowOpenDialog()
        {
            using (OpenFileDialog openFileDialog1 = new OpenFileDialog())
            {
                openFileDialog1.Filter = "Localization CSV File (*.csv)|*.csv";

                if (openFileDialog1.ShowDialog() == DialogResult.OK) LoadFile(openFileDialog1.FileName);
            }
        }

        private void ShowSaveDialog()
        {
            using (SaveFileDialog saveFileDialog1 = new SaveFileDialog())
            {
                saveFileDialog1.FileName = _loadedFile;
                saveFileDialog1.Filter = "Localization CSV File (*.csv)|*.csv";

                if (saveFileDialog1.ShowDialog() == DialogResult.OK) SaveFile(saveFileDialog1.FileName);
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
            if (textBoxFind.Focused && e.KeyCode == Keys.Enter)
            {
                FindNext(textBoxFind.Text);
                e.Handled = true;
            }
        }

        private void ToolStripButtonSave_Click(object sender, EventArgs e)
            => ShowSaveDialog();

        private void DataGridView_CellValueChanged(object sender, DataGridViewCellEventArgs e)
            => Modified = true;

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!Modified) return;

            var result = MessageBox.Show(this, "There are unsaved changes. Are you sure you want to exit?", "LocEdit", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
            if (result == DialogResult.No) e.Cancel = true;
        }

        private void ToolStripLabel1_Click(object sender, EventArgs e)
        {

        }

        private void ToolStripButtonInsertAbove_Click(object sender, EventArgs e)
        {
            if (dataGridView.CurrentCell != null) dataGridView.Rows.Insert(dataGridView.CurrentCell.RowIndex);
        }

        private void ToolStripButtonInsertBelow_Click(object sender, EventArgs e)
        {
            if (dataGridView.CurrentCell != null && dataGridView.CurrentCell.RowIndex < dataGridView.Rows.Count - 1)
                dataGridView.Rows.Insert(dataGridView.CurrentCell.RowIndex + 1);
        }
    }

    internal static class Helpers
    {
        public static string CsvizeString(string input)
        {
            var sb = new StringBuilder();
            bool shouldQuote = input.Contains(",") || input.Contains("\n") || (input != string.Empty && input[0] == '"');

            if (shouldQuote)
            {
                sb.Append('"');
                sb.Append(input.Replace("\"", "\"\"")); // Replace double-quotes with double-double-quotes 
                sb.Append('"');
            }
            else
            {
                sb.Append(input);
            }


            return sb.ToString();
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
