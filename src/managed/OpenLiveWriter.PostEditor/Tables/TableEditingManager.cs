// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using System.Text;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.HtmlEditor;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Mshtml;
using OpenLiveWriter.Interop.Windows;
using OpenLiveWriter.ApplicationFramework;
using mshtml;

namespace OpenLiveWriter.PostEditor.Tables
{
    internal class TableEditingManager : IDisposable
    {
        private bool _selectionChangedHooked = false;
        public TableEditingManager(IHtmlEditorComponentContext editorContext)
        {
            _editorContext = editorContext;

            if (!GlobalEditorOptions.SupportsFeature(ContentEditorFeature.Table))
            {
                _editorContext.SelectionChanged += new EventHandler(_editorContext_SelectionChanged);
                _selectionChangedHooked = true;
            }

            InitializeCommands();
        }

        public MshtmlElementBehavior CreateTableEditingElementBehavior()
        {
            return new TableEditingElementBehavior(_editorContext, this);
        }

        public MshtmlElementBehavior CreateTableCellEditingElementBehavior()
        {
            return new TableCellEditingElementBehavior(_editorContext);
        }

        public bool ShowTableContextMenuForElement(IHTMLElement element)
        {
            IHTMLTable tableElement = TableHelper.GetContainingTableElement(element);

            if (tableElement != null)
            {
                MarkupRange tableMarkupRange = _editorContext.MarkupServices.CreateMarkupRange(tableElement as IHTMLElement);
                return TableHelper.TableElementIsEditable(tableElement as IHTMLElement, tableMarkupRange);
            }
            else
            {
                return false;
            }
        }

        public CommandContextMenuDefinition CreateTableContextMenuDefinition()
        {
            // make sure enable/disable states are correct based on new selection
            ManageCommands();

            // return menu definition
            return new TableContextMenuDefinition(false);
        }

        internal void NotifyTableDetached()
        {
            ManageCommands();
        }

        private void _editorContext_SelectionChanged(object sender, EventArgs e)
        {
            ManageCommands();
        }

        private void InitializeCommands()
        {
            _editorContext.CommandManager.BeginUpdate();

            commandTableMenu = new Command(CommandId.TableMenu);
            commandTableMenu.CommandBarButtonContextMenuDefinition = new TableContextMenuDefinition();
            _editorContext.CommandManager.Add(commandTableMenu);

            commandTableProperties = new Command(CommandId.TableProperties);
            commandTableProperties.Execute += new EventHandler(commandTableProperties_Execute);
            _editorContext.CommandManager.Add(commandTableProperties);

            commandDeleteTable = new Command(CommandId.DeleteTable);
            commandDeleteTable.Execute += new EventHandler(commandDeleteTable_Execute);
            _editorContext.CommandManager.Add(commandDeleteTable);

            commandRowProperties = new Command(CommandId.RowProperties);
            commandRowProperties.Execute += new EventHandler(commandRowProperties_Execute);
            _editorContext.CommandManager.Add(commandRowProperties);

            commandInsertRowAbove = new Command(CommandId.InsertRowAbove);
            commandInsertRowAbove.Execute += new EventHandler(commandInsertRowAbove_Execute);
            _editorContext.CommandManager.Add(commandInsertRowAbove);

            commandInsertRowBelow = new Command(CommandId.InsertRowBelow);
            commandInsertRowBelow.Execute += new EventHandler(commandInsertRowBelow_Execute);
            _editorContext.CommandManager.Add(commandInsertRowBelow);

            commandMoveRowUp = new Command(CommandId.MoveRowUp);
            commandMoveRowUp.Execute += new EventHandler(commandMoveRowUp_Execute);
            _editorContext.CommandManager.Add(commandMoveRowUp);

            commandMoveRowDown = new Command(CommandId.MoveRowDown);
            commandMoveRowDown.Execute += new EventHandler(commandMoveRowDown_Execute);
            _editorContext.CommandManager.Add(commandMoveRowDown);

            commandDeleteRow = new Command(CommandId.DeleteRow);
            commandDeleteRow.Execute += new EventHandler(commandDeleteRow_Execute);
            _editorContext.CommandManager.Add(commandDeleteRow);

            commandColumnProperties = new Command(CommandId.ColumnProperties);
            commandColumnProperties.Execute += new EventHandler(commandColumnProperties_Execute);
            _editorContext.CommandManager.Add(commandColumnProperties);

            commandInsertColumnLeft = new Command(CommandId.InsertColumnLeft);
            commandInsertColumnLeft.Execute += new EventHandler(commandInsertColumnLeft_Execute);
            _editorContext.CommandManager.Add(commandInsertColumnLeft);

            commandInsertColumnRight = new Command(CommandId.InsertColumnRight);
            commandInsertColumnRight.Execute += new EventHandler(commandInsertColumnRight_Execute);
            _editorContext.CommandManager.Add(commandInsertColumnRight);

            commandMoveColumnLeft = new Command(CommandId.MoveColumnLeft);
            commandMoveColumnLeft.Execute += new EventHandler(commandMoveColumnLeft_Execute);
            _editorContext.CommandManager.Add(commandMoveColumnLeft);

            commandMoveColumnRight = new Command(CommandId.MoveColumnRight);
            commandMoveColumnRight.Execute += new EventHandler(commandMoveColumnRight_Execute);
            _editorContext.CommandManager.Add(commandMoveColumnRight);

            commandDeleteColumn = new Command(CommandId.DeleteColumn);
            commandDeleteColumn.Execute += new EventHandler(commandDeleteColumn_Execute);
            _editorContext.CommandManager.Add(commandDeleteColumn);

            commandCellProperties = new Command(CommandId.CellProperties);
            commandCellProperties.Execute += new EventHandler(commandCellProperties_Execute);
            _editorContext.CommandManager.Add(commandCellProperties);

            commandClearCell = new Command(CommandId.ClearCell);
            commandClearCell.Execute += new EventHandler(commandClearCell_Execute);
            _editorContext.CommandManager.Add(commandClearCell);

            _editorContext.CommandManager.EndUpdate();
        }

        internal void ManageCommands()
        {
            // state variables
            bool editableTableSelected, multipleRowsSelected, multipleColumnsSelected, multipleCellsSelected;

            if (_editorContext.EditMode)
            {
                // analyze selection
                TableSelection tableSelection = new TableSelection(_editorContext.Selection.SelectedMarkupRange);
                editableTableSelected = (tableSelection.Table != null) && (tableSelection.Table as IHTMLElement3).isContentEditable;
                multipleRowsSelected = editableTableSelected && !tableSelection.SingleRowSelected;
                multipleColumnsSelected = editableTableSelected && !tableSelection.SingleColumnSelected;
                multipleCellsSelected = editableTableSelected && tableSelection.HasContiguousSelection;
            }
            else
            {
                editableTableSelected = multipleRowsSelected = multipleColumnsSelected = multipleCellsSelected = false;
            }

            commandTableProperties.Enabled = editableTableSelected;
            commandDeleteTable.Enabled = editableTableSelected;

            commandRowProperties.Enabled = editableTableSelected && !multipleRowsSelected;
            commandInsertRowAbove.Enabled = editableTableSelected;
            commandInsertRowBelow.Enabled = editableTableSelected;
            commandMoveRowUp.Enabled = editableTableSelected && !multipleRowsSelected;
            commandMoveRowDown.Enabled = editableTableSelected && !multipleRowsSelected;
            commandDeleteRow.Enabled = editableTableSelected;
            //			commandDeleteRow.MenuFormatArgs = new object[] { multipleRowsSelected ? "s" : String.Empty };

            commandColumnProperties.Enabled = editableTableSelected && !multipleColumnsSelected;
            commandInsertColumnLeft.Enabled = editableTableSelected;
            commandInsertColumnRight.Enabled = editableTableSelected;
            commandMoveColumnLeft.Enabled = editableTableSelected && !multipleColumnsSelected;
            commandMoveColumnRight.Enabled = editableTableSelected && !multipleColumnsSelected;
            commandDeleteColumn.Enabled = editableTableSelected;
            //			commandDeleteColumn.MenuFormatArgs = new object[] { multipleColumnsSelected ? "s" : String.Empty };

            commandCellProperties.Enabled = editableTableSelected && !multipleCellsSelected;
            commandClearCell.Enabled = editableTableSelected;
            //			commandClearCell.MenuFormatArgs = new object[] { multipleCellsSelected ? "s" : String.Empty } ;
        }

        private void commandTableProperties_Execute(object sender, EventArgs e)
        {
            using (TablePropertiesForm tablePropertiesForm = new TablePropertiesForm())
            {
                // read existing properites
                TableProperties existingProperties = TableEditor.GetTableProperties(_editorContext);

                // show the dialog
                TableProperties editedProperties = tablePropertiesForm.EditTable(_editorContext.MainFrameWindow, existingProperties);

                // update
                if (editedProperties != null)
                {
                    TableEditor.SetTableProperties(_editorContext, editedProperties);
                }
            }
        }

        private void commandDeleteTable_Execute(object sender, EventArgs e)
        {
            if (DisplayMessage.Show(MessageId.ConfirmDeleteTable, _editorContext.MainFrameWindow) == DialogResult.Yes)
            {
                TableEditor.DeleteTable(_editorContext);
            }
        }

        private void commandRowProperties_Execute(object sender, EventArgs e)
        {
            using (RowPropertiesForm rowPropertiesForm = new RowPropertiesForm(TableEditor.GetRowProperties(_editorContext)))
            {
                if (rowPropertiesForm.ShowDialog(_editorContext.MainFrameWindow) == DialogResult.OK)
                {
                    TableEditor.SetRowProperties(_editorContext, rowPropertiesForm.RowProperties);
                }
            }
        }

        private void commandInsertRowAbove_Execute(object sender, EventArgs e)
        {
            TableEditor.InsertRowAbove(_editorContext);
        }

        private void commandInsertRowBelow_Execute(object sender, EventArgs e)
        {
            TableEditor.InsertRowBelow(_editorContext);
        }

        private void commandMoveRowUp_Execute(object sender, EventArgs e)
        {
            TableEditor.MoveRowUp(_editorContext);
        }

        private void commandMoveRowDown_Execute(object sender, EventArgs e)
        {
            TableEditor.MoveRowDown(_editorContext);
        }

        private void commandDeleteRow_Execute(object sender, EventArgs e)
        {
            TableEditor.DeleteRows(_editorContext);
            ManageCommands();
        }

        private void commandColumnProperties_Execute(object sender, EventArgs e)
        {
            using (ColumnPropertiesForm columnPropertiesForm = new ColumnPropertiesForm(TableEditor.GetColumnProperties(_editorContext)))
            {
                if (columnPropertiesForm.ShowDialog(_editorContext.MainFrameWindow) == DialogResult.OK)
                {
                    TableEditor.SetColumnProperties(_editorContext, columnPropertiesForm.ColumnProperties);
                }
            }
        }

        private void commandInsertColumnLeft_Execute(object sender, EventArgs e)
        {
            // In RTL, since the table is flipped, we have to flipp the commands that are right and left aware.
            if (_editorContext.IsRTLTemplate)
                TableEditor.InsertColumnRight(_editorContext);
            else
                TableEditor.InsertColumnLeft(_editorContext);
        }

        private void commandInsertColumnRight_Execute(object sender, EventArgs e)
        {
            // In RTL, since the table is flipped, we have to flipp the commands that are right and left aware.
            if (_editorContext.IsRTLTemplate)
                TableEditor.InsertColumnLeft(_editorContext);
            else
                TableEditor.InsertColumnRight(_editorContext);
        }

        private void commandMoveColumnLeft_Execute(object sender, EventArgs e)
        {
            // In RTL, since the table is flipped, we have to flipp the commands that are right and left aware.
            if (_editorContext.IsRTLTemplate)
                TableEditor.MoveColumnRight(_editorContext);
            else
                TableEditor.MoveColumnLeft(_editorContext);
        }

        private void commandMoveColumnRight_Execute(object sender, EventArgs e)
        {
            // In RTL, since the table is flipped, we have to flipp the commands that are right and left aware.
            if (_editorContext.IsRTLTemplate)
                TableEditor.MoveColumnLeft(_editorContext);
            else
                TableEditor.MoveColumnRight(_editorContext);
        }

        private void commandDeleteColumn_Execute(object sender, EventArgs e)
        {
            TableEditor.DeleteColumns(_editorContext);
            ManageCommands();
        }

        private void commandCellProperties_Execute(object sender, EventArgs e)
        {
            using (CellPropertiesForm cellPropertiesForm = new CellPropertiesForm(TableEditor.GetCellProperties(_editorContext)))
            {
                if (cellPropertiesForm.ShowDialog(_editorContext.MainFrameWindow) == DialogResult.OK)
                {
                    TableEditor.SetCellProperties(_editorContext, cellPropertiesForm.CellProperties);
                }
            }
        }

        private void commandClearCell_Execute(object sender, EventArgs e)
        {
            TableSelection tableSelection = new TableSelection(_editorContext.Selection.SelectedMarkupRange);
            if (tableSelection.Table != null)
            {
                MarkupRange tableMarkupRange = _editorContext.MarkupServices.CreateMarkupRange(tableSelection.Table as IHTMLElement);
                using (_editorContext.DamageServices.CreateDamageTracker(tableMarkupRange, false))
                    TableEditor.ClearCells(_editorContext);
            }
        }

        private Command commandTableMenu;
        private Command commandTableProperties;
        private Command commandDeleteTable;

        private Command commandRowProperties;
        private Command commandInsertRowAbove;
        private Command commandInsertRowBelow;
        private Command commandMoveRowUp;
        private Command commandMoveRowDown;
        private Command commandDeleteRow;

        private Command commandColumnProperties;
        private Command commandInsertColumnLeft;
        private Command commandInsertColumnRight;
        private Command commandMoveColumnLeft;
        private Command commandMoveColumnRight;
        private Command commandDeleteColumn;

        private Command commandCellProperties;
        private Command commandClearCell;

        private IHtmlEditorComponentContext _editorContext;
        public void Dispose()
        {
            if (_selectionChangedHooked)
            {
                _editorContext.SelectionChanged -= new EventHandler(_editorContext_SelectionChanged);
                _selectionChangedHooked = false;
            }
        }
    }


}
