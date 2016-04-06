// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Data;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.HtmlParser.Parser;
using OpenLiveWriter.Interop.Windows;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.PostPropertyEditing.CategoryControl
{
    internal partial class TreeCategorySelector : UserControl, ICategorySelector
    {
        private readonly CategoryContext ctx;
        private TreeNode[] nodes = new TreeNode[0];
        private string lastQuery = "";
        private bool initMode = false;

        internal class DoubleClicklessTreeView : TreeView
        {
            public DoubleClicklessTreeView()
            {
                SetStyle(ControlStyles.DoubleBuffer, true);
                SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            }

            protected override void WndProc(ref Message m)
            {
                if (m.Msg == WM.LBUTTONDBLCLK)
                {
                    m.Msg = (int)WM.LBUTTONDOWN;
                }
                base.WndProc(ref m);
            }
        }

        public TreeCategorySelector()
        {
            Debug.Assert(DesignMode);
            InitializeComponent();
        }

        public TreeCategorySelector(CategoryContext ctx)
        {
            this.ctx = ctx;
            InitializeComponent();

            // TODO: Whoops, missed UI Freeze... add this later
            //treeView.AccessibleName = Res.Get(StringId.CategorySelector);

            // On Windows XP, checkboxes and images seem to be mutually exclusive. Vista works fine though.
            if (Environment.OSVersion.Version.Major >= 6)
            {
                imageList.Images.Add(new Bitmap(imageList.ImageSize.Width, imageList.ImageSize.Height));
                treeView.ImageList = imageList;
                treeView.ImageIndex = 0;
            }

            LoadCategories();

            treeView.BeforeCollapse += delegate (object sender, TreeViewCancelEventArgs e) { e.Cancel = true; };
            treeView.AfterCheck += treeView1_AfterCheck;

            treeView.LostFocus += delegate { treeView.Invalidate(); };
        }

        private delegate void Walker(TreeNode n);
        void WalkNodes(TreeNodeCollection nodes, Walker walker)
        {
            foreach (TreeNode n in nodes)
            {
                walker(n);
                WalkNodes(n.Nodes, walker);
            }
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            int width = 0;
            int height = 0;
            WalkNodes(treeView.Nodes, delegate (TreeNode n)
                                          {
                                              width = Math.Max(width, n.Bounds.Right);
                                              height = Math.Max(height, n.Bounds.Bottom);
                                          });

            width += Padding.Left + Padding.Right;
            height += Padding.Top + Padding.Bottom;
            return new Size(width, height);
        }

        void treeView1_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (initMode)
                return;

            List<BlogPostCategory> categories = new List<BlogPostCategory>(ctx.SelectedCategories);
            TreeNode realTreeNode = (TreeNode)e.Node.Tag;
            realTreeNode.Checked = e.Node.Checked;
            BlogPostCategory category = (BlogPostCategory)(realTreeNode.Tag);
            if (e.Node.Checked)
            {
                // Fix bug 587012: Category control can display one category repeatedly if
                // checked category is added from search box after refresh
                categories.Remove(category);
                categories.Add(category);
            }
            else
                categories.Remove(category);
            ctx.SelectedCategories = categories.ToArray();
        }

        public static TreeNode[] CategoriesToNodes(BlogPostCategory[] categories)
        {
            Array.Sort(categories);
            Dictionary<string, TreeNode> catToNode = new Dictionary<string, TreeNode>();
            TreeNode[] allNodes = new TreeNode[categories.Length];
            for (int i = 0; i < categories.Length; i++)
            {
                // TODO: This will need to be rewritten to deal with the fact that
                // ID doesn't work effectively in the face of hierarchy and new categories
                allNodes[i] = new TreeNode(HtmlUtils.UnEscapeEntities(categories[i].Name, HtmlUtils.UnEscapeMode.Default));
                allNodes[i].Tag = categories[i];

                // TODO:
                // This is necessary due to bug in categories, where multiple
                // new categories with the same name (but different parents)
                // have the same ID. When that bug is fixed this check should be
                // replaced with an assertion.
                if (!catToNode.ContainsKey(categories[i].Id))
                    catToNode.Add(categories[i].Id, allNodes[i]);
            }

            for (int i = 0; i < allNodes.Length; i++)
            {
                TreeNode node = allNodes[i];
                string parent = ((BlogPostCategory)node.Tag).Parent;
                if (!string.IsNullOrEmpty(parent) && catToNode.ContainsKey(parent))
                {
                    catToNode[parent].Nodes.Add(node);
                    allNodes[i] = null;
                }
            }
            return (TreeNode[])ArrayHelper.Compact(allNodes);
        }

        private TreeNode[] RealNodes
        {
            get
            {
                return nodes;
            }
        }

        private static TreeNode[] FilteredNodes(IEnumerable nodes, Predicate<TreeNode> predicate)
        {
            List<TreeNode> results = null;
            foreach (TreeNode node in nodes)
            {
                TreeNode[] filteredChildNodes = FilteredNodes(node.Nodes, predicate);
                if (filteredChildNodes.Length > 0 || predicate(node))
                {
                    if (results == null)
                        results = new List<TreeNode>();
                    TreeNode newNode = new TreeNode(node.Text, filteredChildNodes);
                    newNode.Tag = node;
                    newNode.Checked = node.Checked;
                    results.Add(newNode);
                }
            }

            if (results == null)
                return new TreeNode[0];
            else
                return results.ToArray();
        }

        private TreeNode FindFirstMatch(TreeNodeCollection nodes, Predicate<TreeNode> predicate)
        {
            foreach (TreeNode node in nodes)
            {
                if (predicate(node))
                    return node;
                TreeNode child = FindFirstMatch(node.Nodes, predicate);
                if (child != null)
                    return child;
            }
            return null;
        }

        private TreeNode SelectLastNode(TreeNodeCollection nodes)
        {
            if (nodes.Count == 0)
                return null;

            TreeNode lastNode = nodes[nodes.Count - 1];
            return SelectLastNode(lastNode.Nodes) ?? lastNode;
        }

        private bool KeepNodes(ICollection nodes, Predicate<TreeNode> predicate)
        {
            bool keptAny = false;
            // The ArrayList wrapper is to prevent changing the enumerator while
            // still enumerating, which causes bugs
            foreach (TreeNode node in new ArrayList(nodes))
            {
                if (KeepNodes(node.Nodes, predicate) || predicate(node))
                    keptAny = true;
                else
                    node.Remove();
            }
            return keptAny;
        }

        public void LoadCategories()
        {
            initMode = true;
            try
            {
                nodes = CategoriesToNodes(ctx.Categories);
                treeView.Nodes.Clear();
                treeView.Nodes.AddRange(FilteredNodes(RealNodes, delegate { return true; }));
                HashSet selectedCategories = new HashSet();
                selectedCategories.AddAll(ctx.SelectedCategories);
                if (selectedCategories.Count > 0)
                    WalkNodes(treeView.Nodes, delegate (TreeNode n)
                            {
                                n.Checked = selectedCategories.Contains(((TreeNode)n.Tag).Tag as BlogPostCategory);
                            });
                treeView.ExpandAll();
            }
            finally
            {
                initMode = false;
            }
        }

        public void Filter(string criteria)
        {
            treeView.BeginUpdate();
            try
            {
                Predicate<TreeNode> prefixPredicate = delegate (TreeNode node)
                                          {
                                              return node.Text.ToLower(CultureInfo.CurrentCulture).IndexOf(criteria, StringComparison.CurrentCultureIgnoreCase) >= 0;
                                          };

                if (criteria.Length > 0 && criteria.StartsWith(lastQuery))
                {
                    KeepNodes(treeView.Nodes, prefixPredicate);
                }
                else
                {
                    treeView.Nodes.Clear();
                    if (criteria.Length == 0)
                        treeView.Nodes.AddRange(FilteredNodes(RealNodes, delegate { return true; }));
                    else
                    {
                        treeView.Nodes.AddRange(FilteredNodes(RealNodes, prefixPredicate));
                    }
                }
                treeView.ExpandAll();
                if (treeView.Nodes.Count > 0)
                    treeView.Nodes[0].EnsureVisible();

                Predicate<TreeNode> equalityPredicate = delegate (TreeNode n) { return n.Text.ToLower(CultureInfo.CurrentCulture) == criteria; };
                if (treeView.SelectedNode == null || !equalityPredicate(treeView.SelectedNode))
                {
                    TreeNode firstMatch = FindFirstMatch(treeView.Nodes, equalityPredicate);
                    if (firstMatch != null)
                        treeView.SelectedNode = firstMatch;
                    else if (treeView.SelectedNode == null || !prefixPredicate(treeView.SelectedNode))
                    {
                        firstMatch = FindFirstMatch(treeView.Nodes, prefixPredicate);
                        if (firstMatch != null)
                            treeView.SelectedNode = firstMatch;
                    }
                }
            }
            finally
            {
                treeView.EndUpdate();
            }

            lastQuery = criteria;

        }

        public void SelectCategory(BlogPostCategory category)
        {
            WalkNodes(treeView.Nodes, delegate (TreeNode n)
                    {
                        if (category.Equals(((TreeNode)n.Tag).Tag))
                        {
                            if (!n.Checked)
                                n.Checked = true;
                            treeView.SelectedNode = n;
                            n.EnsureVisible();
                        }
                    });
        }

        public void UpArrow()
        {
            TreeNode selectedNode = treeView.SelectedNode;
            if (selectedNode == null)
            {
                if (treeView.Nodes.Count > 0)
                {
                    treeView.SelectedNode = SelectLastNode(treeView.Nodes);
                }
            }
            else
            {
                TreeNode nextNode = selectedNode.PrevVisibleNode;
                if (nextNode != null)
                    treeView.SelectedNode = nextNode;
            }

            if (treeView.SelectedNode != null)
                treeView.SelectedNode.EnsureVisible();

            treeView.Focus();
        }

        public void DownArrow()
        {
            TreeNode selectedNode = treeView.SelectedNode;
            if (selectedNode == null)
            {
                if (treeView.Nodes.Count > 0)
                {
                    treeView.SelectedNode = treeView.Nodes[0];
                }
            }
            else
            {
                TreeNode nextNode = selectedNode.NextVisibleNode;
                if (nextNode != null)
                    treeView.SelectedNode = nextNode;
            }

            if (treeView.SelectedNode != null)
                treeView.SelectedNode.EnsureVisible();

            treeView.Focus();
        }

        void ICategorySelector.Enter()
        {
            if (treeView.SelectedNode != null)
                treeView.SelectedNode.Checked = !treeView.SelectedNode.Checked;
        }

        void ICategorySelector.CtrlEnter()
        {
            if (treeView.SelectedNode != null && !treeView.SelectedNode.Checked)
                treeView.SelectedNode.Checked = true;
            FindForm().Close();
        }
    }
}
