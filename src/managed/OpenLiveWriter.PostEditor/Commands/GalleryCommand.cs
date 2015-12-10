// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.HtmlEditor;
using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.Interop.Com.Ribbon;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.Commands
{
    public class GalleryCommand<T> : PreviewCommand where T : IComparable
    {
        protected const int INVALID_INDEX = -1;
        protected const uint UI_COLLECTION_INVALIDINDEX = 0xffffffff;

        private bool _allowPreview = true;
        private T _defaultItem = default(T);

        public GalleryCommand(CommandId commandId)
            : base(commandId)
        {
            Initialize();
        }

        public GalleryCommand(CommandId commandId, T defaultItem)
            : base(commandId)
        {
            _defaultItem = defaultItem;
            Initialize();
        }

        public GalleryCommand(CommandId commandId, bool allowPreview)
            : base(commandId)
        {
            _allowPreview = allowPreview;
            Initialize();
        }

        private void Initialize()
        {
            UpdateInvalidationState(PropertyKeys.Enabled, InvalidationState.Pending);
            OnStartPreview += new EventHandler(GalleryCommand_OnStartPreview);
            OnCancelPreview += new EventHandler(GalleryCommand_OnCancelPreview);
        }

        private int savedSelectedIndex = INVALID_INDEX;
        void GalleryCommand_OnStartPreview(object sender, EventArgs e)
        {
            // This gives us a chance to save away state.
            savedSelectedIndex = selectedIndex;
        }

        void GalleryCommand_OnCancelPreview(object sender, EventArgs e)
        {
            // This gives us a chance to restore state.
            SelectedIndex = savedSelectedIndex;
        }

        protected GalleryItems items = new GalleryItems();
        public virtual GalleryItems Items
        {
            get { return items; }
            set { items = value; }
        }

        public virtual void LoadItems() { UpdateInvalidationState(PropertyKeys.ItemsSource, InvalidationState.Pending); }

        public override void Invalidate()
        {
            if (_inPreview)
                return;

            selectedIndex = INVALID_INDEX;
            selectedItem = _defaultItem;

            LoadItems();

            // selectedIndex should correspond to the selectedItem
            this.selectedIndex = INVALID_INDEX;
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Cookie.CompareTo(selectedItem) == 0)
                {
                    selectedIndex = i;
                    break;
                }
            }

            InvalidateSelectedItemProperties();

            base.Invalidate();
        }

        protected int selectedIndex = INVALID_INDEX;
        public virtual int SelectedIndex
        {
            get { return selectedIndex; }
            set
            {
                OnExecute(new ExecuteEventHandlerArgs(CommandId.ToString(), value));
            }
        }

        private bool _attemptingReload = false;
        public virtual void SetSelectedItem(T selectedItem)
        {
            this.selectedItem = selectedItem;
            this.selectedIndex = INVALID_INDEX;
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Cookie.CompareTo(selectedItem) == 0)
                {
                    selectedIndex = i;
                    break;
                }
            }

            if (selectedIndex == INVALID_INDEX && !_attemptingReload)
            {
                // We may need to reload the items
                // For example, when adding a new blog account we wouldn't receive the
                // blog list changed event until after we had processed the blog switch
                // In that case, we attempt to switch to a blog that has not yet been
                // loaded into the gallery items, so we need to reload the list.
                _attemptingReload = true;
                items.Clear();
                Invalidate();
                SetSelectedItem(selectedItem);
                _attemptingReload = false;
            }

            Debug.Assert(selectedIndex != INVALID_INDEX, "That item was not in the gallery!");

            InvalidateSelectedItemProperties();

            OnStateChanged(EventArgs.Empty);
        }

        protected bool _invalidateGalleryRepresentation = false;

        protected void InvalidateSelectedItemProperties()
        {
            UpdateInvalidationState(PropertyKeys.SelectedItem, InvalidationState.Pending);

            if (_invalidateGalleryRepresentation)
            {
                UpdateInvalidationState(PropertyKeys.StringValue, InvalidationState.Pending);
                UpdateInvalidationState(PropertyKeys.Label, InvalidationState.Pending);
                UpdateInvalidationState(PropertyKeys.SmallImage, InvalidationState.Pending);
                UpdateInvalidationState(PropertyKeys.SmallHighContrastImage, InvalidationState.Pending);
                UpdateInvalidationState(PropertyKeys.LargeImage, InvalidationState.Pending);
                UpdateInvalidationState(PropertyKeys.LargeHighContrastImage, InvalidationState.Pending);
            }
        }

        protected T selectedItem;
        /// <summary>
        /// This default behavior on the setter is to trigger a command execution.
        /// Use SetSelectedItem if you want to set the selectedItem without a command execution.
        /// </summary>
        public T SelectedItem
        {
            get { return selectedItem; }
            set
            {
                if (SelectedIndex == INVALID_INDEX || selectedItem.CompareTo(value) != 0)
                {
                    int itemIndex = INVALID_INDEX;
                    for (int i = 0; i < Items.Count; i++)
                    {
                        T itemValue = Items[i].Cookie;
                        if (itemValue.CompareTo(value) == 0)
                        {
                            itemIndex = i;
                            break;
                        }
                    }

                    if (itemIndex != INVALID_INDEX)
                        selectedItem = value;
                    else
                        selectedItem = _defaultItem;

                    InvalidateSelectedItemProperties();

                    SelectedIndex = itemIndex;
                }
            }
        }

        private bool allowSelection = true;
        public bool AllowSelection
        {
            get { return allowSelection; }
            set { allowSelection = value; }
        }

        protected bool AllowExecuteOnInvalidIndex = false;

        protected override void OnExecute(ExecuteEventHandlerArgs args)
        {
            selectedIndex = args.GetInt(CommandId.ToString());
            if (AllowExecuteOnInvalidIndex || selectedIndex != INVALID_INDEX)
                base.OnExecute(new ExecuteEventHandlerArgs(CommandId.ToString(), selectedIndex));

            if (!AllowSelection)
                Invalidate();

            OnStateChanged(EventArgs.Empty);
        }

        /// <summary>
        /// Used by ribbon if the selection index is invalid
        /// </summary>
        public virtual string StringValue
        {
            get
            {
                if (SelectedIndex != INVALID_INDEX)
                {
                    string label = items[SelectedIndex].Label;
                    if (label != null)
                        return label;
                }

                return String.Empty;
            }
        }

        protected GalleryItems _categories = new GalleryItems();
        public virtual GalleryItems Categories
        {
            get { return _categories; }
            set { _categories = value; }
        }

        public override void GetPropVariant(PropertyKey key, PropVariantRef currentValue, ref PropVariant value)
        {
            if (key == PropertyKeys.ItemsSource)
            {
                LoadItemsSource(Items, null, ref value);
            }
            else if (key == PropertyKeys.StringValue)
            {
                value = new PropVariant(StringValue);
            }
            else if (key == PropertyKeys.SelectedItem)
            {
                value.SetUInt((SelectedIndex == INVALID_INDEX ? UI_COLLECTION_INVALIDINDEX : (uint)SelectedIndex));
            }
            else if (key == PropertyKeys.Categories)
            {
                LoadItemsSource(Categories, currentValue, ref value);
            }
            else
            {
                base.GetPropVariant(key, currentValue, ref value);
            }
        }

        protected void LoadSimplePropertySet(GalleryItem item, out SimplePropertySet sps)
        {
            sps = new SimplePropertySet();
            Debug.Assert(item.Label != null || item.IUIImage != null);

            if (item.Label != null)
            {
                sps.Add(PropertyKeys.Label, new PropVariant(item.Label));
            }

            if (item.IUIImage != null)
            {
                PropVariant image = new PropVariant(item.IUIImage);

                if (image.VarType == VarEnum.VT_UNKNOWN)
                    sps.Add(PropertyKeys.ItemImage, image);
            }

            sps.Add(PropertyKeys.CategoryId, new PropVariant(item.CategoryIndex));
        }

        public int LoadItemsSource(GalleryItems items, PropVariantRef currentValue, ref PropVariant newValue)
        {
            if (currentValue != null && currentValue.PropVariant.Value is IUICollection)
            {
                IUICollection collection = (IUICollection)currentValue.PropVariant.Value;
                collection.Clear();

                foreach (GalleryItem item in items)
                {
                    SimplePropertySet sps;
                    LoadSimplePropertySet(item, out sps);
                    collection.Add(sps);
                }
                newValue.SetIUnknown(collection);
                return HRESULT.S_OK;
            }

            List<IUISimplePropertySet> list = new List<IUISimplePropertySet>();
            foreach (GalleryItem item in items)
            {
                SimplePropertySet sps;
                LoadSimplePropertySet(item, out sps);
                list.Add(sps);
            }
            newValue.SetIUnknown(new BasicCollection(list));
            return HRESULT.S_OK;
        }

        public override int PerformExecute(CommandExecutionVerb verb, PropertyKeyRef key, PropVariantRef currentValue, IUISimplePropertySet commandExecutionProperties)
        {
            int index = -1;
            if ((uint)currentValue.PropVariant.Value != UI_COLLECTION_INVALIDINDEX)
            {
                index = Convert.ToInt32(currentValue.PropVariant.Value, CultureInfo.InvariantCulture);
            }

            PerformExecuteWithArgs(verb, new ExecuteEventHandlerArgs(CommandId.ToString(), index));
            return HRESULT.S_OK;
        }

        protected override void PerformExecuteWithArgs(CommandExecutionVerb verb, ExecuteEventHandlerArgs args)
        {
            if (_allowPreview || verb == CommandExecutionVerb.Execute)
                base.PerformExecuteWithArgs(verb, args);
        }

        public class GalleryItem
        {
            private uint categoryIndex;
            private T cookie;
            private string label;
            private Bitmap bitmap;
            private IUIImage iuiImage;

            public GalleryItem(string label, uint categoryIndex)
                : this(label, null, default(T), categoryIndex)
            {
            }

            public GalleryItem(string label, Bitmap bitmap, T cookie)
                : this(label, bitmap, cookie, UI_COLLECTION_INVALIDINDEX)
            {
            }

            public GalleryItem(string label, Bitmap bitmap, T cookie, uint categoryIndex)
            {
                this.label = TextHelper.EscapeAmpersands(label);
                this.bitmap = bitmap;
                this.cookie = cookie;
                this.categoryIndex = categoryIndex;
            }

            ~GalleryItem()
            {
                if (bitmap != null)
                {
                    bitmap.Dispose();
                }
            }

            public uint CategoryIndex
            {
                get { return categoryIndex; }
            }

            public T Cookie
            {
                get
                {
                    return cookie;
                }
            }

            public string Label
            {
                get
                {
                    return label;
                }
                set
                {
                    label = value;
                }
            }

            public Bitmap Image
            {
                get
                {
                    return bitmap;
                }
                set
                {
                    if (bitmap != null)
                    {
                        bitmap.Dispose();
                        iuiImage = null;
                    }
                    bitmap = value;
                }
            }

            public IUIImage IUIImage
            {
                get
                {
                    if (iuiImage == null && bitmap != null)
                    {
                        iuiImage = RibbonHelper.CreateImage(bitmap.GetHbitmap(), ImageCreationOptions.Transfer);
                    }

                    return iuiImage;
                }
            }
        }

        public class TooltippedGalleryItem : GalleryItem
        {
            private string labelDescription;
            public string LabelDescription { get { return labelDescription; } }

            public TooltippedGalleryItem(string label, string labelDescription, Bitmap bitmap, T cookie)
                : base(label, bitmap, cookie)
            {
                this.labelDescription = TextHelper.EscapeAmpersands(labelDescription);
            }

            public TooltippedGalleryItem(string label, string labelDescription, Bitmap bitmap, T cookie, uint categoryIndex)
                : base(label, bitmap, cookie, categoryIndex)
            {
                this.labelDescription = TextHelper.EscapeAmpersands(labelDescription);
            }
        }

        public class GalleryItems : List<GalleryItem>
        {
        }
    }

    public class PreviewCommand : OverridableCommand
    {
        public PreviewCommand(CommandId commandId)
            : base(commandId)
        {
        }

        protected override void PerformExecuteWithArgs(CommandExecutionVerb verb, ExecuteEventHandlerArgs args)
        {
            switch (verb)
            {
                case CommandExecutionVerb.Execute:
                    PerformExecuteWithArgs(args);
                    break;
                case CommandExecutionVerb.Preview:
                    FireStartPreview(args);
                    break;
                case CommandExecutionVerb.CancelPreview:
                    FireCancelPreview();
                    break;
                default:
                    Debug.Fail("Unexpected CommandExecutionVerb!");
                    break;
            }
        }

        // Fired before preview starts, allowing a command to save away state
        protected event EventHandler OnStartPreview;

        // Fired upon preview cancel, allowing a command to restore state.
        protected event EventHandler OnCancelPreview;

        protected bool _inPreview = false;
        private IUndoUnit undoUnit;

        protected virtual void Preview(ExecuteEventHandlerArgs args)
        {
            PerformExecuteWithArgs(args);
        }

        public delegate IUndoUnit CreateUndoUnitDelegate();

        public IUndoUnit CreateUndoUnit { get; set; }

        public IHtmlEditorComponentContextDelegate ComponentContext { get; set; }

        private IUndoUnit CreateInvisibleUndoUnit()
        {
            return ComponentContext != null ? ComponentContext().CreateInvisibleUndoUnit() : null;
        }

        public void FireStartPreview(ExecuteEventHandlerArgs args)
        {
            if (undoUnit == null)
            {
                undoUnit = CreateInvisibleUndoUnit();
                Debug.Assert(undoUnit != null, "You haven't provided a way for " + CommandId + " to create undo units.  Won't work.");

            }

            if (!_inPreview)
            {
                _inPreview = true;
                if (OnStartPreview != null)
                    OnStartPreview(this, EventArgs.Empty);
            }

            Preview(args);
        }

        public void FireCancelPreview()
        {
            try
            {
                if (OnCancelPreview != null && _inPreview)
                    OnCancelPreview(this, EventArgs.Empty);
            }
            finally
            {
                if (undoUnit != null)
                {
                    undoUnit.Commit();
                    // If we commit, then applying h1 can leave text split apart.
                    // If we don't commit, then we end up with the Redo stack changing.
                    undoUnit.Dispose();
                    undoUnit = null;
                }
                _inPreview = false;
            }
        }
    }
}
