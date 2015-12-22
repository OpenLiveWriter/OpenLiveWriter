// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Interop.Com;
using OpenLiveWriter.Interop.Com.Ribbon;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.ApplicationFramework
{
    public class GenericCommandHandler : IUICommandHandler
    {
        private readonly CommandManager ParentCommandManager;

        public GenericCommandHandler(CommandManager commandManager)
        {
            ParentCommandManager = commandManager;
        }

        protected IUIImage GetPlaceholderImage()
        {
            Bitmap bitmap = Images.Missing_LargeImage;

            if (bitmap != null)
            {
                return RibbonHelper.CreateImage(bitmap.GetHbitmap(), ImageCreationOptions.Transfer);
            }
            return null;
        }

        #region IUICommandHandler Members

        public virtual int Execute(UInt32 commandId, CommandExecutionVerb verb, PropertyKeyRef key, PropVariantRef currentValue, IUISimplePropertySet commandExecutionProperties)
        {
            switch (verb)
            {
                case CommandExecutionVerb.Execute:
                    ParentCommandManager.Execute((CommandId)commandId);
                    return HRESULT.S_OK;
                case CommandExecutionVerb.Preview:
                    break;
                case CommandExecutionVerb.CancelPreview:
                    break;
            }
            return HRESULT.S_OK;
        }

        public virtual int UpdateProperty(uint commandId, ref PropertyKey key, PropVariantRef currentValue, out PropVariant newValue)
        {
            Command command = ParentCommandManager.Get((CommandId)commandId);
            if (command == null)
            {
                return NullCommandUpdateProperty(commandId, ref key, currentValue, out newValue);
            }

            try
            {
                newValue = new PropVariant();
                command.GetPropVariant(key, currentValue, ref newValue);

                if (newValue.IsNull())
                {
                    Trace.Fail("Didn't property update property for " + PropertyKeys.GetName(key) + " on command " + ((CommandId)commandId).ToString());

                    newValue = PropVariant.FromObject(currentValue.PropVariant.Value);
                }
            }
            catch (Exception ex)
            {
                Trace.Fail("Exception in UpdateProperty for " + PropertyKeys.GetName(key) + " on command " + commandId + ": " + ex);
                newValue = PropVariant.FromObject(currentValue.PropVariant.Value);
            }

            return HRESULT.S_OK;
        }

        #endregion

        public int NullCommandUpdateProperty(uint commandId, ref PropertyKey key, PropVariantRef currentValue, out PropVariant newValue)
        {

            try
            {
                newValue = new PropVariant();
                if (key == PropertyKeys.Enabled)
                {
                    newValue.SetBool(false);
                }
                else if (key == PropertyKeys.SmallImage)
                {
                    Bitmap bitmap = CommandResourceLoader.LoadCommandBitmap(((CommandId)commandId).ToString(), "SmallImage");
                    RibbonHelper.CreateImagePropVariant(bitmap, out newValue);
                }
                else if (key == PropertyKeys.SmallHighContrastImage)
                {
                    Bitmap bitmap =
                        CommandResourceLoader.LoadCommandBitmap(((CommandId)commandId).ToString(),
                                                                "SmallHighContrastImage") ??
                        CommandResourceLoader.LoadCommandBitmap(((CommandId)commandId).ToString(), "SmallImage");

                    RibbonHelper.CreateImagePropVariant(bitmap, out newValue);
                }
                else if (key == PropertyKeys.LargeImage)
                {
                    Bitmap bitmap = CommandResourceLoader.LoadCommandBitmap(((CommandId)commandId).ToString(), "LargeImage");
                    RibbonHelper.CreateImagePropVariant(bitmap, out newValue);
                }
                else if (key == PropertyKeys.LargeHighContrastImage)
                {
                    Bitmap bitmap =
                        CommandResourceLoader.LoadCommandBitmap(((CommandId)commandId).ToString(),
                                                                "LargeHighContrastImage") ??
                        CommandResourceLoader.LoadCommandBitmap(((CommandId)commandId).ToString(), "LargeImage");

                    RibbonHelper.CreateImagePropVariant(bitmap, out newValue);
                }
                else if (key == PropertyKeys.Label)
                {
                    string str = "Command." + ((CommandId)commandId).ToString() + ".LabelTitle";
                    newValue = new PropVariant(TextHelper.UnescapeNewlines(Res.GetProp(str)) ?? String.Empty);
                }
                else if (key == PropertyKeys.LabelDescription)
                {
                    string str = "Command." + ((CommandId)commandId).ToString() + ".LabelDescription";
                    newValue = new PropVariant(Res.GetProp(str) ?? String.Empty);
                }
                else if (key == PropertyKeys.TooltipTitle)
                {
                    string commandName = ((CommandId)commandId).ToString();
                    string str = "Command." + commandName + ".TooltipTitle";
                    newValue = new PropVariant(Res.GetProp(str) ?? (Res.GetProp("Command." + commandName + ".LabelTitle") ?? String.Empty));
                }
                else if (key == PropertyKeys.TooltipDescription)
                {
                    string str = "Command." + ((CommandId)commandId).ToString() + ".TooltipDescription";
                    newValue = new PropVariant(Res.GetProp(str) ?? String.Empty);
                }
                else if (key == PropertyKeys.Keytip)
                {
                    newValue = new PropVariant("XXX");
                }
                else if (key == PropertyKeys.ContextAvailable)
                {
                    newValue.SetUInt((uint)ContextAvailability.NotAvailable);
                }
                else if (key == PropertyKeys.Categories)
                {
                    newValue = new PropVariant();
                    newValue.SetIUnknown(currentValue);
                }
                else if (key == PropertyKeys.RecentItems)
                {
                    object[] currColl = (object[])currentValue.PropVariant.Value;
                    newValue = new PropVariant();
                    newValue.SetSafeArray(currColl);
                    return HRESULT.S_OK;

                }
                else if (key == PropertyKeys.ItemsSource)
                {
                    // This should only be necessary if you have created a gallery in the ribbon markup that you have not yet put into the command manager.
                    List<IUISimplePropertySet> list = new List<IUISimplePropertySet>();

                    OpenLiveWriter.Interop.Com.Ribbon.IEnumUnknown enumUnk = new BasicCollection(list);
                    newValue = new PropVariant();
                    newValue.SetIUnknown(enumUnk);
                    return HRESULT.S_OK;
                }
                else if (key == PropertyKeys.StringValue)
                {
                    newValue = new PropVariant(String.Empty);
                }
                else if (key == PropertyKeys.SelectedItem)
                {
                    newValue = new PropVariant(0);
                }
                else if (key == PropertyKeys.DecimalValue)
                {
                    newValue.SetDecimal(new decimal(0));
                }
                else if (key == PropertyKeys.MinValue)
                {
                    newValue.SetDecimal(new decimal(0));
                }
                else if (key == PropertyKeys.MaxValue)
                {
                    newValue.SetDecimal(new decimal(100));
                }
                else if (key == PropertyKeys.Increment)
                {
                    newValue.SetDecimal(new decimal(1));
                }
                else if (key == PropertyKeys.DecimalPlaces)
                {
                    newValue.SetDecimal(new decimal(0));
                }
                else if (key == PropertyKeys.RepresentativeString)
                {
                    newValue.SetString("9999");
                }
                else if (key == PropertyKeys.FormatString)
                {
                    newValue.SetString(String.Empty);
                }
                else if (key == PropertyKeys.StandardColors)
                {
                    newValue = new PropVariant();
                    newValue.SetUIntVector(new uint[] { });
                }
                else if (key == PropertyKeys.StandardColorsTooltips)
                {
                    newValue = new PropVariant();
                    newValue.SetStringVector(new string[] { });
                }
                else if (key == PropertyKeys.BooleanValue)
                {
                    newValue = new PropVariant();
                    newValue.SetBool(false);
                }
                else
                {
                    Trace.Fail("Didn't properly update property for " + PropertyKeys.GetName(key) + " on command " + ((CommandId)commandId));
                    newValue = new PropVariant();
                }
            }
            catch (Exception ex)
            {
                Trace.Fail("Exception in UpdateProperty for " + PropertyKeys.GetName(key) + " on command " + commandId + ": " + ex);
                newValue = new PropVariant();
            }

            return HRESULT.S_OK;
        }
    }

}
