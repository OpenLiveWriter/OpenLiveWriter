// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Drawing;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Win32;

namespace OpenLiveWriter.CoreServices.Settings
{
    /// <summary>
    /// Convenience class which allows one to use an ISettingsPersister more easily.  Method naming
    /// follows the convention of the System.Convert class for consistency.
    /// </summary>
    public class SettingsPersisterHelper : IDisposable
    {
        /// <summary>
        /// The ISettingsPersister that this SettingsPersisterHelper is bound to.
        /// </summary>
        private ISettingsPersister settingsPersister;

        /// <summary>
        /// Dispose the settings persister helper
        /// </summary>
        public void Dispose()
        {
            settingsPersister.Dispose();
        }

        /// <summary>
        /// Gets the ISettingsPersister that this SettingsPersisterHelper is bound to.  This is an
        /// escape hatch which allows one to access the ISettingsPersister directly.
        /// </summary>
        public ISettingsPersister SettingsPersister
        {
            get
            {
                return settingsPersister;
            }
        }

        /// <summary>
        /// Initializes a new instance of the SettingsPersisterHelper class.
        /// </summary>
        /// <param name="settingsPersister">The ISettingsPersister that this SettingsPersisterHelper is bound to.</param>
        public SettingsPersisterHelper(ISettingsPersister settingsPersister)
        {
            //	Set the settings persister.
            this.settingsPersister = settingsPersister;
        }

        /// <summary>
        /// Copy the contents of the source settings into this settings
        /// </summary>
        /// <param name="sourceSettings"></param>
        public void CopyFrom(SettingsPersisterHelper sourceSettings, bool recursive, bool overWrite)
        {
            // copy root-level values (to work with types generically we need references
            // to the underlying settings persister objects)
            ISettingsPersister source = sourceSettings.SettingsPersister;
            ISettingsPersister destination = SettingsPersister;
            foreach (string name in source.GetNames())
                if (overWrite || destination.Get(name) == null)
                    destination.Set(name, source.Get(name));

            // if this is recursive then copy all of the sub-settings
            if (recursive)
            {
                foreach (string subSetting in sourceSettings.GetSubSettingNames())
                {
                    using (SettingsPersisterHelper
                                sourceSubSetting = sourceSettings.GetSubSettings(subSetting),
                                destinationSubSetting = this.GetSubSettings(subSetting))
                    {
                        destinationSubSetting.CopyFrom(sourceSubSetting, recursive, overWrite);
                    }
                }
            }
        }

        /// <summary>
        /// Get the names of available settings.
        /// </summary>
        public string[] GetNames()
        {
            return settingsPersister.GetNames();
        }

        public bool HasValue(string name)
        {
            return settingsPersister.Get(name) != null;
        }

        public bool HasSubSettings(string subSettingsName)
        {
            return SettingsPersister.HasSubSettings(subSettingsName);
        }

        public SettingsPersisterHelper GetSubSettings(string subSettingsName)
        {
            return new SettingsPersisterHelper(SettingsPersister.GetSubSettings(subSettingsName));
        }

        /// <summary>
        /// Returns the names of the available subsettings.
        /// </summary>
        /// <returns></returns>
        public string[] GetSubSettingNames()
        {
            return SettingsPersister.GetSubSettings();
        }

        /// <summary>
        /// Gets the Unicode character setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to get.</param>
        /// <param name="defaultValue">The default value to return in the event that a setting
        /// with the specified name cannot be found, or an exception occurs.</param>
        /// <returns>The value of the setting, or the default value.</returns>
        public char GetChar(string name, char defaultValue)
        {
            return (char)settingsPersister.Get(name, typeof(char), defaultValue);
        }

        /// <summary>
        /// Gets the string setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to get.</param>
        /// <param name="defaultValue">The default value to return in the event that a setting
        /// with the specified name cannot be found, or an exception occurs.</param>
        /// <returns>The value of the setting, or the default value.</returns>
        public string GetString(string name, string defaultValue)
        {
            return (string)settingsPersister.Get(name, typeof(string), defaultValue);
        }

        public string GetEncryptedString(string name)
        {
            byte[] encrypted = GetByteArray(name, null);
            if (encrypted == null)
                return null;

            try
            {
                return CryptHelper.Decrypt(encrypted);
            }
            catch (Exception e)
            {
                Trace.Fail("Failure during decrypt: " + e);
                return null;
            }
        }

        public string[] GetStrings(string name, params string[] defaultValues)
        {
            return (string[])settingsPersister.Get(name, typeof(string[]), defaultValues);
        }

        /// <summary>
        /// Gets the enum value setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to get.</param>
        /// <param name="enumType">The type of enumeration.</param>
        /// <param name="defaultValue">The default value to return in the event that a setting
        /// with the specified name cannot be found, or an exception occurs.</param>
        /// <returns>The value of the setting, or the default value.</returns>
        public object GetEnumValue(string name, Type enumType, object defaultValue)
        {
            string val = GetString(name, defaultValue.ToString());
            if (val == null)
                return defaultValue;
            try
            {
                return Enum.Parse(enumType, val, true);
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Gets the boolean setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to get.</param>
        /// <param name="defaultValue">The default value to return in the event that a setting
        /// with the specified name cannot be found, or an exception occurs.</param>
        /// <returns>The value of the setting, or the default value.</returns>
        public bool GetBoolean(string name, bool defaultValue)
        {
            return (bool)settingsPersister.Get(name, typeof(bool), defaultValue);
        }

        /// <summary>
        /// Gets the 8-bit signed integer setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to get.</param>
        /// <param name="defaultValue">The default value to return in the event that a setting
        /// with the specified name cannot be found, or an exception occurs.</param>
        /// <returns>The value of the setting, or the default value.</returns>
        public sbyte GetSByte(string name, sbyte defaultValue)
        {
            return (sbyte)settingsPersister.Get(name, typeof(sbyte), defaultValue);
        }

        /// <summary>
        /// Gets the 8-bit unsigned integer setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to get.</param>
        /// <param name="defaultValue">The default value to return in the event that a setting
        /// with the specified name cannot be found, or an exception occurs.</param>
        /// <returns>The value of the setting, or the default value.</returns>
        public byte GetByte(string name, byte defaultValue)
        {
            return (byte)settingsPersister.Get(name, typeof(byte), defaultValue);
        }

        /// <summary>
        /// Gets the 16-bit signed integer setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to get.</param>
        /// <param name="defaultValue">The default value to return in the event that a setting
        /// with the specified name cannot be found, or an exception occurs.</param>
        /// <returns>The value of the setting, or the default value.</returns>
        public short GetInt16(string name, short defaultValue)
        {
            return (short)settingsPersister.Get(name, typeof(short), defaultValue);
        }

        /// <summary>
        /// Gets the 16-bit unsigned integer setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to get.</param>
        /// <param name="defaultValue">The default value to return in the event that a setting
        /// with the specified name cannot be found, or an exception occurs.</param>
        /// <returns>The value of the setting, or the default value.</returns>
        public ushort GetUInt16(string name, ushort defaultValue)
        {
            return (ushort)settingsPersister.Get(name, typeof(ushort), defaultValue);
        }

        /// <summary>
        /// Gets the 32-bit signed integer setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to get.</param>
        /// <param name="defaultValue">The default value to return in the event that a setting
        /// with the specified name cannot be found, or an exception occurs.</param>
        /// <returns>The value of the setting, or the default value.</returns>
        public int GetInt32(string name, int defaultValue)
        {
            return (int)settingsPersister.Get(name, typeof(int), defaultValue);
        }

        /// <summary>
        /// Gets the 32-bit usigned integer setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to get.</param>
        /// <param name="defaultValue">The default value to return in the event that a setting
        /// with the specified name cannot be found, or an exception occurs.</param>
        /// <returns>The value of the setting, or the default value.</returns>
        public uint GetUInt32(string name, uint defaultValue)
        {
            return (uint)settingsPersister.Get(name, typeof(uint), defaultValue);
        }

        /// <summary>
        /// Gets the 64-bit signed integer setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to get.</param>
        /// <param name="defaultValue">The default value to return in the event that a setting
        /// with the specified name cannot be found, or an exception occurs.</param>
        /// <returns>The value of the setting, or the default value.</returns>
        public long GetInt64(string name, long defaultValue)
        {
            return (long)settingsPersister.Get(name, typeof(long), defaultValue);
        }

        /// <summary>
        /// Gets the 64-bit usigned integer setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to get.</param>
        /// <param name="defaultValue">The default value to return in the event that a setting
        /// with the specified name cannot be found, or an exception occurs.</param>
        /// <returns>The value of the setting, or the default value.</returns>
        public ulong GetUInt64(string name, ulong defaultValue)
        {
            return (ulong)settingsPersister.Get(name, typeof(ulong), defaultValue);
        }

        /// <summary>
        /// Gets the double-precision floating point number setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to get.</param>
        /// <param name="defaultValue">The default value to return in the event that a setting
        /// with the specified name cannot be found, or an exception occurs.</param>
        /// <returns>The value of the setting, or the default value.</returns>
        public double GetDouble(string name, double defaultValue)
        {
            return (double)settingsPersister.Get(name, typeof(double), defaultValue);
        }

        /// <summary>
        /// Gets the single-precision floating point number setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to get.</param>
        /// <param name="defaultValue">The default value to return in the event that a setting
        /// with the specified name cannot be found, or an exception occurs.</param>
        /// <returns>The value of the setting, or the default value.</returns>
        public float GetFloat(string name, float defaultValue)
        {
            return (float)settingsPersister.Get(name, typeof(float), defaultValue);
        }

        /// <summary>
        /// Gets the decimal number setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to get.</param>
        /// <param name="defaultValue">The default value to return in the event that a setting
        /// with the specified name cannot be found, or an exception occurs.</param>
        /// <returns>The value of the setting, or the default value.</returns>
        public decimal GetDecimal(string name, decimal defaultValue)
        {
            return (decimal)settingsPersister.Get(name, typeof(decimal), defaultValue);
        }

        /// <summary>
        /// Gets the DateTime setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to get.</param>
        /// <param name="defaultValue">The default value to return in the event that a setting
        /// with the specified name cannot be found, or an exception occurs.</param>
        /// <returns>The value of the setting, or the default value.</returns>
        public DateTime GetDateTime(string name, DateTime defaultValue)
        {
            return (DateTime)settingsPersister.Get(name, typeof(DateTime), defaultValue);
        }

        /// <summary>
        /// Gets the Rectangle setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to get.</param>
        /// <param name="defaultValue">The default value to return in the event that a setting
        /// with the specified name cannot be found, or an exception occurs.</param>
        /// <returns>The value of the setting, or the default value.</returns>
        public Rectangle GetRectangle(string name, Rectangle defaultValue)
        {
            return (Rectangle)settingsPersister.Get(name, typeof(Rectangle), defaultValue);
        }

        /// <summary>
        /// Gets the Point setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to get.</param>
        /// <param name="defaultValue">The default value to return in the event that a setting
        /// with the specified name cannot be found, or an exception occurs.</param>
        /// <returns>The value of the setting, or the default value.</returns>
        public Point GetPoint(string name, Point defaultValue)
        {
            return (Point)settingsPersister.Get(name, typeof(Point), defaultValue);
        }

        /// <summary>
        /// Gets the SizeF setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to get.</param>
        /// <param name="defaultValue">The default value to return in the event that a setting
        /// with the specified name cannot be found, or an exception occurs.</param>
        /// <returns>The value of the setting, or the default value.</returns>
        public SizeF GetSizeF(string name, SizeF defaultValue)
        {
            return (SizeF)settingsPersister.Get(name, typeof(SizeF), defaultValue);
        }

        /// <summary>
        /// Gets the Size setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to get.</param>
        /// <param name="defaultValue">The default value to return in the event that a setting
        /// with the specified name cannot be found, or an exception occurs.</param>
        /// <returns>The value of the setting, or the default value.</returns>
        public Size GetSize(string name, Size defaultValue)
        {
            return (Size)settingsPersister.Get(name, typeof(Size), defaultValue);
        }

        /// <summary>
        /// Gets the DateTime setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to get.</param>
        /// <param name="defaultValue">The default value to return in the event that a setting
        /// with the specified name cannot be found, or an exception occurs.</param>
        /// <returns>The value of the setting, or the default value.</returns>
        public byte[] GetByteArray(string name, byte[] defaultValue)
        {
            return (byte[])settingsPersister.Get(name, typeof(byte[]), defaultValue);
        }

        /// <summary>
        /// See ISettingsPersister.BatchUpdate()
        /// </summary>
        public IDisposable BatchUpdate()
        {
            return settingsPersister.BatchUpdate();
        }

        /// <summary>
        /// Sets the Unicode character setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to set.</param>
        /// <param name="value">The value to set.</param>
        public void SetChar(string name, char value)
        {
            settingsPersister.Set(name, value);
        }

        /// <summary>
        /// Sets the string setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to set.</param>
        /// <param name="value">The value to set.</param>
        public void SetString(string name, string value)
        {
            settingsPersister.Set(name, value);
        }

        public void SetEncryptedString(string name, string value)
        {
            SetByteArray(name, CryptHelper.Encrypt(value));
        }

        public void SetStrings(string name, params string[] values)
        {
            settingsPersister.Set(name, values);
        }

        /// <summary>
        /// Sets the boolean setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to set.</param>
        /// <param name="value">The value to set.</param>
        public void SetBoolean(string name, bool value)
        {
            settingsPersister.Set(name, value);
        }

        /// <summary>
        /// Sets the 8-bit signed integer setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to set.</param>
        /// <param name="value">The value to set.</param>
        public void SetSByte(string name, sbyte value)
        {
            settingsPersister.Set(name, value);
        }

        /// <summary>
        /// Sets the 8-bit unsigned integer setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to set.</param>
        /// <param name="value">The value to set.</param>
        public void SetByte(string name, byte value)
        {
            settingsPersister.Set(name, value);
        }

        /// <summary>
        /// Sets the 16-bit signed integer setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to set.</param>
        /// <param name="value">The value to set.</param>
        public void SetInt16(string name, short value)
        {
            settingsPersister.Set(name, value);
        }

        /// <summary>
        /// Sets the 16-bit unsigned integer setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to set.</param>
        /// <param name="value">The value to set.</param>
        public void SetUInt16(string name, ushort value)
        {
            settingsPersister.Set(name, value);
        }

        /// <summary>
        /// Sets the 32-bit signed integer setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to set.</param>
        /// <param name="value">The value to set.</param>
        public void SetInt32(string name, int value)
        {
            settingsPersister.Set(name, value);
        }

        /// <summary>
        /// Sets the 32-bit usigned integer setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to set.</param>
        /// <param name="value">The value to set.</param>
        public void SetUInt32(string name, uint value)
        {
            settingsPersister.Set(name, value);
        }

        /// <summary>
        /// Sets the 64-bit signed integer setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to set.</param>
        /// <param name="value">The value to set.</param>
        public void SetInt64(string name, long value)
        {
            settingsPersister.Set(name, value);
        }

        /// <summary>
        /// Sets the 64-bit usigned integer setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to set.</param>
        /// <param name="value">The value to set.</param>
        public void SetUInt64(string name, ulong value)
        {
            settingsPersister.Set(name, value);
        }

        /// <summary>
        /// Sets the double-precision floating point number setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to set.</param>
        /// <param name="value">The value to set.</param>
        public void SetDouble(string name, double value)
        {
            settingsPersister.Set(name, value);
        }

        /// <summary>
        /// Sets the single-precision floating point number setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to set.</param>
        /// <param name="value">The value to set.</param>
        public void SetFloat(string name, float value)
        {
            settingsPersister.Set(name, value);
        }

        /// <summary>
        /// Sets the decimal number setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to set.</param>
        /// <param name="value">The value to set.</param>
        public void SetDecimal(string name, decimal value)
        {
            settingsPersister.Set(name, value);
        }

        /// <summary>
        /// Sets the DateTime setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to set.</param>
        /// <param name="value">The value to set.</param>
        public void SetDateTime(string name, DateTime value)
        {
            settingsPersister.Set(name, value);
        }

        /// <summary>
        /// Sets the Rectangle setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to set.</param>
        /// <param name="value">The value to set.</param>
        public void SetRectangle(string name, Rectangle value)
        {
            settingsPersister.Set(name, value);
        }

        /// <summary>
        /// Sets the Point setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to set.</param>
        /// <param name="value">The value to set.</param>
        public void SetPoint(string name, Point value)
        {
            settingsPersister.Set(name, value);
        }

        /// <summary>
        /// Sets the Size setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to set.</param>
        /// <param name="value">The value to set.</param>
        public void SetSize(string name, Size value)
        {
            settingsPersister.Set(name, value);
        }

        /// <summary>
        /// Sets the SizeF setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to set.</param>
        /// <param name="value">The value to set.</param>
        public void SetSizeF(string name, SizeF value)
        {
            settingsPersister.Set(name, value);
        }

        /// <summary>
        /// Sets the byte array setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to set.</param>
        /// <param name="value">The value to set.</param>
        public void SetByteArray(string name, byte[] value)
        {
            settingsPersister.Set(name, value);
        }

        public void Unset(string name)
        {
            settingsPersister.Unset(name);
        }

        public void UnsetSubsettingTree(string name)
        {
            if (settingsPersister.HasSubSettings(name))
                settingsPersister.UnsetSubSettingsTree(name);
        }

        /// <summary>
        /// Returns a SettingsPersisterHelper for the first RegistryKeySpec that exists
        /// in the registry, or null if the registry doesn't contain any of them.
        /// </summary>
        public static SettingsPersisterHelper OpenFirstRegistryKey(params RegistryKeySpec[] keySpecs)
        {
            foreach (RegistryKeySpec spec in keySpecs)
            {
                using (RegistryKey key = spec.baseKey.OpenSubKey(spec.subkey, false))
                {
                    if (key != null)
                    {
                        if (spec.requiredValue != null && key.GetValue(spec.requiredValue, null) == null)
                            continue;
                        return new SettingsPersisterHelper(new RegistrySettingsPersister(spec.baseKey, spec.subkey));
                    }
                }
            }
            return null;
        }
    }

    public class RegistryKeySpec
    {
        internal readonly RegistryKey baseKey;
        internal readonly string subkey;
        internal readonly string requiredValue;

        /// <summary>
        /// Specifies a particular key.
        /// </summary>
        public RegistryKeySpec(RegistryKey baseKey, string subkey) : this(baseKey, subkey, null)
        {
        }

        /// <summary>
        /// Specifies a particular key AND that a value name exists in that key.
        /// </summary>
        public RegistryKeySpec(RegistryKey baseKey, string subkey, string requiredValue)
        {
            this.baseKey = baseKey;
            this.subkey = subkey;
            this.requiredValue = requiredValue;
        }
    }
}
