// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Forms;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Localization;
using OpenLiveWriter.CoreServices.Layout;

namespace OpenLiveWriter.FileDestinations
{
    public delegate void ErrorHandler(Exception e);

    public class PublishFolderPicker : ApplicationDialog
    {
        private TreeView destinationTree;
        private IContainer components = null;
        private Button buttonOK;
        private Button buttonCancel;
        private ImageList imageList;
        private Label label1;
        private NewFolderButton buttonNewFolder;
        private TreeNode RootNode;
        private ErrorHandler errorHandler;

        public PublishFolderPicker(string destinationName, ErrorHandler errorCallback) : this(destinationName, errorCallback, "/")
        {
        }

        public PublishFolderPicker(string destinationName, ErrorHandler errorCallback, string rootPath)
        {
            // This call is required by the Windows Form Designer.
            InitializeComponent();
            destinationTree.RightToLeftLayout = true;
            this.buttonOK.Text = Res.Get(StringId.OKButtonText);
            this.buttonCancel.Text = Res.Get(StringId.CancelButton);
            this.label1.Text = Res.Get(StringId.PublishFolderPickerCaption);
            this.buttonNewFolder.ButtonText = Res.Get(StringId.NewFolder);
            this.buttonNewFolder.AccessibleName = Res.Get(StringId.NewFolder);
            this.buttonNewFolder.ToolTip = Res.Get(StringId.NewFolderTooltip);
            this.Text = Res.Get(StringId.PublishFolderPickerTitle);

            //set the error handler.
            errorHandler = errorCallback;

            //Root Node
            RootNode = new TreeNode();

            // configure tree
            RootNode.Text = destinationName;
            RootNode.Tag = rootPath;
            destinationTree.Nodes.Add(RootNode);
            destinationTree.SelectedNode = RootNode;
            destinationTree.SelectedNode.ImageIndex = 0;
        }

        public static string BrowseFTPDestination(string name, WinInetFTPFileDestination destination, string currentPath, ErrorHandler errorHandler, IWin32Window owner)
        {
            PublishFolderPicker folderPicker = new PublishFolderPicker(name, errorHandler);
            using (folderPicker)
            {
                try
                {
                    using (new WaitCursor())
                        destination.Connect();

                    folderPicker.Destination = destination;

                    //default the selected path to what's in the textField (if it exists!)
                    if (!currentPath.Equals(""))
                    {
                        if (!currentPath.StartsWith("/", StringComparison.OrdinalIgnoreCase))
                        {
                            currentPath = destination.HomeDir + "/" + currentPath;
                        }
                        folderPicker.SelectedPath = currentPath;
                    }
                    else
                    {
                        string currDirectory = destination.HomeDir;
                        if (!currDirectory.StartsWith("/", StringComparison.OrdinalIgnoreCase))
                            currDirectory = "/" + currDirectory;
                        folderPicker.SelectedPath = currDirectory;
                    }

                    if (folderPicker.ShowDialog(owner) == DialogResult.OK)
                    {
                        return folderPicker.SelectedPath;
                    }
                }
                catch (Exception ex)
                {
                    if (errorHandler != null)
                        errorHandler(ex);
                }
                finally
                {
                    try
                    {
                        destination.Disconnect();
                    }
                    catch (Exception)
                    {
                        //eat it since we've already reported the error and this error
                        //is probably because no valid connection was ever established
                    }
                }
                return null;
            }
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PublishFolderPicker));
            this.destinationTree = new System.Windows.Forms.TreeView();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonNewFolder = new OpenLiveWriter.Controls.NewFolderButton(this.components);
            this.SuspendLayout();
            //
            // destinationTree
            //
            this.destinationTree.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.destinationTree.HideSelection = false;
            this.destinationTree.HotTracking = true;
            this.destinationTree.ImageIndex = 0;
            this.destinationTree.ImageList = this.imageList;
            this.destinationTree.Location = new System.Drawing.Point(11, 33);
            this.destinationTree.Name = "destinationTree";
            this.destinationTree.SelectedImageIndex = 0;
            this.destinationTree.Size = new System.Drawing.Size(312, 265);
            this.destinationTree.TabIndex = 0;
            this.destinationTree.MouseDown += new System.Windows.Forms.MouseEventHandler(this.destinationTree_MouseDown);
            //
            // imageList
            //
            this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList.Images.SetKeyName(0, "");
            this.imageList.Images.SetKeyName(1, "");
            this.imageList.Images.SetKeyName(2, "");
            this.imageList.Images.SetKeyName(3, "");
            this.imageList.Images.SetKeyName(4, "");
            //
            // buttonOK
            //
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(133, 306);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(90, 27);
            this.buttonOK.TabIndex = 1;
            this.buttonOK.Text = "OK";
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            //
            // buttonCancel
            //
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(232, 306);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(90, 27);
            this.buttonCancel.TabIndex = 2;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            //
            // label1
            //
            this.label1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label1.Location = new System.Drawing.Point(11, 11);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(247, 19);
            this.label1.TabIndex = 5;
            this.label1.Text = "Select a folder to publish to:";
            //
            // buttonNewFolder
            //
            this.buttonNewFolder.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.buttonNewFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonNewFolder.AutoSizeHeight = true;
            this.buttonNewFolder.AutoSizeWidth = true;
            this.buttonNewFolder.BitmapDisabled = ((System.Drawing.Bitmap)(resources.GetObject("buttonNewFolder.BitmapDisabled")));
            this.buttonNewFolder.BitmapEnabled = ((System.Drawing.Bitmap)(resources.GetObject("buttonNewFolder.BitmapEnabled")));
            this.buttonNewFolder.BitmapSelected = ((System.Drawing.Bitmap)(resources.GetObject("buttonNewFolder.BitmapSelected")));
            this.buttonNewFolder.ButtonText = "New Folder";
            this.buttonNewFolder.Location = new System.Drawing.Point(208, 5);
            this.buttonNewFolder.Name = "buttonNewFolder";
            this.buttonNewFolder.Size = new System.Drawing.Size(117, 26);
            this.buttonNewFolder.TabIndex = 3;
            this.buttonNewFolder.ToolTip = "Create a new folder";
            this.buttonNewFolder.Click += new System.EventHandler(this.buttonNewFolder_Click);
            //
            // PublishFolderPicker
            //
            this.AcceptButton = this.buttonOK;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(336, 341);
            this.Controls.Add(this.buttonNewFolder);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.destinationTree);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PublishFolderPicker";
            this.Text = "Browse For Folder";
            this.ResumeLayout(false);

        }
        #endregion

        public string SelectedPath
        {
            get
            {
                return this.destinationTree.SelectedNode.Tag.ToString();
            }
            set
            {
                string rootPath = RootNode.Tag.ToString();
                if (value.StartsWith(rootPath, StringComparison.OrdinalIgnoreCase))
                {
                    string relativePath = value.Substring(rootPath.Length);
                    SelectedRelativePath = relativePath;
                }
                else
                {
                    Debug.Fail(String.Format(CultureInfo.InvariantCulture, "specified path [{0}] is not within destination root [{1}]", value, rootPath));
                }
            }
        }

        public string SelectedRelativePath
        {
            get
            {
                // calculate root path length
                string rootPath = RootNode.Tag.ToString();
                int rootPathLength = rootPath.Length;
                if (!(rootPath.EndsWith("\\", StringComparison.OrdinalIgnoreCase) || rootPath.EndsWith("/", StringComparison.OrdinalIgnoreCase)))
                    rootPathLength++;

                string fullPath = destinationTree.SelectedNode.Tag.ToString();
                if (rootPathLength < fullPath.Length)
                    return fullPath.Substring(rootPathLength);
                else
                    return String.Empty;
            }

            set
            {
                using (new WaitCursor())
                {
                    //walk from the root node and select the specified path
                    string path = value;
                    string[] paths = path.Split(new char[] { destination.PathDelimiterChar });
                    TreeNode currNode = RootNode;

                    //Build the tree for the selected path
                    //note: skip the first part since it should be the root
                    for (int i = 0; i < paths.Length; i++)
                    {
                        string pathPart = paths[i];
                        if (pathPart == string.Empty)
                        {
                            //this case can occur if the path is "/"
                            continue;
                        }
                        TreeNode nextNode = null;
                        EnumDirectories(currNode);
                        TreeNodeCollection nodes = currNode.Nodes;

                        //search for a node in the tree the matches the pathPath exactly
                        //note: exact match is required for UNIX-based FTP destinations
                        foreach (TreeNode node in nodes)
                        {
                            if (node.Text.Equals(pathPart))
                            {
                                nextNode = node;
                                break;
                            }
                        }

                        //if the exact path part wasn't found, try a case-insensitive comparison
                        if (nextNode == null)
                        {
                            foreach (TreeNode node in nodes)
                            {
                                if (node.Text.ToLower(CultureInfo.CurrentCulture).Equals(pathPart.ToLower(CultureInfo.CurrentCulture)))
                                {
                                    nextNode = node;
                                    break;
                                }
                            }
                        }

                        //If the elements in the folder can't be listed, then just shove it into the tree.
                        //Note: This case is necessary for shared FTP environments where the user starts
                        //out in a writable homedir but the parent dirs aren't browsable.
                        if (nextNode == null)
                        {
                            string fullPath = destination.CombinePath(currNode.Tag.ToString(), pathPart);
                            if (destination.DirectoryExists(fullPath))
                            {
                                nextNode = createFolderNode(currNode, pathPart);
                            }
                            else
                            {
                                //we hit the end of the valid part of this path
                                break;
                            }
                        }
                        currNode = nextNode;
                    }
                    destinationTree.SelectedNode = currNode;
                }
            }
        }

        private FileDestination destination;
        /// <summary>
        /// Set the destination that this dialog will choose a folder from.
        /// </summary>
        public FileDestination Destination
        {
            get
            {
                return destination;
            }
            set
            {
                destination = value;
                destinationTree.AfterSelect += new TreeViewEventHandler(destinationTree_AfterSelect);
            }
        }

        private void destinationTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            EnumDirectories(e.Node);
            e.Node.EnsureVisible();
        }

        //Enumerates the destination directory associated with this treenode and populates
        //the TreeNode with nodes representing folders in the directory.
        private void EnumDirectories(TreeNode ParentNode)
        {
            destinationTree.SelectedNode = ParentNode;
            string DirectoryPath = ParentNode.Tag.ToString();
            if (ParentNode.Nodes.Count == 0)
            {
                if (!DirectoryPath.EndsWith(destination.PathDelimiter, StringComparison.OrdinalIgnoreCase))
                    DirectoryPath += destination.PathDelimiter;
                try
                {
                    using (new WaitCursor())
                    {
                        string[] directories = Destination.ListDirectories(DirectoryPath);
                        if (directories.Length == 0)
                        {
                            string homeDir = ((WinInetFTPFileDestination)Destination).HomeDir;

                            // break homedir and directorypath into path elements
                            if (IsParentPath(DirectoryPath, homeDir))
                            {
                                string restOfPath = homeDir.Substring(DirectoryPath.Length);
                                if (restOfPath.Length > 0)
                                {
                                    int nextChunkLen = restOfPath.IndexOf(destination.PathDelimiter, StringComparison.OrdinalIgnoreCase);
                                    if (nextChunkLen == -1)
                                        nextChunkLen = restOfPath.Length;
                                    string nextChunk = restOfPath.Substring(0, nextChunkLen);

                                    TreeNode newNode = createFolderNode(ParentNode, nextChunk);
                                    newNode.EnsureVisible();
                                }
                            }
                        }
                        else
                        {
                            foreach (string directory in directories)
                            {
                                if (directory != "." && directory != "..")
                                {
                                    TreeNode newNode = createFolderNode(ParentNode, directory);
                                    newNode.EnsureVisible();
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Trace.WriteLine("Unexpected exception enumerating Publish Folder directories: " + e.ToString());
                }
            }
        }

        private bool IsParentPath(string parent, string child)
        {
            if (parent.Length == 0)
                return false;

            if (!child.StartsWith(parent, StringComparison.OrdinalIgnoreCase))
                return false;

            string[] parentElements = StringHelper.Split(parent, Destination.PathDelimiter);
            string[] childElements = StringHelper.Split(child, Destination.PathDelimiter);

            for (int i = 0; i < parentElements.Length; i++)
            {
                if (childElements[i] != parentElements[i])
                    return false;
            }
            return true;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void buttonNewFolder_Click(object sender, EventArgs e)
        {
            using (new WaitCursor())
            {
                TreeNode selectedNode = destinationTree.SelectedNode;
                string selectedPath = selectedNode.Tag.ToString();
                NewFolder newFolder = new NewFolder();
                using (newFolder)
                {
                    if (newFolder.ShowDialog(this.FindForm()) == DialogResult.OK)
                    {
                        try
                        {
                            string newFolderName = newFolder.NewFolderName;
                            string newPath = Destination.CombinePath(selectedPath, newFolderName);
                            using (new WaitCursor())
                            {
                                Destination.CreateDirectory(newPath);
                                refreshNode(selectedNode);
                            }
                            SelectedPath = newPath;
                            destinationTree.Focus();
                        }
                        catch (Exception ex)
                        {
                            if (errorHandler != null)
                                errorHandler(ex);
                            else
                                throw ex;
                        }
                    }
                }
            }
        }

        //causes the specified parent node to refresh its listing of children
        private void refreshNode(TreeNode parent)
        {
            //dump the children nodes, and re-enumerate the directory.
            parent.Nodes.Clear();
            EnumDirectories(parent);
        }

        private TreeNode createFolderNode(TreeNode parent, string dirName)
        {
            TreeNode TempNode = new TreeNode();
            TempNode.Text = dirName;
            TempNode.Tag = destination.CombinePath(parent.Tag.ToString(), dirName);
            TempNode.ImageIndex = 3;
            TempNode.SelectedImageIndex = 2;
            parent.Nodes.Add(TempNode);
            return TempNode;
        }

        private void destinationTree_MouseDown(object sender, MouseEventArgs e)
        {
            // make right-click work for selection (defect 2518)
            if (e.Button == MouseButtons.Right)
            {
                TreeNode node = destinationTree.GetNodeAt(e.X, e.Y);
                if (node != null)
                    destinationTree.SelectedNode = node;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            using (new AutoGrow(this, AnchorStyles.Bottom, false))
            {
                LayoutHelper.NaturalizeHeightAndDistribute(3, label1, destinationTree, new ControlGroup(buttonOK, buttonCancel));

                buttonNewFolder.Left = destinationTree.Right - buttonNewFolder.Width;

                if (label1.Right > buttonNewFolder.Left)
                {
                    buttonNewFolder.Text = null;
                    buttonNewFolder.ButtonText = null;
                    buttonNewFolder.Width = buttonNewFolder.BitmapEnabled.Width + 8;
                    buttonNewFolder.Left = destinationTree.Right - buttonNewFolder.Width;
                }

                buttonNewFolder.Top = destinationTree.Top - buttonNewFolder.Height - 1;
            }

        }
    }
}

