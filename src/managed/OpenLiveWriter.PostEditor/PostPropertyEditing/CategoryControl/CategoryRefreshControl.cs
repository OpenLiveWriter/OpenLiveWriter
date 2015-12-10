// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.PostPropertyEditing.CategoryControl
{
    /// <summary>
    /// Summary description for CategoryRefreshControl.
    /// </summary>
    internal class CategoryRefreshControl : UserControl
    {
        private BitmapButton buttonRefresh;
        private IContainer components;
        private CategoryContext _categoryContext;

        public CategoryRefreshControl()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();
            buttonRefresh.ButtonText = Res.Get(StringId.CategoryRefreshList);
            this.buttonRefresh.ToolTip = Res.Get(StringId.CategoryRefreshListTooltip);
        }

        public void Initialize(CategoryContext context)
        {
            SuspendLayout();
            buttonRefresh.BitmapDisabled = ResourceHelper.LoadAssemblyResourceBitmap("PostPropertyEditing.CategoryControl.Images.refresh.png");
            buttonRefresh.BitmapEnabled = ResourceHelper.LoadAssemblyResourceBitmap("PostPropertyEditing.CategoryControl.Images.refresh.png");
            buttonRefresh.BitmapPushed = ResourceHelper.LoadAssemblyResourceBitmap("PostPropertyEditing.CategoryControl.Images.refresh.png");
            buttonRefresh.BitmapSelected = ResourceHelper.LoadAssemblyResourceBitmap("PostPropertyEditing.CategoryControl.Images.refreshSelected.png");
            buttonRefresh.TextAlignment = TextAlignment.Right;
            buttonRefresh.ButtonStyle = ButtonStyle.Standard;
            buttonRefresh.ButtonText = Res.Get(StringId.CategoryRefreshList);
            this.buttonRefresh.Size = new Size(94, 22);
            buttonRefresh.Click += new EventHandler(buttonRefresh_Click);

            ResumeLayout(false);
            _categoryContext = context;

        }

        private void RequestRefresh()
        {
            _refreshing = true;
            using (new WaitCursor())
                _categoryContext.Refresh();
            _refreshing = false;
        }
        private bool _refreshing = false;

        protected override void OnMouseUp(MouseEventArgs e)
        {
            RequestRefresh();
            base.OnMouseUp (e);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Enter && !_refreshing)
            {
                RequestRefresh();
                return true;
            }
            else if (keyData == Keys.Up)
            {
                if (Parent != null)
                    Parent.SelectNextControl(this, false, true, true, true);
                return true;
            }
            else if (keyData == Keys.Down)
            {
                if (Parent != null)
                    Parent.SelectNextControl(this, true, true, true, true);
                return true;
            }
            return base.ProcessCmdKey (ref msg, keyData);
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose( bool disposing )
        {
            if( disposing )
            {
                if(components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose( disposing );
        }

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.buttonRefresh = new OpenLiveWriter.Controls.BitmapButton(this.components);
            this.SuspendLayout();
            //
            // buttonRefresh
            //
            this.buttonRefresh.AutoSizeWidth = true;
            this.buttonRefresh.ButtonText = "Refresh List";
            this.buttonRefresh.Location = new System.Drawing.Point(0, 0);
            this.buttonRefresh.Name = "buttonRefresh";
            this.buttonRefresh.Size = new System.Drawing.Size(94, 29);
            this.buttonRefresh.TabIndex = 0;
            this.buttonRefresh.ToolTip = "Refreshes the list of categories.";
            //
            // CategoryRefreshControl
            //
            this.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.Controls.Add(this.buttonRefresh);
            this.Name = "CategoryRefreshControl";
            this.Size = new System.Drawing.Size(94, 29);
            this.ResumeLayout(false);

        }
        #endregion

        private void buttonRefresh_Click(object sender, EventArgs e)
        {
            RequestRefresh();
        }

    }
}
