// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace OpenLiveWriter.Interop.Com.Ribbon
{
    public class BasicCollection : IEnumUnknown
    {
        private ArrayList _collection;
        private int _enumIndex;

        //
        // Initialization

        public BasicCollection()
        {
            _collection = new ArrayList();
            _enumIndex = 0;
        }

        public BasicCollection(ICollection c)
        {
            _collection = new ArrayList(c);
            _enumIndex = 0;
        }

        // Private copy ctor, used for Clone().
        private BasicCollection(BasicCollection that)
        {
            _collection = that._collection.Clone() as ArrayList;
            _enumIndex = that._enumIndex;
        }

        //
        // IEnumUnknown implementation

        Int32 IEnumUnknown.Next(uint celt, object[] rgelt, IntPtr pceltFetched)
        {
            // Check rgelt pointer.
            if (rgelt == null)
                throw new ArgumentNullException("rgelt");

            // Zero-init the rgelt buffer.
            for (uint i = 0; i < celt; ++i)
                rgelt[i] = null;

            // Fill the buffer.
            uint nFetched = 0;
            for (uint i = 0; i < celt; ++i)
            {
                if (_enumIndex >= _collection.Count)
                    break;

                rgelt[i] = _collection[_enumIndex];

                ++nFetched;
                ++_enumIndex;
            }

            // Careful: caller can pass NULL for pceltFetched.
            if (pceltFetched != IntPtr.Zero)
                Marshal.WriteInt32(pceltFetched, (int)nFetched);

            // Return S_FALSE if num elements fetched is < num elements requested.
            if (nFetched != celt)
                return 1;//false

            // Else return S_OK.
            return 0;//ok
        }

        Int32 IEnumUnknown.Skip(uint celt)
        {
            _enumIndex += (int)celt;

            // Return S_FALSE if we're skipping past the final element.
            if (_enumIndex >= _collection.Count)
                return 1;//false

            // Else return S_OK.
            return 0;//ok
        }

        void IEnumUnknown.Reset()
        {
            _enumIndex = 0;
        }

        IEnumUnknown IEnumUnknown.Clone()
        {
            IEnumUnknown clone = new BasicCollection(this);
            return clone;
        }
    }
}
