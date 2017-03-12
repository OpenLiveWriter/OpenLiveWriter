// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.HtmlEditor;
using mshtml;

namespace OpenLiveWriter.PostEditor.Tables
{

    internal class TableEditingContext
    {
        public TableEditingContext(IHtmlEditorComponentContext editorContext)
        {
            // list of cell element behaviors (demand create if necessary)
            if (!editorContext.Cookies.Contains(CELL_ELEMENT_BEHAVIORS))
                editorContext.Cookies[CELL_ELEMENT_BEHAVIORS] = new ArrayList();
            _cellElementBehaviors = editorContext.Cookies[CELL_ELEMENT_BEHAVIORS] as ArrayList;
        }

        public void AddCellBehavior(TableCellEditingElementBehavior cellBehavior)
        {
            _cellElementBehaviors.Add(cellBehavior);
        }

        public void RemoveCellBehavior(TableCellEditingElementBehavior cellBehavior)
        {
            _cellElementBehaviors.Remove(cellBehavior);
        }

        public TableCellEditingElementBehavior GetCellBehavior(IHTMLElement cellElement)
        {
            foreach (TableCellEditingElementBehavior cellBehavior in _cellElementBehaviors)
            {
                if (cellBehavior.Attached && HTMLElementHelper.ElementsAreEqual(cellElement, cellBehavior.HTMLElement))
                    return cellBehavior;
            }

            // didn't find the behavior
            return null;
        }

        private const string CELL_ELEMENT_BEHAVIORS = "CellElementBehaviors";
        private ArrayList _cellElementBehaviors;

    }
}
