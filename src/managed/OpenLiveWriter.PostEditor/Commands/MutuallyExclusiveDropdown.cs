// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.Interop.Com.Ribbon;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.Commands
{
    /// <summary>
    /// Ensures that only one drop down entry is latched at a time.
    /// Updates drop down label based on the latched entry.
    /// </summary>
    public class MutuallyExclusiveDropdown<T> : MutuallyExclusiveCommandGroup where T : IComparable
    {
        protected Command DropDownCommand;
        public MutuallyExclusiveDropdown(Command dropDownCommand, Command[] commands, EventHandler executeHandler)
            : base(commands)
        {
            DropDownCommand = dropDownCommand;

            foreach (Command c in Commands)
            {
                Debug.Assert(c.Tag is IComparable);
                c.Execute += executeHandler;
            }

        }

        public virtual void SelectTag(T tag)
        {
            foreach (Command c in Commands)
            {
                IComparable comparable = c.Tag as IComparable;
                Debug.Assert(c.Tag is IComparable);

                bool selected = comparable != null && comparable.CompareTo(tag) == 0;
                c.Latched = selected;
                c.Invalidate(Keys);

                if (selected)
                    UpdateDropdown(c);
            }
        }

        protected override void OnExecute(Command executedCommand)
        {
            UpdateDropdown(executedCommand);
            base.OnExecute(executedCommand);
        }

        /// <summary>
        /// Synchronizes the drop down command LabelTitle with the selected command LabelTitle.
        /// Override if you require something more elaborate.
        /// </summary>
        /// <param name="selectedCommand"></param>
        public virtual void UpdateDropdown(Command selectedCommand)
        {
            DropDownCommand.LabelTitle = selectedCommand.LabelTitle;
        }

        protected virtual void SelectCommand(CommandId commandId)
        {
            foreach (Command c in Commands)
            {
                bool selected = c.CommandId == commandId;
                c.Latched = selected;
                c.Invalidate(Keys);

                if (selected)
                    UpdateDropdown(c);
            }
        }

        public bool Enabled
        {
            get
            {
                return DropDownCommand.Enabled;
            }

            set
            {
                foreach (Command c in Commands)
                {
                    c.Enabled = value;
                }
                DropDownCommand.Enabled = value;
            }
        }
    }

    /// <summary>
    /// Ensures that only a single command is latched among a set of commands.
    /// </summary>
    public class MutuallyExclusiveCommandGroup
    {
        protected Command[] Commands;
        public MutuallyExclusiveCommandGroup(Command[] commands)
        {
            Commands = commands;

            foreach (Command command in Commands)
            {
                command.Execute += new EventHandler(command_Execute);
            }
        }

        protected static PropertyKey[] Keys = new[] { PropertyKeys.BooleanValue };
        protected virtual void OnExecute(Command executedCommand)
        {
            foreach (Command c in Commands)
            {
                c.Latched = c.CommandId == executedCommand.CommandId;
                c.Invalidate(Keys);
            }
        }

        private void command_Execute(object sender, EventArgs e)
        {
            OnExecute((Command)sender);
        }
    }
}
