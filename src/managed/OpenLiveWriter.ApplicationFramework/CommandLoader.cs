// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.ApplicationFramework
{
    public class CommandLoader : IDisposable
    {
        private readonly CommandManager _commandManager;
        private readonly ArrayList _loadedCommands = new ArrayList();

        public CommandLoader(CommandManager commandManager, params CommandId[] commandIds)
        {
            if (commandManager == null)
                throw new ArgumentNullException("commandManager");

            this._commandManager = commandManager;
            try
            {
                _commandManager.BeginUpdate();
                foreach (CommandId commandId in commandIds)
                {
                    Command command = new Command(commandId);
                    commandManager.Add(command);
                    _loadedCommands.Add(command);
                }
            }
            catch (Exception)
            {
                Dispose();
                throw;
            }
            finally
            {
                _commandManager.EndUpdate();
            }
        }

        public Command[] Commands
        {
            get { return (Command[])_loadedCommands.ToArray(typeof(Command)); }
        }

        public void Dispose()
        {
            try
            {
                _commandManager.BeginUpdate();
                foreach (Command command in _loadedCommands)
                    _commandManager.Remove(command);
                _loadedCommands.Clear();
            }
            finally
            {
                _commandManager.EndUpdate();
            }
        }
    }
}
