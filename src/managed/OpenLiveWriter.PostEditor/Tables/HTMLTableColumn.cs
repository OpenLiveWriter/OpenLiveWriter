// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Collections;
using System.Globalization;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.HtmlEditor;
using mshtml;

namespace OpenLiveWriter.PostEditor.Tables
{
    public class HTMLTableColumn
    {
        public HTMLTableColumn(IHTMLTable table, IHTMLTableCell baseCell)
        {
            _table = table;
            _baseCell = baseCell;
            _index = _baseCell.cellIndex;
        }

        public int Index
        {
            get
            {
                return _index;
            }
        }

        public IHTMLTableCell BaseCell
        {
            get
            {
                return _baseCell;
            }
        }

        public PixelPercent Width
        {
            get
            {
                return new PixelPercent((string) _baseCell.width, CultureInfo.CurrentCulture);
            }
            set
            {
                ProcessColumnCells(new CellWidthProcessor(value));
            }
        }

        public CellColor BackgroundColor
        {
            get
            {
                CellBackgroundColorReader bgColorReader = new CellBackgroundColorReader();
                ProcessColumnCells(bgColorReader);
                return bgColorReader.BackgroundColor;
            }
            set
            {
                ProcessColumnCells(new CellBackgroundColorWriter(value));
            }
        }

        public HorizontalAlignment HorizontalAlignment
        {
            get
            {
                CellAlignmentReader alignmentReader = new CellAlignmentReader();
                ProcessColumnCells(alignmentReader);
                return alignmentReader.HorizontalAlignment;
            }
            set
            {
                ProcessColumnCells(new CellAlignmentWriter(value));
            }
        }

        public VerticalAlignment VerticalAlignment
        {
            get
            {
                CellVAlignmentReader vAlignmentReader = new CellVAlignmentReader();
                ProcessColumnCells(vAlignmentReader);
                return vAlignmentReader.VerticalAlignment;
            }
            set
            {
                ProcessColumnCells(new CellVAlignmentWriter(value));
            }
        }

        private class CellBackgroundColorReader : IColumnCellProcessor
        {
            bool _firstCellProcessed = false;
            private CellColor _backgroundColor = new CellColor();

            public void ProcessCell(IHTMLTableCell cell)
            {
                // for the first cell processed, note its color
                if (!_firstCellProcessed)
                {
                    _backgroundColor.Color = TableHelper.GetColorForHtmlColor(cell.bgColor);
                    _firstCellProcessed = true;
                }
                // for subsequent cells, if any of them differ from the first cell
                // then the background color is mixed
                else if (!_backgroundColor.IsMixed)
                {
                    if (_backgroundColor.Color != TableHelper.GetColorForHtmlColor(cell.bgColor))
                    {
                        _backgroundColor.IsMixed = true;
                    }
                }
            }

            public CellColor BackgroundColor
            {
                get
                {
                    return _backgroundColor;
                }
            }
        }

        private class CellAlignmentReader : IColumnCellProcessor
        {
            bool _firstCellProcessed = false;
            private HorizontalAlignment _horizontalAlignment;

            public void ProcessCell(IHTMLTableCell cell)
            {
                // for the first cell processed, note its alignment
                if (!_firstCellProcessed)
                {
                    _horizontalAlignment = TableHelper.GetAlignmentForHtmlAlignment(cell.align);
                    _firstCellProcessed = true;
                }
                // for subsequent cells, if any of them differ from the first cell
                // then the alignment is mixed
                else if (_horizontalAlignment != HorizontalAlignment.Mixed)
                {
                    if (_horizontalAlignment != TableHelper.GetAlignmentForHtmlAlignment(cell.align))
                        _horizontalAlignment = HorizontalAlignment.Mixed;
                }
            }

            public HorizontalAlignment HorizontalAlignment
            {
                get
                {
                    return _horizontalAlignment;
                }
            }
        }

        private class CellVAlignmentReader : IColumnCellProcessor
        {
            bool _firstCellProcessed = false;
            private VerticalAlignment _verticalAlignment;

            public void ProcessCell(IHTMLTableCell cell)
            {
                // for the first cell processed, note its alignment
                if (!_firstCellProcessed)
                {
                    _verticalAlignment = TableHelper.GetVAlignmentForHtmlAlignment(cell.vAlign);
                    _firstCellProcessed = true;
                }
                // for subsequent cells, if any of them differ from the first cell
                // then the alignment is mixed
                else if (_verticalAlignment != VerticalAlignment.Mixed) // optimize
                {
                    if (_verticalAlignment != TableHelper.GetVAlignmentForHtmlAlignment(cell.vAlign))
                        _verticalAlignment = VerticalAlignment.Mixed;
                }
            }

            public VerticalAlignment VerticalAlignment
            {
                get
                {
                    return _verticalAlignment;
                }
            }
        }

        private class CellBackgroundColorWriter : IColumnCellProcessor
        {
            private CellColor _backgroundColor;
            public CellBackgroundColorWriter(CellColor backgroundColor) { _backgroundColor = backgroundColor; }

            public void ProcessCell(IHTMLTableCell cell)
            {
                // mixed means leave the cells alone
                if (!_backgroundColor.IsMixed)
                {
                    if (_backgroundColor.Color != Color.Empty)
                    {
                        cell.bgColor = ColorHelper.ColorToString(_backgroundColor.Color);
                    }
                    else
                    {
                        (cell as IHTMLElement).removeAttribute("bgcolor", 0);
                    }
                }
            }
        }

        private class CellAlignmentWriter : IColumnCellProcessor
        {
            private HorizontalAlignment _alignment;
            public CellAlignmentWriter(HorizontalAlignment alignment) { _alignment = alignment; }

            public void ProcessCell(IHTMLTableCell cell)
            {
                // mixed means leave the cells alone
                if (_alignment != HorizontalAlignment.Mixed)
                {
                    if (_alignment != HorizontalAlignment.Left)
                        cell.align = TableHelper.GetHtmlAlignmentForAlignment(_alignment);
                    else
                        (cell as IHTMLElement).removeAttribute("align", 0);
                }
            }
        }

        private class CellVAlignmentWriter : IColumnCellProcessor
        {
            private VerticalAlignment _alignment;
            public CellVAlignmentWriter(VerticalAlignment alignment) { _alignment = alignment; }

            public void ProcessCell(IHTMLTableCell cell)
            {
                // mixed means leave the cells alone
                if (_alignment != VerticalAlignment.Mixed)
                {
                    if (_alignment != VerticalAlignment.Middle)
                        cell.vAlign = TableHelper.GetHtmlAlignmentForVAlignment(_alignment);
                    else
                        (cell as IHTMLElement).removeAttribute("valign", 0);
                }
            }
        }

        private class CellWidthProcessor : IColumnCellProcessor
        {
            private PixelPercent _width;
            public CellWidthProcessor(PixelPercent width) { _width = width; }

            public void ProcessCell(IHTMLTableCell cell)
            {
                if (_width > 0 && _width.Units != PixelPercentUnits.Undefined)
                {
                   
                    cell.width = _width.ToString(CultureInfo.InvariantCulture);
                }
                else
                {
                    (cell as IHTMLElement).removeAttribute("width", 0);
                }
            }
        }

        private interface IColumnCellProcessor
        {
            void ProcessCell(IHTMLTableCell cell);
        }

        private void ProcessColumnCells(IColumnCellProcessor cellProcessor)
        {
            // set the specified alignment for each cell in the column
            foreach (IHTMLTableRow row in _table.rows)
            {
                if (row.cells.length > Index)
                {
                    IHTMLTableCell cell = row.cells.item(Index, Index) as IHTMLTableCell;
                    cellProcessor.ProcessCell(cell);
                }
            }
        }

        private IHTMLTable _table;
        private IHTMLTableCell _baseCell;
        private int _index;
    }

}
