// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Collections;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.InternalWriterPlugin.Controls
{

    internal enum MapContextCommand
    {
        None,
        AddPushpin,
        ZoomStreetLevel,
        ZoomCityLevel,
        ZoomRegionLevel,
        CenterMap
    }

    internal enum PushpinContextCommand
    {
        None,
        EditPushpin,
        DeletePushpin
    }

    internal class MapContextMenu
    {
        internal static MapContextCommand ShowMapContextMenu(Control parent, Point location, MapContextCommand[] hideCommands, CommandManager commandManager)
        {
            Command returnCommand;
            using (CommandLoader commandLoader = new CommandLoader(commandManager, FilterMapContextMenuCommands(hideCommands)))
            {
                CommandContextMenuDefinition ccmd = new CommandContextMenuDefinition();
                ccmd.CommandBar = false;
                ccmd.Entries.Add(CommandId.MapAddPushpin, false, true);
                ccmd.Entries.Add(CommandId.MapZoomStreetLevel, false, false);
                ccmd.Entries.Add(CommandId.MapZoomCityLevel, false, false);
                ccmd.Entries.Add(CommandId.MapZoomRegionLevel, false, false);
                ccmd.Entries.Add(CommandId.MapCenterMap, false, false);

                returnCommand = CommandContextMenu.ShowModal(
                    commandManager, parent, location, ccmd);
            }

            if (returnCommand != null)
                return (MapContextCommand)_mapCommandIds[Enum.Parse(typeof(CommandId), returnCommand.Identifier)];
            else
                return MapContextCommand.None;
        }

        private static CommandId[] FilterMapContextMenuCommands(MapContextCommand[] excludeCommands)
        {
            Hashtable contextCommandTable = new Hashtable();
            foreach (MapContextCommand commandEnum in excludeCommands)
                contextCommandTable[commandEnum] = commandEnum;
            ArrayList commandIds = new ArrayList();
            foreach (CommandId id in MapContextMenuIds)
            {
                if (!contextCommandTable.ContainsKey(_mapCommandIds[id]))
                    commandIds.Add(id);
            }
            return (CommandId[])commandIds.ToArray(typeof(CommandId));
        }

        internal static PushpinContextCommand ShowPushpinContextMenu(Control parent, Point location, CommandManager commandManager)
        {
            Command command;
            using (CommandLoader commandLoader = new CommandLoader(commandManager, PushpinContextMenuIds))
            {
                CommandContextMenuDefinition ccmd = new CommandContextMenuDefinition();
                ccmd.CommandBar = false;
                ccmd.Entries.Add(CommandId.MapEditPushpin, false, false);
                ccmd.Entries.Add(CommandId.MapDeletePushpin, false, false);

                command = CommandContextMenu.ShowModal(
                    commandManager, parent, location, ccmd);
            }

            if (command != null)
                return (PushpinContextCommand)_pushpinCommandIds[Enum.Parse(typeof(CommandId), command.Identifier)];
            else
                return PushpinContextCommand.None;
        }

        private static CommandId[] MapContextMenuIds
        {
            get
            {
                return new ArrayList(_mapCommandIds.Keys).ToArray(typeof(CommandId)) as CommandId[];
            }
        }

        private static CommandId[] PushpinContextMenuIds
        {
            get
            {
                return new ArrayList(_pushpinCommandIds.Keys).ToArray(typeof(CommandId)) as CommandId[];
            }
        }

        static MapContextMenu()
        {
            _mapCommandIds.Add(CommandId.MapAddPushpin, MapContextCommand.AddPushpin);
            _mapCommandIds.Add(CommandId.MapZoomStreetLevel, MapContextCommand.ZoomStreetLevel);
            _mapCommandIds.Add(CommandId.MapZoomCityLevel, MapContextCommand.ZoomCityLevel);
            _mapCommandIds.Add(CommandId.MapZoomRegionLevel, MapContextCommand.ZoomRegionLevel);
            _mapCommandIds.Add(CommandId.MapCenterMap, MapContextCommand.CenterMap);

            _pushpinCommandIds.Add(CommandId.MapEditPushpin, PushpinContextCommand.EditPushpin);
            _pushpinCommandIds.Add(CommandId.MapDeletePushpin, PushpinContextCommand.DeletePushpin);
        }

        private static Hashtable _mapCommandIds = Hashtable.Synchronized(new Hashtable());
        private static Hashtable _pushpinCommandIds = Hashtable.Synchronized(new Hashtable());
    }

}
