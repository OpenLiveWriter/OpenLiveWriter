// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.Interop.Com.Ribbon;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.Commands
{
    /// <summary>
    /// This is a specific type of GalleryCommand that is equivalent to 'command' type galleries in ribbon
    /// as opposed to 'item' type gallery that GalleryCommand implements
    /// Each item in this gallery command has a corresponding Command with a valid .CommandId
    /// Note: This is a simplistic implementation of 'command gallery' with-in the current CommandManager
    /// architecture. A true command gallery in ribbon supports dynamic commands, but to support such a system
    /// CommandManager needs to be updated to support the concept of dynamic commandId, a potential MQ work item.
    /// </summary>
    public class DynamicCommandGallery : GalleryCommand<Command>
    {
        public DynamicCommandGallery(CommandId commandId)
            : base(commandId)
        {
        }

        public override int PerformExecute(CommandExecutionVerb verb, PropertyKeyRef key, PropVariantRef currentValue, IUISimplePropertySet commandExecutionProperties)
        {
            // This is the main command
            if (verb == CommandExecutionVerb.Execute)
            {
                OnExecute(EventArgs.Empty);
            }
            return HRESULT.S_OK;
        }

        private bool LoadCommandSimplePropertySet(GalleryItem item, out SimplePropertySet sps)
        {
            Debug.Assert(item.Cookie != null && item.Cookie.CommandId != CommandId.None,
                "Command gallery item without a valid Command or Command.CommandId!");
            sps = null;
            if (item.Cookie != null && item.Cookie.CommandId != CommandId.None)
            {
                sps = new SimplePropertySet();

                if (item.Label != null)
                {
                    sps.Add(PropertyKeys.Label, new PropVariant(item.Label));
                }

                if (item.Image != null)
                {
                    PropVariant image = new PropVariant(item.IUIImage);

                    if (image.VarType == VarEnum.VT_UNKNOWN)
                        sps.Add(PropertyKeys.ItemImage, image);
                }

                // Command type items require CommandId, CommandType and CategoryId
                UInt32 cmdId = (uint)(item.Cookie.CommandId);
                sps.Add(PropertyKeys.CommandId, new PropVariant(cmdId));
                sps.Add(PropertyKeys.CommandType, new PropVariant((uint)CommandTypeID.UI_COMMANDTYPE_ACTION));
                sps.Add(PropertyKeys.CategoryId, new PropVariant(item.CategoryIndex));
                return true;
            }
            return false;
        }

        private void LoadItemsSourceAsCommands(ref PropVariant newValue)
        {
            List<IUISimplePropertySet> list = new List<IUISimplePropertySet>();
            foreach (GalleryItem item in Items)
            {
                SimplePropertySet sps;
                if (LoadCommandSimplePropertySet(item, out sps))
                {
                    list.Add(sps);
                }
            }
            newValue.SetIUnknown(new BasicCollection(list));
        }

        public override void GetPropVariant(PropertyKey key, PropVariantRef currentValue, ref PropVariant value)
        {
            if (key == PropertyKeys.ItemsSource)
            {
                // We are a command gallery (not an item gallery), so load our items as commands
                LoadItemsSourceAsCommands(ref value);
            }
            else
            {
                base.GetPropVariant(key, currentValue, ref value);
            }
        }
    }
}
