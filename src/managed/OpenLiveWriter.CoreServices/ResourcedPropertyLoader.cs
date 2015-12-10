// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.CoreServices
{
    public class ResourcedPropertyLoader
    {
        private readonly Type _type;
        private readonly ArrayList _localizedProperties = new ArrayList();
        private readonly ArrayList _invariantProperties = new ArrayList();
        private ResourceManager _localizedResourceManager;
        private ResourceManager _invariantResourceManager;

        public ResourcedPropertyLoader(Type type, ResourceManager localizedResourceManager, ResourceManager invariantResourceManager)
        {
            _type = type;
            _localizedResourceManager = localizedResourceManager;
            _invariantResourceManager = invariantResourceManager;

            foreach (PropertyInfo prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty))
            {
                object[] attributes = prop.GetCustomAttributes(typeof(LocalizableAttribute), false);

                if (prop.PropertyType.IsEnum || prop.PropertyType.IsPrimitive || prop.PropertyType == typeof(string))
                {
                    if (attributes.Length == 0 || !((LocalizableAttribute)attributes[0]).IsLocalizable)
                        _invariantProperties.Add(prop);
                    else
                        _localizedProperties.Add(prop);
                }
            }
        }

        public ResourceManager LocalizedResources
        {
            get { return _localizedResourceManager; }
        }
        public ResourceManager NonLocalizedResources
        {
            get { return _invariantResourceManager; }
        }

        public void ApplyResources(object obj, string id)
        {
            if (obj == null)
            {
                Debug.Fail("Can't apply resources to a null value");
                throw new ArgumentNullException("obj", "Can't apply resources to a null value");
            }
            if (!_type.IsInstanceOfType(obj))
            {
                Debug.Fail("Can't apply resources to an object of type " + obj.GetType().Name);
                throw new ArgumentException("obj", "Can't apply resources to an object of type " + obj.GetType().Name);
            }

            Apply(_localizedResourceManager, _localizedProperties, obj, id, true);
            Apply(_invariantResourceManager, _invariantProperties, obj, id, false);
        }
        List<string> props = new List<string>();
        private void Apply(ResourceManager rm, ArrayList properties, object obj, string id, bool localized)
        {
            foreach (PropertyInfo prop in properties)
            {
                // rm.GetObject is the most costly operation in this function.  Protect it against
                // by only letting properties through that we know might have a string associated with them
                switch (prop.Name)
                {
                    case "Text":
                    case "MenuText":
                    case "Shortcut":
                    case "TooltipTitle":
                    case "TooltipDescription":
                    case "Keytip":
                    case "LabelTitle":
                    case "LabelDescription":
                    case "SuppressMenuBitmap":
                    case "VisibleOnContextMenu":
                    case "Buttons":
                    case "Type":
                    case "AdvancedShortcut":
                    case "Title":
                    case "CommandBarButtonText":
                    case "SuppressCommandBarBitmap":
                    case "Description":
                    case "VisibleOnCommandBar":
                    case "AccessibleDescription":
                    case "DefaultButton":
                        break;
                    case "CommandId":
                    case "AccessibleName":
                    case "AcceleratorMnemonic":
                    case "ExecutionArgs":
                    case "Identifier":
                    case "VisibleOnMainMenu":
                    case "ShowShortcut":
                    case "MainMenuPath":
                    case "CommandBarButtonStyle":
                    case "CommandBarButtonContextMenuAcceleratorMnemonic":
                    case "CommandBarButtonContextMenuDropDown":
                    case "CommandBarButtonContextMenuInvalidateParent":
                    case "InvalidationCount":
                    case "On":
                    case "Enabled":
                    case "Latched":
#if DEBUG
                        string strTest = (string)rm.GetObject(_type.Name + "." + id + "." + prop.Name);
                        if (strTest != null)
                        {
                            Debug.Fail("Skipping property even though it has a value: " + prop.Name);
                        }
#endif
                        continue;
                    default:
                        Debug.Fail("Unknown property: " + prop.Name);
                        break;

                }

                string str = (string)rm.GetObject(_type.Name + "." + id + "." + prop.Name);
                if (str != null)
                {
                    try
                    {
                        object o;
                        if (prop.PropertyType == typeof(string))
                        {
                            o = str;
                            if (Res.DebugMode && localized)
                                o = id + "." + prop.Name;
                        }
                        else if (prop.PropertyType.IsPrimitive)
                            o = Convert.ChangeType(str, prop.PropertyType, CultureInfo.InvariantCulture);
                        else if (prop.PropertyType.IsEnum)
                        {
                            try
                            {
                                o = Enum.Parse(prop.PropertyType, str, true);
                            }
                            catch (Exception e)
                            {
                                Trace.Fail("Failed to parse enum value for property " + prop.Name + ":\r\n" + e.ToString());
                                continue;
                            }
                        }
                        else
                            throw new ArgumentException("Unexpected type " + prop.PropertyType.FullName);

                        prop.SetValue(obj, o, null);
                    }
                    catch (Exception e)
                    {
                        Trace.Fail("Error setting property " + _type.Name + ":\r\n" + e.ToString());
                        throw;
                    }
                }
            }
        }

    }
}
