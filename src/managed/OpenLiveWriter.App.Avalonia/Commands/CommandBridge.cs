// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.App.Avalonia.Commands
{
    /// <summary>
    /// Bridges ribbon CommandId events to editor actions.
    /// </summary>
    public class CommandBridge
    {
        private readonly Dictionary<CommandId, Action> _handlers = new();

        public void RegisterHandler(CommandId commandId, Action handler)
        {
            _handlers[commandId] = handler;
        }

        public bool Execute(CommandId commandId)
        {
            if (_handlers.TryGetValue(commandId, out var handler))
            {
                handler();
                return true;
            }
            return false;
        }

        public bool HasHandler(CommandId commandId) => _handlers.ContainsKey(commandId);
    }
}
