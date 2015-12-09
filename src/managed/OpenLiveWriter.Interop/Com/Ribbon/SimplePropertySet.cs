// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace OpenLiveWriter.Interop.Com.Ribbon
{
    public class SimplePropertySet : IUISimplePropertySet
    {
        PropertyDictionary _props = new PropertyDictionary();

        //
        // Interface

        public void Add(PropertyKey key, PropVariant value)
        {
            _props.Add(key, value);
        }

        //
        // IUISimplePropertySet implementation

        Int32 IUISimplePropertySet.GetValue(ref PropertyKey key, out PropVariant value)
        {
            if (_props.ContainsKey(key))
            {
                value = _props[key].Clone();
                return 0;//ok
            }

            value = PropVariant.Empty;
            return Marshal.GetHRForException(new ArgumentException()); // E_INVALIDARG
        }

        private class PropertyDictionary : Dictionary<PropertyKey, PropVariant>
        {
            ~PropertyDictionary()
            {
                foreach (PropVariant propVariant in Values)
                {
                    propVariant.Clear();
                }
            }
        }
    }
}
