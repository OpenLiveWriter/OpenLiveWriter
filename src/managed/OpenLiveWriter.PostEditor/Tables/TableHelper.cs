// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Globalization;
using System.Diagnostics;
using OpenLiveWriter.Mshtml;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.PostEditor.ContentSources;
using mshtml;

namespace OpenLiveWriter.PostEditor.Tables
{
    internal sealed class TableHelper
    {
        public static IHTMLTable GetContainingTableElement(IHTMLElement element)
        {
            // search up the parent heirarchy
            while (element != null)
            {
                if (element is IHTMLTable)
                {
                    return element as IHTMLTable;
                }

                // search parent
                element = element.parentElement;
            }

            // didn't find a table
            return null;
        }

        public static IHTMLTableRow GetContainingRowElement(IHTMLTableCell cell)
        {
            // search up the parent heirarchy
            IHTMLElement element = cell as IHTMLElement;
            while (element != null)
            {
                if (element is IHTMLTableRow)
                {
                    return element as IHTMLTableRow;
                }

                // search parent
                element = element.parentElement;
            }

            // didn't find a table row
            return null;
        }

        public static int GetCellWidth(IHTMLTableCell cell)
        {
            if (cell.width != null)
            {
                try
                {
                    return int.Parse(cell.width.ToString(), CultureInfo.InvariantCulture);
                }
                catch
                {
                    return (cell as IHTMLElement).offsetWidth;
                }
            }
            else
            {
                return (cell as IHTMLElement).offsetWidth;
            }
        }

        public static int GetParentContainerBlockWidth(MarkupRange markupRange)
        {
            IHTMLElement2 parentBlock = markupRange.Start.GetParentElement(ElementFilters.BLOCK_OR_TABLE_CELL_ELEMENTS) as IHTMLElement2;
            if (parentBlock != null)
            {
                // TODO: we would like to always clientWidth here however for an empty block element this will
                // be zero. So in this case we use scrollWidth which should be a proxy except in the case where
                // the parent element has a horizontal scroll bar (in which case we may insert a table which
                // is worst case too narrow). What we "should" do is insert and remove some bogus content
                // within the block elemet to force its clientWidth to the right value.
                int blockWidth = parentBlock.clientWidth;

                if (blockWidth == 0)
                    blockWidth = parentBlock.scrollWidth;

                return blockWidth;
            }
            else
            {
                return 0;
            }
        }

        public static PixelPercent GetTableWidth(IHTMLTable table)
        {
            if (table.width != null)
            {
                try
                {
                    return new PixelPercent(table.width.ToString(), CultureInfo.InvariantCulture);
                }
                catch
                {
                    return new PixelPercent();
                }
            }
            else
            {
                return new PixelPercent();
            }
        }

        public static int GetRowHeight(IHTMLTableRow row)
        {
            IHTMLTableRow2 row2 = row as IHTMLTableRow2;
            if (row2.height != null)
            {
                try
                {
                    return int.Parse(row2.height.ToString(), CultureInfo.InvariantCulture);
                }
                catch
                {
                    return 0;
                }
            }
            else
            {
                return 0;
            }
        }

        public static PixelPercent GetTableLogicalEditingWidth(IHTMLTable table)
        {
            // If percentage, then just keep that
            var width = GetTableWidth(table);

            if (width.Units == PixelPercentUnits.Percentage || width.Units == PixelPercentUnits.Undefined)
                return width;

            // value to return (default to zero)
            int logicalWidth = 0;

            // calculate the "logical" width of the table
            if (table.rows.length > 0)
            {
                // save value of cellSpacing
                int cellSpacing = GetAttributeAsInteger(table.cellSpacing);

                // use the first row as a proxy for the width of the table
                IHTMLTableRow firstRow = table.rows.item(0, 0) as IHTMLTableRow;
                foreach (IHTMLTableCell cell in firstRow.cells)
                    logicalWidth += GetCellWidth(cell) + cellSpacing;

                // total width + extra cellspacing @ end + size of borders
                return logicalWidth + cellSpacing + GetTableBorderEditingOffset(table);
            }

            // return width
            return logicalWidth;
        }

        public static void SynchronizeCellWidthsForEditing(IHTMLTable table)
        {
            // resize the cells in the table to match their actual width
            foreach (IHTMLTableCell cell in (table as IHTMLElement2).getElementsByTagName("th"))
                SynchronizeCellWidthForEditing(cell);
            foreach (IHTMLTableCell cell in (table as IHTMLElement2).getElementsByTagName("td"))
                SynchronizeCellWidthForEditing(cell);
        }

        public static void SynchronizeCellWidthForEditing(IHTMLTableCell cell)
        {
            if (GetCellWidth(cell) != ((cell as IHTMLElement).offsetWidth))
                cell.width = (cell as IHTMLElement).offsetWidth;
        }

        public static void SynchronizeTableWidthForEditing(IHTMLTable table)
        {
            var logicalWidth = TableHelper.GetTableLogicalEditingWidth(table);
            if (logicalWidth > 0 && logicalWidth.Units != PixelPercentUnits.Undefined)
                table.width = logicalWidth.ToString();
            else
                (table as IHTMLElement).removeAttribute("width", 0);
        }

        public static void SynchronizeCellAndTableWidthsForEditing(IHTMLTable table)
        {
            SynchronizeCellWidthsForEditing(table);
            SynchronizeTableWidthForEditing(table);
        }

        public static Color GetColorForHtmlColor(object color)
        {
            if (color == null)
                return Color.Empty;
            else
                return ColorHelper.StringToColor(color.ToString());
        }

        public static HorizontalAlignment GetAlignmentForHtmlAlignment(string alignment)
        {
            if (alignment == null)
                return HorizontalAlignment.Left;
            else if (alignment == "left")
                return HorizontalAlignment.Left;
            else if (alignment == "center")
                return HorizontalAlignment.Center;
            else if (alignment == "right")
                return HorizontalAlignment.Right;
            else
                return HorizontalAlignment.Left;
        }

        public static string GetHtmlAlignmentForAlignment(HorizontalAlignment alignment)
        {
            switch (alignment)
            {
                case HorizontalAlignment.Left:
                    return "left";
                case HorizontalAlignment.Center:
                    return "center";
                case HorizontalAlignment.Right:
                    return "right";
                default:
                    return String.Empty;
            }
        }

        public static VerticalAlignment GetVAlignmentForHtmlAlignment(string alignment)
        {
            if (alignment == null)
                return VerticalAlignment.Middle;
            else if (alignment == "top")
                return VerticalAlignment.Top;
            else if (alignment == "middle")
                return VerticalAlignment.Middle;
            else if (alignment == "bottom")
                return VerticalAlignment.Bottom;
            else
                return VerticalAlignment.Middle;
        }

        public static string GetHtmlAlignmentForVAlignment(VerticalAlignment alignment)
        {
            switch (alignment)
            {
                case VerticalAlignment.Top:
                    return "top";
                case VerticalAlignment.Middle:
                    return "middle";
                case VerticalAlignment.Bottom:
                    return "bottom";
                default:
                    return String.Empty;
            }
        }

        public static void UpdateDesignTimeBorders(IHTMLTable table)
        {
            // update the table's borders
            UpdateDesignTimeBorders(table, table as IHTMLElement2);

            // update the contained cell borders
            foreach (IHTMLTableRow row in table.rows)
                foreach (IHTMLTableCell cell in row.cells)
                    UpdateDesignTimeBorders(table, cell as IHTMLElement2);
        }

        public static void UpdateDesignTimeBorders(IHTMLTable table, IHTMLElement2 tableElement)
        {
            // don't do anything if there is a css-based border on this element
            if ((tableElement as IHTMLElement).style.borderStyle != null)
                return;

            // don't attach if is there a standard table border
            if (table.border != null && table.border.ToString() != "0")
            {
                RemoveDesignTimeBorder(table, tableElement);
            }
            else
            {
                AttachDesignTimeBorder(table, tableElement);
            }
        }

        private static void AttachDesignTimeBorder(IHTMLTable table, IHTMLElement2 tableElement)
        {
            // attach design time border
            tableElement.runtimeStyle.borderWidth = "1";
            tableElement.runtimeStyle.borderColor = "#BCBCBC";
            tableElement.runtimeStyle.borderStyle = "dotted";

            // collapse cells if there is no cellspacing
            if ((table.cellSpacing == null) || table.cellSpacing.ToString() != "0")
                (tableElement.runtimeStyle as IHTMLStyle2).borderCollapse = "separate";
            else
                (tableElement.runtimeStyle as IHTMLStyle2).borderCollapse = "collapse";
        }

        private static void RemoveDesignTimeBorder(IHTMLTable table, IHTMLElement2 tableElement)
        {
            IHTMLElement element = tableElement as IHTMLElement;
            tableElement.runtimeStyle.borderWidth = element.style.borderWidth;
            tableElement.runtimeStyle.borderColor = element.style.borderColor;
            tableElement.runtimeStyle.borderStyle = element.style.borderStyle;
            (tableElement.runtimeStyle as IHTMLStyle2).borderCollapse = (element.style as IHTMLStyle2).borderCollapse;
        }

        public static int GetTableBorderEditingOffset(IHTMLTable table)
        {
            int borderOffset = GetAttributeAsInteger(table.border) * 2;
            if (borderOffset == 0)
            {
                // respect css border width
                IHTMLElement tableElement = table as IHTMLElement;
                borderOffset = GetAttributeAsInteger(tableElement.style.borderWidth) * 2;

                // if no css border width, we know the total border width is 2 (b/c we
                // add a one pixel border for editing)
                if (borderOffset == 0)
                    borderOffset = 2;
            }

            // return width
            return borderOffset;
        }

        public static int GetAttributeAsInteger(object value)
        {
            if (value != null)
            {
                try
                {
                    return int.Parse(value.ToString(), CultureInfo.InvariantCulture);
                }
                catch
                {
                    return 0;
                }
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Only edit tables that are not contained within SmartContent blocks and which
        /// are marked with the "unselectable"  attribute. Since this is an attribute which
        /// applies only to editing scenarios it is almost certain never to enter the editor
        /// "from the wild" so it is a  reasonable way to determine whether we created the
        /// table (and thus can guarantee that it conforms to our editing capabilities). The
        /// only other reasonable choice would be to mark the table up with some other
        /// pseudo-hidden metadata, which seems even more undesirable.
        /// </summary>
        public static bool TableElementIsEditable(IHTMLElement element)
        {
            return TableElementContainsWriterEditingMark(element) &&
                !ContentSourceManager.IsSmartContent(element);
        }

        /// <summary>
        /// Only edit tables that are not contained within SmartContent blocks and which
        /// are marked with the "unselectable"  attribute. Since this is an attribute which
        /// applies only to editing scenarios it is almost certain never to enter the editor
        /// "from the wild" so it is a  reasonable way to determine whether we created the
        /// table (and thus can guarantee that it conforms to our editing capabilities). The
        /// only other reasonable choice would be to mark the table up with some other
        /// pseudo-hidden metadata, which seems even more undesirable.
        /// </summary>
        public static bool TableElementIsEditable(IHTMLElement element, MarkupRange elementMarkupRange)
        {
            return TableElementContainsWriterEditingMark(element) &&
                !TableElementIsContainedInSmartContent(elementMarkupRange);
        }

        public static bool TableElementContainsWriterEditingMark(IHTMLElement element)
        {
            return TableElementIsContainedInUnselectableTable(element);
        }

        public static void MakeTableWriterEditableIfRectangular(IHTMLTable table)
        {
            try
            {
                // no-op for null table
                if (table == null)
                    return;

                // no-op if we are already unselectable
                IHTMLElement tableElement = table as IHTMLElement;
                if (TableElementContainsWriterEditingMark(tableElement))
                    return;

                // check for rectangular structure
                int tableColumnCount = -1;
                foreach (IHTMLTableRow row in table.rows)
                {
                    int columnCount = row.cells.length;
                    if (tableColumnCount == -1)
                    {
                        // initilize table column count if this is the first pass
                        tableColumnCount = columnCount;
                    }
                    else
                    {
                        // compare this rows column count with the table column
                        // count -- if they are not equal then exit function
                        if (columnCount != tableColumnCount)
                            return;
                    }
                }

                // if we made it this far then the table is rectangular, so
                // mark it with our "editable" sentinel
                tableElement.setAttribute("unselectable", "on", 0);
            }
            catch (Exception ex)
            {
                Trace.Fail("Unexpected error in MakeTableWriterEditableIfRectangular: " + ex.ToString());
            }
        }

        private static bool TableElementIsContainedInUnselectableTable(IHTMLElement element)
        {
            IHTMLTable table = TableHelper.GetContainingTableElement(element);
            if (table != null)
            {
                object unselectable = (table as IHTMLElement).getAttribute("unselectable", 0);
                if (unselectable != null)
                    return unselectable.ToString() == "on";
                else
                    return false;
            }
            else
            {
                return false;
            }
        }

        private static bool TableElementIsContainedInSmartContent(MarkupRange elementRange)
        {
            // table elements inside smart content regions are not editable
            IHTMLElement parentSmartContent = elementRange.Start.GetParentElement(ContentSourceManager.CreateSmartContentElementFilter());
            return parentSmartContent != null;
        }

    }
}
