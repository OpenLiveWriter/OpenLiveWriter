// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.PostEditor.SupportingFiles
{
    /// <summary>
    /// Summary description for AttachedFileForm.
    /// </summary>
    public class SupportingFilesForm : System.Windows.Forms.Form
    {
        private System.Windows.Forms.ListBox listBoxFiles;
        private System.Windows.Forms.DataGrid dataGridProperties;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public SupportingFilesForm()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            listBoxFiles.SelectedIndexChanged += new EventHandler(listBoxFiles_SelectedIndexChanged);
        }

        public static SupportingFilesForm ShowForm(Form parent, BlogPostEditingManager editingManager)
        {
            SupportingFilesForm supportingFilesForm = new SupportingFilesForm();
            parent.AddOwnedForm(supportingFilesForm);
            supportingFilesForm.Show();
            Point location = parent.Location;
            Size size = parent.Size;
            supportingFilesForm.Location = new Point(location.X + size.Width + 2, location.Y);

            supportingFilesForm.BlogPostEditingManager = editingManager;
            return supportingFilesForm;
        }

        internal BlogPostEditingManager BlogPostEditingManager
        {
            get { return _editingContext; }
            set
            {
                Debug.Assert(_editingContext == null, "BlogPostEditingManager already set");
                _editingContext = value;
                SupportingFileService = (SupportingFileService)(_editingContext as IBlogPostEditingContext).SupportingFileService;
                _editingContext.BlogChanged += new EventHandler(_editingContext_BlogChanged);
            }
        }
        private BlogPostEditingManager _editingContext;

        private SupportingFileService SupportingFileService
        {
            get { return _fileService; }
            set
            {
                if (_fileService != null)
                {
                    _fileService.FileAdded -= new SupportingFileService.AttachedFileHandler(fileService_FileAdded);
                    _fileService.FileChanged -= new SupportingFileService.AttachedFileHandler(fileService_FileChanged);
                    _fileService.FileRemoved -= new SupportingFileService.AttachedFileHandler(fileService_FileRemoved);
                }
                _fileService = value;
                listBoxFiles.Items.Clear();
                ResetDataSet();

                if (_fileService != null)
                {
                    _fileService.FileAdded += new SupportingFileService.AttachedFileHandler(fileService_FileAdded);
                    _fileService.FileChanged += new SupportingFileService.AttachedFileHandler(fileService_FileChanged);
                    _fileService.FileRemoved += new SupportingFileService.AttachedFileHandler(fileService_FileRemoved);

                    foreach (ISupportingFile file in _fileService.GetAllSupportingFiles())
                    {
                        AddFile(file);
                    }
                }
            }
        }
        SupportingFileService _fileService;

        private Stream CreateTextStream(string text)
        {
            MemoryStream s = new MemoryStream();
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            s.Write(bytes, 0, bytes.Length);
            s.Seek(0, SeekOrigin.Begin);
            return s;
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_editingContext != null)
                    _editingContext.BlogChanged -= new EventHandler(_editingContext_BlogChanged);
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.listBoxFiles = new System.Windows.Forms.ListBox();
            this.dataGridProperties = new System.Windows.Forms.DataGrid();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridProperties)).BeginInit();
            this.SuspendLayout();
            //
            // listBoxFiles
            //
            this.listBoxFiles.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
            this.listBoxFiles.HorizontalScrollbar = true;
            this.listBoxFiles.Location = new System.Drawing.Point(8, 15);
            this.listBoxFiles.Name = "listBoxFiles";
            this.listBoxFiles.Size = new System.Drawing.Size(648, 121);
            this.listBoxFiles.TabIndex = 0;
            //
            // dataGridProperties
            //
            this.dataGridProperties.AlternatingBackColor = System.Drawing.SystemColors.Control;
            this.dataGridProperties.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridProperties.CaptionVisible = false;
            this.dataGridProperties.DataMember = "";
            this.dataGridProperties.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.dataGridProperties.Location = new System.Drawing.Point(8, 149);
            this.dataGridProperties.Name = "dataGridProperties";
            this.dataGridProperties.ParentRowsVisible = false;
            this.dataGridProperties.ReadOnly = true;
            this.dataGridProperties.RowHeadersVisible = false;
            this.dataGridProperties.Size = new System.Drawing.Size(648, 260);
            this.dataGridProperties.TabIndex = 1;
            //
            // SupportingFilesForm
            //
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(672, 421);
            this.Controls.Add(this.dataGridProperties);
            this.Controls.Add(this.listBoxFiles);
            this.Name = "SupportingFilesForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "AttachedFileForm";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridProperties)).EndInit();
            this.ResumeLayout(false);

        }
        #endregion

        private void ShowFileProperties(SupportingFileFactory.VersionedSupportingFile file)
        {
            ResetDataSet();
            if (file != null)
            {
                AddData("id", file.FileId);
                AddData("version", file.FileVersion.ToString(CultureInfo.InvariantCulture));
                AddData("name", file.FileName);
                AddData("nameUniqueToken", file.FileNameUniqueToken);
                AddData("embedded", file.Embedded.ToString());
                AddData("localPath", UrlHelper.SafeToAbsoluteUri(file.FileUri));
                AddSettings("", file.Settings);

                foreach (string uploadContext in file.GetAllUploadContexts())
                {
                    ISupportingFileUploadInfo uploadInfo = file.GetUploadInfo(uploadContext);
                    string prefix = "upload." + uploadContext + ".";
                    AddData(prefix + "version", uploadInfo.UploadedFileVersion.ToString(CultureInfo.InvariantCulture));
                    AddData(prefix + "uri", UrlHelper.SafeToAbsoluteUri(uploadInfo.UploadUri));
                    AddSettings(prefix, uploadInfo.UploadSettings);
                }
            }
        }
        private void AddSettings(string prefix, BlogPostSettingsBag settings)
        {
            foreach (string name in settings.Names)
            {
                AddData(prefix + name, settings[name]);
            }

            foreach (string subsettingsName in settings.SubsettingNames)
            {
                AddSettings(prefix + subsettingsName + "/", settings.GetSubSettings(subsettingsName));
            }
        }

        private void AddData(string name, string val)
        {
            dataTable.LoadDataRow(new object[] { name, val }, true);
        }

        private void ResetDataSet()
        {
            if (ds == null)
            {
                ds = new DataSet("fileProperties");
                ds.Locale = CultureInfo.InvariantCulture;
                dataGridProperties.DataSource = ds;
                dataTable = new DataTable("properties");
                dataTable.Locale = CultureInfo.InvariantCulture;
                ds.Tables.Add(dataTable);
                nameColumn = dataTable.Columns.Add("name");
                valueColumn = dataTable.Columns.Add("value");
                dataGridProperties.DataMember = "properties";

                // Create new Table Style
                dataGridProperties.TableStyles.Clear();
                DataGridTableStyle ts = new DataGridTableStyle();
                ts.MappingName = dataTable.TableName;
                dataGridProperties.TableStyles.Add(ts);

                //set the width of the name column
                dataGridProperties.TableStyles[dataTable.TableName].GridColumnStyles[nameColumn.ColumnName].Width = 200;
                UpdateTableSize();
            }

            dataTable.Clear();
        }
        DataSet ds;
        DataTable dataTable;
        DataColumn nameColumn;
        DataColumn valueColumn;

        private void AddFile(ISupportingFile file)
        {
            listBoxFiles.Items.Add(file);
        }
        private void fileService_FileAdded(ISupportingFile file)
        {
            AddFile(file);
        }

        private void fileService_FileChanged(ISupportingFile file)
        {
            if (file == listBoxFiles.SelectedItem)
                ShowFileProperties((SupportingFileFactory.VersionedSupportingFile)file);
        }

        private void fileService_FileRemoved(ISupportingFile file)
        {
            listBoxFiles.Items.Remove(file);
        }

        private void listBoxFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            ISupportingFile file = listBoxFiles.SelectedItem as ISupportingFile;
            ShowFileProperties((SupportingFileFactory.VersionedSupportingFile)file);
        }

        protected override void OnResize(EventArgs e)
        {
            if (dataTable != null)
                UpdateTableSize();
            base.OnResize(e);
        }

        private void UpdateTableSize()
        {
            int nameWidth = dataGridProperties.TableStyles[dataTable.TableName].GridColumnStyles[nameColumn.ColumnName].Width;
            dataGridProperties.TableStyles[dataTable.TableName].GridColumnStyles[valueColumn.ColumnName].Width = dataGridProperties.Width - nameWidth - 40;
        }

        private void _editingContext_BlogChanged(object sender, EventArgs e)
        {
            SupportingFileService = (SupportingFileService)(_editingContext as IBlogPostEditingContext).SupportingFileService;
        }
    }
}
