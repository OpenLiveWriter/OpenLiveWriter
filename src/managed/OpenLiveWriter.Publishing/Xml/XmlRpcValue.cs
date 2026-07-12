// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Globalization;
using System.Xml;

namespace OpenLiveWriter.Publishing.Xml
{
    /// <summary>
    /// Cross-platform port of the XML-RPC value serialization types from
    /// <c>OpenLiveWriter.CoreServices.XmlRpcClient</c>. Only <c>System.Xml</c> is
    /// used, so these build and run on macOS. The wire format is byte-for-byte
    /// identical to the Windows implementation.
    /// </summary>
    public abstract class XmlRpcValue
    {
        protected XmlRpcValue(object value)
            : this(value, false)
        {
        }

        protected XmlRpcValue(object value, bool suppressLog)
        {
            _value = value;
            _suppressLog = suppressLog;
        }

        public void Write(XmlWriter writer)
        {
            Write(writer, false);
        }

        public void Write(XmlWriter writer, bool logging)
        {
            using (new WriteXmlElement(writer, "value"))
            {
                WriteValue(writer, _value, logging);
            }
        }

        protected virtual void WriteValue(XmlWriter writer, object value, bool logging)
        {
            if (!_suppressLog || !logging)
                WriteValue(writer, value);
            else
                writer.WriteString("[removed]");
        }

        protected abstract void WriteValue(XmlWriter writer, object value);

        private readonly object _value;
        private readonly bool _suppressLog;
    }

    public class XmlRpcString : XmlRpcValue
    {
        public XmlRpcString(string value, bool suppressLog)
            : base(value, suppressLog)
        {
        }

        public XmlRpcString(string value)
            : base(value)
        {
        }

        protected override void WriteValue(XmlWriter writer, object value)
        {
            using (new WriteXmlElement(writer, "string"))
                writer.WriteString(value as string);
        }
    }

    public class XmlRpcInt : XmlRpcValue
    {
        public XmlRpcInt(int value)
            : base(value)
        {
        }

        protected override void WriteValue(XmlWriter writer, object value)
        {
            using (new WriteXmlElement(writer, "int"))
                writer.WriteString(((int)value).ToString(CultureInfo.InvariantCulture));
        }
    }

    public class XmlRpcBoolean : XmlRpcValue
    {
        public XmlRpcBoolean(bool value)
            : base(value)
        {
        }

        protected override void WriteValue(XmlWriter writer, object value)
        {
            using (new WriteXmlElement(writer, "boolean"))
                writer.WriteString((bool)value ? "1" : "0");
        }
    }

    public class XmlRpcBase64 : XmlRpcValue
    {
        public XmlRpcBase64(byte[] bytes)
            : base(bytes)
        {
        }

        protected override void WriteValue(XmlWriter writer, object value, bool logging)
        {
            byte[] bytes = (byte[])value;
            using (new WriteXmlElement(writer, "base64"))
            {
                if (!logging)
                    writer.WriteBase64(bytes, 0, bytes.Length);
                else
                    writer.WriteString(string.Format(CultureInfo.InvariantCulture, "[{0} bytes]", bytes.Length));
            }
        }

        protected override void WriteValue(XmlWriter writer, object value)
        {
            throw new InvalidOperationException("This should never be called");
        }
    }

    public class XmlRpcArray : XmlRpcValue
    {
        public XmlRpcArray(XmlRpcValue[] values)
            : base(values)
        {
        }

        protected override void WriteValue(XmlWriter writer, object value, bool logging)
        {
            using (new WriteXmlElement(writer, "array"))
            using (new WriteXmlElement(writer, "data"))
                foreach (XmlRpcValue val in (value as XmlRpcValue[]))
                {
                    val.Write(writer, logging);
                }
        }

        protected override void WriteValue(XmlWriter writer, object value)
        {
            throw new InvalidOperationException("This should never be called");
        }
    }

    public class XmlRpcStruct : XmlRpcValue
    {
        public XmlRpcStruct(XmlRpcMember[] members)
            : base(members)
        {
        }

        protected override void WriteValue(XmlWriter writer, object value, bool logging)
        {
            using (new WriteXmlElement(writer, "struct"))
            {
                foreach (XmlRpcMember member in (value as XmlRpcMember[]))
                {
                    using (new WriteXmlElement(writer, "member"))
                    {
                        using (new WriteXmlElement(writer, "name"))
                            writer.WriteString(member.Name);
                        member.Value.Write(writer, logging);
                    }
                }
            }
        }

        protected override void WriteValue(XmlWriter writer, object value)
        {
            throw new InvalidOperationException("This should never be called");
        }
    }

    public class XmlRpcMember
    {
        public XmlRpcMember(string name, string value)
            : this(name, new XmlRpcString(value))
        {
        }

        public XmlRpcMember(string name, string value, bool suppressLog)
            : this(name, new XmlRpcString(value, suppressLog))
        {
        }

        public XmlRpcMember(string name, bool value)
            : this(name, new XmlRpcBoolean(value))
        {
        }

        public XmlRpcMember(string name, int value)
            : this(name, new XmlRpcInt(value))
        {
        }

        public XmlRpcMember(string name, XmlRpcMember[] members)
            : this(name, new XmlRpcStruct(members))
        {
        }

        public XmlRpcMember(string name, XmlRpcValue value)
        {
            Name = name;
            _value = value;
        }

        public readonly string Name;

        public XmlRpcValue Value => _value;

        private readonly XmlRpcValue _value;
    }

    /// <summary>Utility class used to write elements in a scoped (using) block.</summary>
    internal sealed class WriteXmlElement : IDisposable
    {
        public WriteXmlElement(XmlWriter writer, string elName)
        {
            _writer = writer;
            _writer.WriteStartElement(elName);
        }

        public void Dispose()
        {
            _writer.WriteEndElement();
        }

        private readonly XmlWriter _writer;
    }
}
