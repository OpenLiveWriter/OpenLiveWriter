// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

#pragma warning disable SYSLIB0011 // BinaryFormatter is obsolete - kept for backwards compatibility reading only
#pragma warning disable SYSLIB0051 // Formatter-based serialization is obsolete - kept for backwards compatibility

namespace OpenLiveWriter.CoreServices.Settings
{
    /// <summary>
    /// A simple mechanism for serializing/deserializing arbitrary data types to the registry.
    /// </summary>
    public class RegistryCodec
    {
        #region Singleton
        private static RegistryCodec singleton = new RegistryCodec();

        private RegistryCodec() { }

        public static RegistryCodec Instance
        {
            get
            {
                return singleton;
            }
        }
        #endregion

        /// <summary>
        /// The available codecs, in order of priority (highest priority first).
        /// Any <c>Codec</c> that is not part of this list will never get called.
        /// </summary>
        private Codec[] codecs = {
                                     // common types up top
                                     new StringCodec(),
                                     new BooleanCodec(),
                                     new Int32Codec(),
                                     new DoubleCodec(),
                                     new Int64Codec(),
                                     // now all other primitive types
                                     new SByteCodec(),
                                     new ByteCodec(),
                                     new CharCodec(),
                                     new Int16Codec(),
                                     new UInt16Codec(),
                                     new UInt32Codec(),
                                     new UInt64Codec(),
                                     new FloatCodec(),
                                     new DecimalCodec(),
                                     // date-time
                                     new DateTimeCodec(),
                                     new RectangleCodec(),
                                     new PointCodec(),
                                     new SizeCodec(),
                                     new SizeFCodec(),
                                     new MultiStringCodec(),
                                     // catch-all case
                                     new SerializableCodec()
                                 };

        /// <summary>
        /// A cache of previously matched type/codec pairs.
        /// Keys are <c>Types</c>, values are <c>Codecs</c>.
        /// </summary>
        private Hashtable codecCache = new Hashtable();

        /// <summary>
        /// Take a native value and return a registry-ready representation.
        /// </summary>
        public object Encode(object val)
        {
            return GetCodec(val.GetType()).Encode(val);
        }

        /// <summary>
        /// Take a registry representation and return a native value.
        /// </summary>
        public object Decode(object val, Type desiredType)
        {
            return GetCodec(desiredType).Decode(val, desiredType);
        }

        protected Codec GetCodec(Type type)
        {
            lock (this)
            {
                // if cached, return immediately
                if (codecCache.ContainsKey(type))
                    return (Codec)codecCache[type];

                // not cached?  then iterate over the list
                foreach (Codec c in codecs)
                {
                    if (c.CanHandle(type))
                    {
                        // put it in the cache
                        codecCache[type] = c;
                        return c;
                    }
                }
            }
            // this is bad!
            Debug.Fail("No codec was found for type " + type.ToString());

            return null;
        }

        /// <summary>
        /// Basic interface for encoding/decoding values to/from registry.
        /// Any concrete impl of this interface must be threadsafe and
        /// must be added to RegistryCodec.codecs.
        /// </summary>
        public abstract class Codec
        {
            /// <summary>
            /// Subclasses that can handle only one exact type should override this
            /// method. All other subclasses should override <c>CanHandle</c>. No
            /// subclass should override both.
            /// </summary>
            protected virtual Type Type() { return null; }

            /// <summary>
            /// Returns true if the codec can be used to encode/decode an object
            /// of the specified <c>Type</c>. Most subclasses should leave this alone
            /// and override the <c>Type</c> method.
            /// </summary>
            public virtual bool CanHandle(Type type) { return Type().Equals(type); }

            /// <summary>
            /// Convert a native object into a symmetrically persistable type (see below).
            ///
            /// This method will only be called if the type of the "val" parameter
            /// caused CanHandle to return true.  A properly written subclass will
            /// never throw ClassCastException on this method.
            ///
            /// The type returned by this method must be something that can
            /// be symmetrically persisted into the registry; i.e., when RegistryKey.GetValue
            /// is called, the value returned must be equal to the value that was
            /// previously passed in to RegistryKey.SetValue.  Types that are OK
            /// include strings, ints, longs, and byte arrays.  Types that are not OK
            /// include floating point values (which can be stored, but come back as
            /// strings).
            /// </summary>
            public abstract object Encode(object val);

            /// <summary>
            /// Convert a symmetrically persistable type back into a native object.
            ///
            /// The return value should be of the same type as Encode() expects
            /// to receive.  The val parameter can reasonably expected to be of
            /// the same type as Encode() returns.  However, if someone goes into
            /// the registry and changes the types of the values, for example, then
            /// all bets are off.  No need to code defensively around that case, though;
            /// that will be handled at a higher level.  It's OK just to throw an exception
            /// here.
            /// </summary>
            public abstract object Decode(object val);

            /// <summary>
            /// Convert a symmetrically persistable type back into a native object,
            /// given the type the caller expects. Codecs that can handle a variety
            /// of types (like SerializableCodec) need the desired type to produce
            /// the correctly typed value; the default implementation ignores it.
            /// </summary>
            public virtual object Decode(object val, Type desiredType)
            {
                return Decode(val);
            }
        }

        /// <summary>
        /// Abstract impl of codec for values that can be stored "as-is" in the registry.
        /// </summary>
        abstract class PassthoughCodec : Codec
        {
            public override object Encode(object val)
            {
                return val;
            }

            public override object Decode(object val)
            {
                return val;
            }
        }

        /// <summary>
        /// Abstract impl of codec for values that can be stored as their
        /// natural string representation in the registry.
        /// </summary>
        abstract class StringifyCodec : Codec
        {
            public override object Encode(object val)
            {
                return Convert.ToString(val, CultureInfo.InvariantCulture);
            }

            public sealed override object Decode(object val)
            {
                return DecodeString((string)val);
            }

            protected abstract object DecodeString(string val);
        }

        /// <summary>
        /// Abstract impl of codec for values that can be represented as ints.
        /// This is basically for the integral value types that are smaller than
        /// Int32.
        /// </summary>
        abstract class IntifyCodec : Codec
        {
            public override object Encode(object val)
            {
                return ((IConvertible)val).ToInt32(CultureInfo.InvariantCulture);
            }
        }

        class StringCodec : PassthoughCodec
        {
            protected override Type Type()
            {
                return typeof(string);
            }
        }

        class MultiStringCodec : PassthoughCodec
        {
            protected override Type Type()
            {
                return typeof(string[]);
            }

        }

        class BooleanCodec : Codec
        {
            protected override Type Type()
            {
                return typeof(bool);
            }

            public override object Encode(object val)
            {
                return (bool)val ? 1 : 0;
            }

            public override object Decode(object val)
            {
                int intValue = (int)val;
                return (int)val == 1;
            }
        }

        #region Integral datatypes
        class SByteCodec : IntifyCodec
        {
            protected override Type Type() { return typeof(sbyte); }
            public override object Decode(object val) { return ((IConvertible)val).ToSByte(NumberFormatInfo.CurrentInfo); }
        }

        class ByteCodec : IntifyCodec
        {
            protected override Type Type() { return typeof(byte); }
            public override object Decode(object val) { return ((IConvertible)val).ToByte(NumberFormatInfo.CurrentInfo); }
        }

        class CharCodec : StringifyCodec
        {
            protected override Type Type() { return typeof(char); }
            protected override object DecodeString(string val) { return val.ToCharArray(0, 1)[0]; }
        }

        class Int16Codec : IntifyCodec
        {
            protected override Type Type() { return typeof(short); }
            public override object Decode(object val) { return (short)val; }
        }

        class UInt16Codec : IntifyCodec
        {
            protected override Type Type() { return typeof(ushort); }
            public override object Decode(object val) { return (ushort)val; }
        }

        class Int32Codec : PassthoughCodec
        {
            protected override Type Type() { return typeof(int); }
        }

        class UInt32Codec : StringifyCodec
        {
            protected override Type Type() { return typeof(uint); }
            protected override object DecodeString(string val) { return uint.Parse(val, CultureInfo.InvariantCulture); }
        }

        class Int64Codec : StringifyCodec
        {
            protected override Type Type() { return typeof(long); }
            protected override object DecodeString(string val) { return long.Parse(val, CultureInfo.InvariantCulture); }
        }

        class UInt64Codec : StringifyCodec
        {
            protected override Type Type() { return typeof(ulong); }
            protected override object DecodeString(string val) { return ulong.Parse(val, CultureInfo.InvariantCulture); }
        }
        #endregion

        #region Floating-point datatypes
        class DoubleCodec : StringifyCodec
        {
            protected override Type Type()
            {
                return typeof(double);
            }

            protected override object DecodeString(string val)
            {
                return double.Parse(val, CultureInfo.InvariantCulture);
            }
        }

        class FloatCodec : StringifyCodec
        {
            protected override Type Type()
            {
                return typeof(float);
            }

            protected override object DecodeString(string val)
            {
                return float.Parse(val, CultureInfo.InvariantCulture);
            }
        }

        #endregion

        class DecimalCodec : StringifyCodec
        {
            protected override Type Type() { return typeof(decimal); }
            protected override object DecodeString(string val) { return decimal.Parse(val, CultureInfo.InvariantCulture); }
        }

        /// <summary>
        /// Uses a string of ticks (millis) as the registry representation.
        /// </summary>
        class DateTimeCodec : Codec
        {
            protected override Type Type()
            {
                return typeof(DateTime);
            }

            public override object Encode(object val)
            {
                return ((DateTime)val).Ticks.ToString(CultureInfo.InvariantCulture);
            }

            public override object Decode(object val)
            {
                return new DateTime(long.Parse((string)val, CultureInfo.InvariantCulture));
            }
        }

        /// <summary>
        /// Persists a System.Drawing.Rectangle into "<x>,<y>,<width>,<height>".
        /// Useful for saving System.Windows.Control.Bounds.
        /// </summary>
        class RectangleCodec : Codec
        {
            protected override Type Type()
            {
                return typeof(Rectangle);
            }

            public override object Encode(object val)
            {
                Rectangle r = (Rectangle)val;
                return string.Format(CultureInfo.InvariantCulture, "{0},{1},{2},{3}", r.Left, r.Top, r.Width, r.Height);
            }

            public override object Decode(object val)
            {
                string[] strings = ((string)val).Split(',');
                if (strings.Length != 4)
                    return null;
                try
                {
                    return new Rectangle(
                        int.Parse(strings[0], CultureInfo.InvariantCulture),
                        int.Parse(strings[1], CultureInfo.InvariantCulture),
                        int.Parse(strings[2], CultureInfo.InvariantCulture),
                        int.Parse(strings[3], CultureInfo.InvariantCulture));
                }
                catch
                {
                    return null;
                }
            }

        }

        /// <summary>
        /// Persists a System.Drawing.Point into "<x>,<y>".
        /// Useful for saving System.Windows.Control.Bounds.Location.
        /// </summary>
        class PointCodec : Codec
        {
            protected override Type Type()
            {
                return typeof(Point);
            }

            public override object Encode(object val)
            {
                Point p = (Point)val;
                return string.Format(CultureInfo.InvariantCulture, "{0},{1}", p.X, p.Y);
            }

            public override object Decode(object val)
            {
                string[] strings = ((string)val).Split(',');
                if (strings.Length != 2)
                    return null;
                try
                {
                    return new Point(
                        int.Parse(strings[0], CultureInfo.InvariantCulture),
                        int.Parse(strings[1], CultureInfo.InvariantCulture));
                }
                catch
                {
                    return null;
                }
            }

        }

        /// <summary>
        /// Persists a System.Drawing.Size into "<x>,<y>".
        /// Useful for saving System.Windows.Control.Bounds.Size.
        /// </summary>
        class SizeCodec : Codec
        {
            protected override Type Type()
            {
                return typeof(Size);
            }

            public override object Encode(object val)
            {
                Size s = (Size)val;
                return string.Format(CultureInfo.InvariantCulture, "{0},{1}", s.Width, s.Height);
            }

            public override object Decode(object val)
            {
                string[] strings = ((string)val).Split(',');
                if (strings.Length != 2)
                    return null;
                try
                {
                    return new Size(
                        int.Parse(strings[0], CultureInfo.InvariantCulture),
                        int.Parse(strings[1], CultureInfo.InvariantCulture));
                }
                catch
                {
                    return null;
                }
            }

        }

        /// <summary>
        /// Persists a System.Drawing.Size into "<x>,<y>".
        /// Useful for saving System.Windows.Control.Bounds.Size.
        /// </summary>
        class SizeFCodec : Codec
        {
            protected override Type Type()
            {
                return typeof(SizeF);
            }

            public override object Encode(object val)
            {
                SizeF s = (SizeF)val;
                return string.Format(CultureInfo.InvariantCulture, "{0},{1}", s.Width, s.Height);
            }

            public override object Decode(object val)
            {
                string[] strings = ((string)val).Split(',');
                if (strings.Length != 2)
                    return null;
                try
                {
                    return new SizeF(
                        float.Parse(strings[0], CultureInfo.InvariantCulture),
                        float.Parse(strings[1], CultureInfo.InvariantCulture));
                }
                catch
                {
                    return null;
                }
            }

        }

        /// <summary>
        /// Handles any serializable type by converting to/from byte array.
        /// Uses JSON for new data, with backwards compatibility for legacy BinaryFormatter data.
        /// </summary>
        /// <remarks>
        /// Migration strategy:
        /// - New data is always saved as JSON (with "OLW_JSON:" prefix for identification)
        /// - Legacy BinaryFormatter data is detected and deserialized for backwards compatibility
        /// - On next save, data will be migrated to JSON format automatically
        /// </remarks>
        internal class SerializableCodec : Codec
        {
            // Magic header to identify JSON-serialized data (UTF-8 bytes)
            private static readonly byte[] JsonMagicHeader = Encoding.UTF8.GetBytes("OLW_JSON:");

            // JSON serializer options with support for common scenarios
            private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
            {
                WriteIndented = false,
                PropertyNamingPolicy = null, // Preserve original property names
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                IncludeFields = true, // Include public fields (important for some types)
                Converters = { new JsonStringEnumConverter() } // Serialize enums as strings
            };

            /// <summary>
            /// Unlike the other codecs, Serializable can handle a variety
            /// of types--anything that can be serialized.
            /// </summary>
            public override bool CanHandle(Type type)
            {
                return type.IsSerializable;
            }

            public override object Encode(object val)
            {
                // Serialize to JSON
                string json = JsonSerializer.Serialize(val, val.GetType(), JsonOptions);
                byte[] jsonBytes = Encoding.UTF8.GetBytes(json);

                // Prefix with magic header so we can identify JSON data on decode
                byte[] result = new byte[JsonMagicHeader.Length + jsonBytes.Length];
                Array.Copy(JsonMagicHeader, 0, result, 0, JsonMagicHeader.Length);
                Array.Copy(jsonBytes, 0, result, JsonMagicHeader.Length, jsonBytes.Length);

                return result;
            }

            public override object Decode(object val)
            {
                return Deserialize((byte[])val);
            }

            public override object Decode(object val, Type desiredType)
            {
                return Deserialize((byte[])val, desiredType);
            }

            /// <summary>
            /// Deserializes data, automatically detecting format (JSON or legacy BinaryFormatter).
            /// </summary>
            public static object Deserialize(byte[] data)
            {
                return Deserialize(data, null);
            }

            /// <summary>
            /// Deserializes data to the specified type, automatically detecting format.
            /// </summary>
            public static object Deserialize(byte[] data, Type targetType)
            {
                if (data == null || data.Length == 0)
                    return null;

                // Check for JSON magic header
                if (IsJsonFormat(data))
                {
                    return DeserializeJson(data, targetType);
                }

                // Fall back to legacy BinaryFormatter for backwards compatibility
                return DeserializeLegacyBinaryFormatter(data);
            }

            private static bool IsJsonFormat(byte[] data)
            {
                if (data.Length < JsonMagicHeader.Length)
                    return false;

                for (int i = 0; i < JsonMagicHeader.Length; i++)
                {
                    if (data[i] != JsonMagicHeader[i])
                        return false;
                }
                return true;
            }

            private static object DeserializeJson(byte[] data, Type targetType)
            {
                // Skip the magic header
                int jsonStart = JsonMagicHeader.Length;
                int jsonLength = data.Length - jsonStart;
                string json = Encoding.UTF8.GetString(data, jsonStart, jsonLength);

                if (targetType != null)
                {
                    return JsonSerializer.Deserialize(json, targetType, JsonOptions);
                }

                // If no target type specified, deserialize as JsonElement and return
                // This is a fallback - callers should specify the type when possible
                return JsonSerializer.Deserialize<JsonElement>(json, JsonOptions);
            }

            /// <summary>
            /// Legacy BinaryFormatter deserialization for backwards compatibility with existing settings.
            /// This method is kept to read old settings that were serialized before the JSON migration.
            /// </summary>
            private static object DeserializeLegacyBinaryFormatter(byte[] data)
            {
                try
                {
                    using (MemoryStream ms = new MemoryStream(data))
                    {
                        var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                        return formatter.Deserialize(ms);
                    }
                }
                catch (Exception ex)
                {
                    // If BinaryFormatter fails, the data might be corrupted or in an unknown format
                    Trace.TraceWarning($"Failed to deserialize legacy BinaryFormatter data: {ex.Message}");
                    return null;
                }
            }
        }

    }
}
