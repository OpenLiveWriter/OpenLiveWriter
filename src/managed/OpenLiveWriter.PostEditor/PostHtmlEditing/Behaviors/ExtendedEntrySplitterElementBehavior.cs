// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Windows.Forms;
using mshtml;
using OpenLiveWriter.ApplicationFramework.Skinning;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.HtmlEditor;
using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Localization.Bidi;
using OpenLiveWriter.Mshtml;
using OpenLiveWriter.PostEditor.PostHtmlEditing.Behaviors;
using OpenLiveWriter.PostEditor.PostHtmlEditing.Sidebar;
using OpenLiveWriter.PostEditor.ContentSources;

namespace OpenLiveWriter.PostEditor.PostHtmlEditing.Behaviors
{
    /// <summary>
    /// Summary description for ExtendedEntrySplitterElementBehavior.
    /// </summary>
    internal class ExtendedEntrySplitterElementBehavior : ElementControlBehavior
    {
        private SplitterControl _splitter;

        // _splitterHeight and _verticalPadding measured in 96dpi pixels
        private const int _splitterHeight = 16;
        private const int _verticalPadding = 2;

        public ExtendedEntrySplitterElementBehavior(IHtmlEditorComponentContext editorContext)
            : base(editorContext)
        {
            _splitter = new SplitterControl(_splitterHeight);
            _splitter.VirtualLocation = new Point(0, _verticalPadding);
            Controls.Add(_splitter);

            //keep the line width synchronized with the element's width
            ElementSizeChanged += new EventHandler(ExtendedEntrySplitterElementBehavior_ElementSizeChanged);
        }

        protected override void OnElementAttached()
        {
            // Padding is applied to both top and bottom, so halve the splitter height
            int cssVerticalPadding = DisplayHelper.ScaleYCeil(_splitterHeight / 2 + _verticalPadding);

            IHTMLElement2 e2 = (IHTMLElement2)HTMLElement;
            e2.runtimeStyle.padding = $"{cssVerticalPadding}px 0px {cssVerticalPadding}px 0px"; 
            e2.runtimeStyle.margin = "0px 0px 0px 0px";
            e2.runtimeStyle.lineHeight = "16px";
            
            (e2 as IHTMLElement3).contentEditable = "false";
            base.OnElementAttached();

            //Remove any duplicate extended entries that may have been introduced (including maybe this one!)
            RemoveDuplicateExtendedEntries();
        }

        /// <summary>
        /// Removes duplicate extended entries that may exist in the document.
        /// </summary>
        private void RemoveDuplicateExtendedEntries()
        {
            IHTMLDocument3 doc3 = (IHTMLDocument3)HTMLElement.document;
            IHTMLElementCollection extendedEntryElements = doc3.getElementsByName(PostBodyEditingElementBehavior.EXTENDED_ENTRY_ID);
            if (extendedEntryElements.length > 1)
            {
                //delete any extended entry markers that are not this element (use a timer to avoid bug 407544)
                ExtendedEntrySweeper extendedEntrySweeper = new ExtendedEntrySweeper(EditorContext, (IHTMLDocument3)HTMLElement.document, HTMLElement);
                TimerHelper.CallbackOnDelay(new InvokeInUIThreadDelegate(extendedEntrySweeper.DoDelete), 10);
            }
        }

        protected override void OnSelectedChanged()
        {
            PerformLayout();
            Invalidate();
        }

        protected override bool QueryElementSelected()
        {
            if (HTMLElement.sourceIndex > 0) //Hack! avoid exceptions caused by the fact that we delete during attach
            {
                try
                {
                    MarkupRange selectedRange = EditorContext.Selection.SelectedMarkupRange;
                    MarkupRange splitterRange = ElementRange;
                    return selectedRange.Start.IsEqualTo(splitterRange.Start) && selectedRange.End.IsEqualTo(splitterRange.End);
                }
                catch (Exception) { } //the fact that we delete during attach causes exceptions here, so eat them.
            }
            return false;
        }

        private void ExtendedEntrySplitterElementBehavior_ElementSizeChanged(object sender, EventArgs e)
        {
            //synchronized the splitter width with the new element width
            _splitter.VirtualWidth = ElementRectangle.Width;
        }

        protected override void OnKeyDown(HtmlEventArgs e)
        {
            base.OnKeyDown(e);
            if (((Keys)e.htmlEvt.keyCode) == Keys.Back)
            {
                using (IUndoUnit undo = EditorContext.CreateUndoUnit())
                {
                    (HTMLElement as IHTMLDOMNode).removeNode(true);
                    undo.Commit();
                }
                e.Cancel();
            }
        }

        protected override void Dispose(bool disposeManagedResources)
        {
            if (!_disposed)
            {
                if (disposeManagedResources)
                {
                    ElementSizeChanged -= new EventHandler(ExtendedEntrySplitterElementBehavior_ElementSizeChanged);
                }

                _disposed = true;
            }

            base.Dispose(disposeManagedResources);
        }
        private bool _disposed;

        /// <summary>
        /// Deletes extra extended
        /// </summary>
        private class ExtendedEntrySweeper
        {
            private IHtmlEditorComponentContext _editorContext;
            private IHTMLDocument3 _document;
            private IHTMLElement _realExtendedEntry;
            public ExtendedEntrySweeper(IHtmlEditorComponentContext editorContext, IHTMLDocument3 document, IHTMLElement extendedEntry)
            {
                _editorContext = editorContext;
                _realExtendedEntry = extendedEntry;
                _document = document;
            }

            public void DoDelete()
            {
                if (_realExtendedEntry.sourceIndex == 0) //this "real" entry is deleted
                    return;

                using (IUndoUnit undo = _editorContext.CreateInvisibleUndoUnit())
                {
                    IHTMLElementCollection extendedEntryElements = _document.getElementsByName(PostBodyEditingElementBehavior.EXTENDED_ENTRY_ID);
                    foreach (IHTMLElement element in extendedEntryElements)
                    {
                        if (element.sourceIndex != _realExtendedEntry.sourceIndex)
                        {
                            if (element.sourceIndex > -1) //just in case its already been deleted
                                (element as IHTMLDOMNode).removeNode(true);
                        }
                    }

                    undo.Commit();
                }
            }
        }
    }

    /// <summary>
    /// The "More..." splitter for the extended entry break
    /// </summary>
    internal class SplitterControl : BehaviorControl
    {
        private const string IMAGE_RESOURCE_PATH = "PostHtmlEditing.Behaviors.Images.";
        private Bitmap extendedEntrySeparatorTileImage = ResourceHelper.LoadAssemblyResourceBitmap(IMAGE_RESOURCE_PATH + "ExtendedEntrySeparatorTile.png");

        private Rectangle _controlRect;
        private Rectangle _lineRect;
        private string _moreText = Res.Get(StringId.SplitterMore);
        private Font font = Res.GetFont(FontSize.PostSplitCaption, FontStyle.Regular);

        /// <summary>
        /// Instansiates a new SplitterControl
        /// </summary>
        /// <param name="virtualHeight96">The 'virtual height' of the splitter in 96 DPI pixels.
        /// Subtracting _lineRect.Height from this value yields the height of the 'More' rectangle on the end of the splitter line.</param>
        public SplitterControl(int virtualHeight96)
        {
            VirtualHeight = DisplayHelper.ScaleYCeil(virtualHeight96);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            BidiGraphics g = new BidiGraphics(e.Graphics, _controlRect);

            //draw the splitter line
            GraphicsHelper.TileFillScaledImageHorizontally(g, extendedEntrySeparatorTileImage, _lineRect);

            //draw the More... box
            Color backgroundColor = Color.FromArgb(128, 128, 128);
            SizeF moreTextSize = g.MeasureText(_moreText, font);
            int morePadding = 0;
            int moreRightOffset = 1;

            Size moreRectSize = new Size(Convert.ToInt32(moreTextSize.Width) + DisplayHelper.ScaleXCeil(morePadding * 2), 
                                         VirtualHeight - DisplayHelper.ScaleYCeil(_lineRect.Height));
            Point moreRectLocation = new Point(_controlRect.Right - moreRectSize.Width - moreRightOffset, _lineRect.Bottom);
            Rectangle moreRect = new Rectangle(moreRectLocation, moreRectSize);

            // It's necessary to double-buffer the text drawing, because GDI
            // text drawing doesn't work well in these behavior controls.
            using (Bitmap bitmap = new Bitmap(moreRect.Width, moreRect.Height))
            {
                using (Graphics moreG = Graphics.FromImage(bitmap))
                {
                    using (SolidBrush b = new SolidBrush(backgroundColor))
                        moreG.FillRectangle(b, 0, 0, moreRect.Width, moreRect.Height);
                    new BidiGraphics(moreG, moreRect.Size).DrawText(_moreText, font, new Rectangle(Point.Empty, moreRect.Size), Color.White, backgroundColor, TextFormatFlags.Default);
                }

                g.DrawImage(false, bitmap, moreRect);
            }
        }

        protected override void OnLayout(EventArgs e)
        {
            _controlRect = new Rectangle(VirtualLocation, new Size(VirtualWidth, VirtualHeight));
            _lineRect = new Rectangle(new Point(0, 0), new Size(_controlRect.Width, DisplayHelper.ScaleYCeil(2)));
        }
    }
}
