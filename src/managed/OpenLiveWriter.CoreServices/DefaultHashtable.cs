// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Hashtable that allows you to specify a default value.
    /// </summary>
    public class DefaultHashtable : Hashtable
    {
        public delegate object DefaultValuePump(object key);

        private DefaultValuePump defaultValuePump;

        public DefaultHashtable(DefaultValuePump valuePump)
        {
            this.defaultValuePump = valuePump;
        }

        public override object this[object key]
        {
            get
            {
                object val = base[key];
                if (val != null)
                    return val;

                // if key was explicitly set to null...
                if (this.ContainsKey(key))
                    return null;

                // need a default.
                object defVal = defaultValuePump(key);
                this[key] = defVal;
                return defVal;
            }
            set
            {
                base[key] = value;
            }
        }

        /// <summary>
        ///  Commonly used DefaultValuePump candidate
        /// </summary>
        public static object ZeroDefault(object key)
        {
            return 0;
        }

        public static object ArrayListDefault(object key)
        {
            return new ArrayList();
        }

        public static object LinkedListDefault(object key)
        {
            return new LinkedList();
        }

        public static object HashSetDefault(object key)
        {
            return new HashSet();
        }

        public static object TreeSetDefault(object key)
        {
            return new TreeSet();
        }

        public static object HashtableDefault(object key)
        {
            return new Hashtable();
        }

        public static object DateTimeMinValueDefault(object key)
        {
            return DateTime.MinValue;
        }
    }
}
