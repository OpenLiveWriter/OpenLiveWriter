// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using OpenLiveWriter.HtmlEditor;
using OpenLiveWriter.CoreServices;
using mshtml;

namespace OpenLiveWriter.PostEditor.Tables
{

    internal class TableCellSelectionTracker
    {
        public TableCellSelectionTracker(TableEditingElementBehavior tableElementBehavior, TableEditingContext tableEditingContext)
        {
            _tableElementBehavior = tableElementBehavior;
            _tableEditingContext = tableEditingContext;
        }

        public void EditorSelectionChanged(TableSelection tableSelection)
        {
            // list of currently selected cells (default to none)
            ArrayList selectedCells = new ArrayList();
            if (tableSelection != null && tableSelection.HasContiguousSelection)
                selectedCells = tableSelection.SelectedCells;

            // build hashtables of element behaviors for our selected and unselected cells
            Hashtable selectedCellBehaviors = new Hashtable();
            foreach (IHTMLElement cellElement in selectedCells)
            {
                TableCellEditingElementBehavior cellBehavior = _tableEditingContext.GetCellBehavior(cellElement);
                if (cellBehavior != null)
                    selectedCellBehaviors[cellElement.sourceIndex] = cellBehavior;
            }
            Hashtable lastSelectedCellBehaviors = new Hashtable();
            foreach (IHTMLElement cellElement in _lastSelectedCells)
            {
                TableCellEditingElementBehavior cellBehavior = _tableEditingContext.GetCellBehavior(cellElement);
                if (cellBehavior != null)
                    lastSelectedCellBehaviors[cellElement.sourceIndex] = cellBehavior;
            }

            // unselect cells that should no longer be selected
            foreach (DictionaryEntry entry in lastSelectedCellBehaviors)
            {
                if (!selectedCellBehaviors.ContainsKey(entry.Key))
                    (entry.Value as TableCellEditingElementBehavior).DrawSelectionBorder = false;
            }

            // select cells that are joining the selection
            foreach (DictionaryEntry entry in selectedCellBehaviors)
            {
                if (!lastSelectedCellBehaviors.ContainsKey(entry.Key))
                    (entry.Value as TableCellEditingElementBehavior).DrawSelectionBorder = true;
            }

            // update the last selected cells
            _lastSelectedCells = selectedCells;
        }

        private TableEditingElementBehavior _tableElementBehavior;
        private TableEditingContext _tableEditingContext;

        private ArrayList _lastSelectedCells = new ArrayList();

    }
}
