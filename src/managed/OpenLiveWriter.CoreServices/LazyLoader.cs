// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Text;

namespace OpenLiveWriter.CoreServices
{
    public class LazyLoader<T>
    {
        public delegate T LazyLoaderDelegate();
        private bool isInit = false;
        private T _value;
        private LazyLoaderDelegate _valueCalculator;

        public LazyLoader(LazyLoaderDelegate valueCalculator)
        {
            _valueCalculator = valueCalculator;
        }

        public T Value
        {
            get
            {
                if (isInit != true)
                {
                    _value = _valueCalculator();
                    isInit = true;
                }
                return _value;
            }
            set
            {
                _value = value;
                isInit = true;
            }
        }

        public static implicit operator T(LazyLoader<T> obj)
        {
            return obj.Value;
        }

        public void Clear()
        {
            isInit = false;
        }

        public bool IsInitialized
        {
            get { return isInit; }
        }

    }
}
