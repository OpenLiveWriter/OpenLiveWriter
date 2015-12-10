// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

namespace Project31.ApplicationFramework
{
    /// <summary>
    /// Application control.
    /// </summary>
    public class ApplicationControl : System.Windows.Forms.UserControl, ICommandManager, ISelectionManager
    {
        /// <summary>
        /// The set of active commands, keyed by command identifier.
        /// </summary>
        private Hashtable commandTable = new Hashtable();

        /// <summary>
        /// The set of selected objects.
        /// </summary>
        private ArrayList selectionList = new ArrayList();

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        /// <summary>
        /// Occurs when the selection changes.
        /// </summary>
        public event EventHandler SelectionChanged;

        /// <summary>
        /// Initializes a new instance of the ApplicationControl class.
        /// </summary>
        public ApplicationControl()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose( bool disposing )
        {
            if( disposing )
            {
                if(components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose( disposing );
        }

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
        }
        #endregion

        ///	<interface>ICommandManager</interface>
        /// <summary>
        /// Activates the specified command list.
        /// </summary>
        /// <param name="commandList">The CommandList to activate.</param>
        public void ActivateCommandList(CommandList commandList)
        {
            //	Add all the commands from this command provider to the command table.
            foreach (Command command in commandList.Commands)
                commandTable.Add(command.Identifier, command);
        }

        ///	<interface>ICommandManager</interface>
        /// <summary>
        /// Deactivates the specified command list.
        /// </summary>
        /// <param name="commandList">The CommandList to deactivate.</param>
        public void DeactivateCommandList(CommandList commandList)
        {
            //	Remove all the commands from this command provider from the command table.
            foreach (Command command in commandList.Commands)
                commandTable.Remove(command.Identifier);
        }

        ///	<interface>ISelectionManager</interface>
        /// <summary>
        /// Clears the current selection.
        /// </summary>
        public void ClearSelection()
        {
            //	Clear the selection list.
            selectionList.Clear();

            //	Raise the SelectionChanged event.
            OnSelectionChanged(EventArgs.Empty);
        }

        /// <summary>
        /// Raises the SelectionChanged event.
        /// </summary>
        /// <param name="e">And EventArgs that contains the event data.</param>
        protected virtual void OnSelectionChanged(EventArgs e)
        {
            if (SelectionChanged != null)
                SelectionChanged(this, e);
        }

        ///	<interface>ISelectionManager</interface>
        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="selectableObject">The ISelectableObject value to select.</param>
        public void SetSelection(ISelectableObject selectableObject)
        {
        }

        ///	<interface>ISelectionManager</interface>
        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="selectableObject">The array of ISelectableObject values to select.</param>
        public void SetSelection(ISelectableObject[] selectableObjects)
        {
        }
    }
}
