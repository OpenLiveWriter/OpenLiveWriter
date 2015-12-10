// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Text;

namespace OpenLiveWriter.CoreServices
{
    public static class TypeHelper
    {
        // A generic instance (e.g. List<int>) is not a subclass of its generic definition (e.g List<T>),
        // so we have to walk up the inheritance tree manually for this.
        // Adapted from: http://stackoverflow.com/questions/457676/
        public static bool IsInstanceOfGeneric(Type generic, Type toCheck)
        {
            while (toCheck != typeof(object))
            {
                var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == cur)
                    return true;

                toCheck = toCheck.BaseType;
            }
            return false;
        }
    }
}
