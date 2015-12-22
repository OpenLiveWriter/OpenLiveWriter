// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Windows.Forms;

namespace OpenLiveWriter.Controls.MarginUtil
{
    public enum MarginType
    {
        DefaultMargins,
        NoMargins,
        CustomMargins
    }

    public class MarginComboItem
    {
        private readonly MarginType _type;
        private readonly string _text;

        public MarginComboItem(string text, MarginType type)
        {
            _text = text;
            _type = type;
        }

        public MarginType Type
        {
            get { return _type; }
        }

        public override string ToString()
        {
            return _text;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (!(obj is MarginComboItem)) return false;
            return Equals((MarginComboItem)obj);
        }

        public bool Equals(MarginComboItem obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return Equals(obj._type, _type);
        }

        public override int GetHashCode()
        {
            return _type.GetHashCode();
        }
    }

}
