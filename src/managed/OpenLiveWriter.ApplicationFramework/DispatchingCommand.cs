// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.ApplicationFramework
{
    public class DispatchingCommand : Command
    {
        protected readonly CommandManager CommandManager;

        public DispatchingCommand(CommandId commandId, CommandManager commandManager)
            : base(commandId)
        {
            CommandManager = commandManager;
        }

        protected Hashtable commands = new Hashtable();

        private void command_StateChanged(object sender, EventArgs e)
        {
            OnStateChanged(e);
        }

        /// <summary>
        /// Associates a commandid with the property key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="executeWithArgsDelegate"></param>
        public void AddCommand(PropertyKey key, ExecuteWithArgsDelegate executeWithArgsDelegate)
        {
            commands.Add(key, executeWithArgsDelegate);
        }

        public void Dispatch(PropertyKey key, ExecuteEventHandlerArgs args)
        {
            ExecuteWithArgsDelegate executeWithArgsDelegate = commands[key] as ExecuteWithArgsDelegate;

            if (executeWithArgsDelegate != null)
            {
                executeWithArgsDelegate(args);
                return;
            }

            CommandId commandId = (CommandId)commands[key];
            Command command = CommandManager.Get(commandId);
            command.PerformExecuteWithArgs(args);
        }
    }
}
