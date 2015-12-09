// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.Tables
{
    /// <summary>
    /// Summary description for TextContextMenuDefinition.
    /// </summary>
    public class TableContextMenuDefinition : CommandContextMenuDefinition
    {
        public TableContextMenuDefinition() : this(true)
        {
        }

        public TableContextMenuDefinition(bool includeInsertCommand)
        {
            if (includeInsertCommand)
                Entries.Add(CommandId.InsertTable, false, true);
            Entries.Add(CommandId.TableProperties, false, false);
            Entries.Add(CommandId.RowProperties, false, false);
            Entries.Add(CommandId.ColumnProperties, false, false);
            Entries.Add(CommandId.CellProperties, false, false);
            Entries.Add(CommandId.InsertRowAbove, true, false);
            Entries.Add(CommandId.InsertRowBelow, false, false);
            Entries.Add(CommandId.MoveRowUp, false, false);
            Entries.Add(CommandId.MoveRowDown, false, false);
            Entries.Add(CommandId.InsertColumnLeft, true, false);
            Entries.Add(CommandId.InsertColumnRight, false, false);
            Entries.Add(CommandId.MoveColumnLeft, false, false);
            Entries.Add(CommandId.MoveColumnRight, false, false);
            Entries.Add(CommandId.DeleteTable, true, false);
            Entries.Add(CommandId.DeleteRow, false, false);
            Entries.Add(CommandId.DeleteColumn, false, false);
            Entries.Add(CommandId.ClearCell, false, false);
        }
    }
}
