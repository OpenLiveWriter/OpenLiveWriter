// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;

namespace OpenLiveWriter.CoreServices.Settings
{
    public abstract class XmlSettingsPersister : ISettingsPersister
    {
        protected internal Hashtable values;
        protected internal Hashtable subsettings;

        protected XmlSettingsPersister(Hashtable values, Hashtable subsettings)
        {
            this.values = values;
            this.subsettings = subsettings;
        }

        internal abstract void Persist();
        public abstract IDisposable BatchUpdate();

        protected internal abstract object SyncRoot { get; }

        public string[] GetNames()
        {
            lock (SyncRoot)
            {
                ArrayList nameList = new ArrayList(values.Keys);
                nameList.Sort();
                return (string[])nameList.ToArray(typeof(string));
            }
        }

        public object Get(string name, Type desiredType, object defaultValue)
        {
            lock (SyncRoot)
            {
                object o = Get(name);
                if (desiredType.IsInstanceOfType(o))
                    return o;
                else if (defaultValue != null)
                {
                    values[name] = defaultValue;
                    Persist();
                    return defaultValue;
                }
                else
                    return null;
            }
        }

        public object Get(string name)
        {
            lock (SyncRoot)
            {
                return values[name];
            }
        }

        public void Set(string name, object value)
        {
            lock (SyncRoot)
            {
                values[name] = value;
                Persist();
            }
        }

        public void Unset(string name)
        {
            lock (SyncRoot)
            {
                values.Remove(name);
                Persist();
            }
        }

        public void UnsetSubSettingsTree(string name)
        {
            lock (SyncRoot)
            {
                subsettings.Remove(name);
                Persist();
            }
        }

        public bool HasSubSettings(string subSettingsName)
        {
            lock (SyncRoot)
            {
                return subsettings.ContainsKey(subSettingsName);
            }
        }

        public ISettingsPersister GetSubSettings(string subSettingsName)
        {
            lock (SyncRoot)
            {
                if (!subsettings.ContainsKey(subSettingsName))
                    subsettings[subSettingsName] = new Hashtable[] { new Hashtable(), new Hashtable() };
                Hashtable[] tables = (Hashtable[])subsettings[subSettingsName];
                return new XmlChildSettingsPersister(this, tables[0], tables[1]);
            }
        }

        public string[] GetSubSettings()
        {
            lock (SyncRoot)
            {
                ArrayList keyList = new ArrayList(subsettings.Keys);
                keyList.Sort();
                return (string[])keyList.ToArray(typeof(string));
            }
        }

        public virtual void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            GC.SuppressFinalize(this);
        }

        ~XmlSettingsPersister()
        {
            Dispose(false);
            //Debug.Fail("Failed to dispose XmlSettingsPersister");
        }
    }

    internal class XmlChildSettingsPersister : XmlSettingsPersister
    {
        private readonly XmlSettingsPersister parent;

        public XmlChildSettingsPersister(XmlSettingsPersister parent, Hashtable values, Hashtable subsettings)
            : base(values, subsettings)
        {
            this.parent = parent;
        }

        protected internal override object SyncRoot
        {
            get { return parent.SyncRoot; }
        }

        public override IDisposable BatchUpdate()
        {
            return parent.BatchUpdate();
        }

        internal override void Persist()
        {
            parent.Persist();
        }
    }

    public class XmlFileSettingsPersister : XmlSettingsPersister
    {
        private readonly Stream stream;
        private readonly object syncRoot = new object();
        private int batchUpdateRefCount = 0;

        private XmlFileSettingsPersister(Stream stream, Hashtable values, Hashtable subsettings)
            : base(values, subsettings)
        {
            this.stream = stream;
        }

        protected internal override object SyncRoot
        {
            get { return syncRoot; }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                stream.Dispose();
            }
            base.Dispose(false);

        }

        public static XmlFileSettingsPersister Open(string filename)
        {
            Stream s = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
            if (s.Length == 0)
            {
                return new XmlFileSettingsPersister(s, new Hashtable(), new Hashtable());
            }
            else
            {
                Hashtable values, subsettings;
                Parse(s, out values, out subsettings);
                return new XmlFileSettingsPersister(s, values, subsettings);
            }
        }

        private static void Parse(Stream s, out Hashtable values, out Hashtable subsettings)
        {
            values = new Hashtable();
            subsettings = new Hashtable();

            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(s);
            }
            catch (Exception e)
            {
                // Deal with legacy corruption caused by bug 762601
                Trace.WriteLine(e.ToString());
                return;
            }
            System.Xml.XmlElement root = doc.DocumentElement;
            Parse(root, values, subsettings);
        }

        private static void Parse(XmlElement node, Hashtable values, Hashtable subsettings)
        {
            foreach (System.Xml.XmlElement valueNode in node.SelectNodes("value"))
            {
                string name = valueNode.GetAttribute("name");
                string type = valueNode.GetAttribute("type");
                string value = valueNode.InnerText;
                values[name] = ParseValue(type, value);
            }

            foreach (System.Xml.XmlElement settingsNode in node.SelectNodes("settings"))
            {
                string name = settingsNode.GetAttribute("name");
                Hashtable subvalues = new Hashtable();
                Hashtable subsubsettings = new Hashtable();
                Parse(settingsNode, subvalues, subsubsettings);
                subsettings[name] = new Hashtable[] { subvalues, subsubsettings };
            }
        }

        private static object ParseValue(string type, string value)
        {
            switch ((int)Enum.Parse(typeof(ValueType), type))
            {
                case (int)ValueType.Char:
                    return char.Parse(value);
                case (int)ValueType.String:
                    return value;
                case (int)ValueType.Bool:
                    return bool.Parse(value);
                case (int)ValueType.SByte:
                    return sbyte.Parse(value, CultureInfo.InvariantCulture);
                case (int)ValueType.Byte:
                    return byte.Parse(value, CultureInfo.InvariantCulture);
                case (int)ValueType.Int16:
                    return Int16.Parse(value, CultureInfo.InvariantCulture);
                case (int)ValueType.UInt16:
                    return UInt16.Parse(value, CultureInfo.InvariantCulture);
                case (int)ValueType.Int32:
                    return Int32.Parse(value, CultureInfo.InvariantCulture);
                case (int)ValueType.UInt32:
                    return UInt32.Parse(value, CultureInfo.InvariantCulture);
                case (int)ValueType.Int64:
                    return Int64.Parse(value, CultureInfo.InvariantCulture);
                case (int)ValueType.UInt64:
                    return UInt64.Parse(value, CultureInfo.InvariantCulture);
                case (int)ValueType.Double:
                    return Double.Parse(value, CultureInfo.InvariantCulture);
                case (int)ValueType.Float:
                    return float.Parse(value, CultureInfo.InvariantCulture);
                case (int)ValueType.Decimal:
                    return decimal.Parse(value, CultureInfo.InvariantCulture);
                case (int)ValueType.DateTime:
                    return DateTime.Parse(value, CultureInfo.InvariantCulture);
                case (int)ValueType.Rectangle:
                    {
                        string[] ints = StringHelper.Split(value, ",");
                        return new Rectangle(
                            int.Parse(ints[0], CultureInfo.InvariantCulture),
                            int.Parse(ints[1], CultureInfo.InvariantCulture),
                            int.Parse(ints[2], CultureInfo.InvariantCulture),
                            int.Parse(ints[3], CultureInfo.InvariantCulture)
                            );
                    }
                case (int)ValueType.Point:
                    {
                        string[] ints = StringHelper.Split(value, ",");
                        return new Point(
                            int.Parse(ints[0], CultureInfo.InvariantCulture),
                            int.Parse(ints[1], CultureInfo.InvariantCulture)
                            );
                    }
                case (int)ValueType.Size:
                    {
                        string[] ints = StringHelper.Split(value, ",");
                        return new Size(
                            int.Parse(ints[0], CultureInfo.InvariantCulture),
                            int.Parse(ints[1], CultureInfo.InvariantCulture)
                            );
                    }
                case (int)ValueType.SizeF:
                    {
                        string[] floats = StringHelper.Split(value, ",");
                        return new SizeF(
                            float.Parse(floats[0], CultureInfo.InvariantCulture),
                            float.Parse(floats[1], CultureInfo.InvariantCulture)
                            );
                    }
                case (int)ValueType.Strings:
                    return StringHelper.SplitWithEscape(value, ',', '\\');
                case (int)ValueType.ByteArray:
                    return Convert.FromBase64String(value);
                default:
                    Trace.Fail("Unknown type " + type);
                    return null;
            }
        }

        private static void UnparseValue(object input, out ValueType valueType, out string output)
        {
            if (input == null)
            {
                Trace.Fail("Null input is not allowed here");
                throw new ArgumentNullException("input", "Null input is not allowed here");
            }

            if (input is char)
            {
                valueType = ValueType.Char;
                output = input.ToString();
            }
            else if (input is string)
            {
                valueType = ValueType.String;
                output = (string)input;
            }
            else if (input is bool)
            {
                valueType = ValueType.Bool;
                output = input.ToString();
            }
            else if (input is sbyte)
            {
                valueType = ValueType.SByte;
                output = ((sbyte)input).ToString(CultureInfo.InvariantCulture);
            }
            else if (input is byte)
            {
                valueType = ValueType.Byte;
                output = ((byte)input).ToString(CultureInfo.InvariantCulture);
            }
            else if (input is Int16)
            {
                valueType = ValueType.Int16;
                output = ((short)input).ToString(CultureInfo.InvariantCulture);
            }
            else if (input is UInt16)
            {
                valueType = ValueType.UInt16;
                output = ((UInt16)input).ToString(CultureInfo.InvariantCulture);
            }
            else if (input is Int32)
            {
                valueType = ValueType.Int32;
                output = ((Int32)input).ToString(CultureInfo.InvariantCulture);
            }
            else if (input is UInt32)
            {
                valueType = ValueType.UInt32;
                output = ((UInt32)input).ToString(CultureInfo.InvariantCulture);
            }
            else if (input is Int64)
            {
                valueType = ValueType.Int64;
                output = ((Int64)input).ToString(CultureInfo.InvariantCulture);
            }
            else if (input is UInt64)
            {
                valueType = ValueType.UInt64;
                output = ((UInt64)input).ToString(CultureInfo.InvariantCulture);
            }
            else if (input is Double)
            {
                valueType = ValueType.Double;
                output = ((double)input).ToString(CultureInfo.InvariantCulture);
            }
            else if (input is float)
            {
                valueType = ValueType.Float;
                output = ((float)input).ToString(CultureInfo.InvariantCulture);
            }
            else if (input is decimal)
            {
                valueType = ValueType.Decimal;
                output = ((decimal)input).ToString(CultureInfo.InvariantCulture);
            }
            else if (input is DateTime)
            {
                valueType = ValueType.DateTime;
                output = ((DateTime)input).ToString(CultureInfo.InvariantCulture);
            }
            else if (input is Rectangle)
            {
                valueType = ValueType.Rectangle;
                Rectangle rect = (Rectangle)input;
                output = string.Format(CultureInfo.InvariantCulture,
                                       "{0},{1},{2},{3}",
                                       rect.Left.ToString(CultureInfo.InvariantCulture),
                                       rect.Top.ToString(CultureInfo.InvariantCulture),
                                       rect.Width.ToString(CultureInfo.InvariantCulture),
                                       rect.Height.ToString(CultureInfo.InvariantCulture));
            }
            else if (input is Point)
            {
                valueType = ValueType.Point;
                Point pt = (Point)input;
                output = string.Format(CultureInfo.InvariantCulture, "{0},{1}",
                                       pt.X,
                                       pt.Y);
            }
            else if (input is Size)
            {
                valueType = ValueType.Size;
                Size sz = (Size)input;
                output = string.Format(CultureInfo.InvariantCulture, "{0},{1}",
                                       sz.Width,
                                       sz.Height);
            }
            else if (input is SizeF)
            {
                valueType = ValueType.SizeF;
                SizeF sz = (SizeF)input;
                output = string.Format(CultureInfo.InvariantCulture, "{0},{1}",
                                       sz.Width,
                                       sz.Height);
            }
            else if (input is string[])
            {
                valueType = ValueType.Strings;
                StringBuilder sb = new StringBuilder();
                string[] values = new string[((string[])input).Length];
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = ((string[])input)[i].Replace("\\", "\\\\").Replace(",", "\\,");
                }
                output = StringHelper.Join(values, ",");
            }
            else if (input is byte[])
            {
                valueType = ValueType.ByteArray;
                output = Convert.ToBase64String((byte[])input);
            }
            else
            {
                throw new ArgumentException("Unexpected valueType: " + input.GetType().FullName);
            }
        }

        enum ValueType
        {
            Char,
            String,
            Bool,
            SByte,
            Byte,
            Int16,
            UInt16,
            Int32,
            UInt32,
            Int64,
            UInt64,
            Double,
            Float,
            Decimal,
            DateTime,
            Rectangle,
            Point,
            Size,
            SizeF,
            Strings,
            ByteArray
        }

        public override IDisposable BatchUpdate()
        {
            return new BatchUpdateHelper(this);
        }

        private class BatchUpdateHelper : IDisposable
        {
            private readonly XmlFileSettingsPersister parent;
            private int disposed = 0;

            public BatchUpdateHelper(XmlFileSettingsPersister parent)
            {
                this.parent = parent;
                parent.BeginUpdate();
            }

            public void Dispose()
            {
                if (Interlocked.CompareExchange(ref disposed, 1, 0) == 0)
                    parent.EndUpdate();
            }
        }

        private void BeginUpdate()
        {
            lock (SyncRoot)
            {
                batchUpdateRefCount++;
            }
        }

        private void EndUpdate()
        {
            lock (SyncRoot)
            {
                batchUpdateRefCount--;
                Persist();
                Trace.Assert(batchUpdateRefCount >= 0, "batchUpdateRefCount is less than zero");
            }
        }

        internal override void Persist()
        {
            lock (SyncRoot)
            {
                if (batchUpdateRefCount > 0)
                    return;

                XmlDocument xmlDoc = new XmlDocument();
                System.Xml.XmlElement settings = xmlDoc.CreateElement("settings");
                xmlDoc.AppendChild(settings);
                ToXml(settings, values, subsettings);

                stream.Position = 0;
                stream.SetLength(0);
                xmlDoc.Save(stream);
                stream.Flush();
            }
        }

        private static void ToXml(System.Xml.XmlElement settings, Hashtable values, Hashtable subsettings)
        {
            ArrayList valueKeys = new ArrayList(values.Keys);
            valueKeys.Sort();
            foreach (string key in valueKeys)
            {
                System.Xml.XmlElement el = settings.OwnerDocument.CreateElement("value");
                el.SetAttribute("name", key);
                object value = values[key];
                if (value != null)
                {
                    ValueType valueType;
                    string output;
                    UnparseValue(value, out valueType, out output);
                    el.SetAttribute("type", valueType.ToString());
                    el.InnerText = output;
                    settings.AppendChild(el);
                }
            }

            ArrayList subsettingsKeys = new ArrayList(subsettings.Keys);
            subsettingsKeys.Sort();
            foreach (string key in subsettingsKeys)
            {
                System.Xml.XmlElement el = settings.OwnerDocument.CreateElement("settings");
                el.SetAttribute("name", key);
                Hashtable[] hashtables = (Hashtable[])subsettings[key];
                if (hashtables != null)
                {
                    settings.AppendChild(el);
                    ToXml(el, hashtables[0], hashtables[1]);
                }
            }
        }
    }
}
