// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Collections;

namespace OpenLiveWriter.Api
{
    /// <summary>
    /// Interface that describes a hierarchical property-set.
    /// </summary>
    public interface IProperties
    {
        /// <summary>
        /// Gets or sets the string value associated with the specified property name.
        /// </summary>
        string this[string name]
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the string value associated with the specified property name.
        /// </summary>
        /// <param name="name">Property name.</param>
        /// <param name="defaultValue">Default value for property if it does not exist.</param>
        /// <returns>Property value (or specified default value if the property does not exist).</returns>
        string GetString(string name, string defaultValue);

        /// <summary>
        /// Sets the property to the specified string value.
        /// </summary>
        /// <param name="name">Property name.</param>
        /// <param name="value">Property value.</param>
        void SetString(string name, string value);

        /// <summary>
        /// Gets the integer value associated with the specified property name.
        /// </summary>
        /// <param name="name">Property name.</param>
        /// <param name="defaultValue">Default value for property if it does not exist.</param>
        /// <returns>Property value (or specified default value if the property does not exist).</returns>
        int GetInt(string name, int defaultValue);

        /// <summary>
        /// Sets the property to the specified integer value.
        /// </summary>
        /// <param name="name">Property name.</param>
        /// <param name="value">Property value.</param>
        void SetInt(string name, int value);

        /// <summary>
        /// Gets the boolean value associated with the specified property name.
        /// </summary>
        /// <param name="name">Property name.</param>
        /// <param name="defaultValue">Default value for property if it does not exist.</param>
        /// <returns>Property value (or specified default value if the property does not exist).</returns>
        bool GetBoolean(string name, bool defaultValue);

        /// <summary>
        /// Sets the property to the specified boolean value.
        /// </summary>
        /// <param name="name">Property name.</param>
        /// <param name="value">Property value.</param>
        void SetBoolean(string name, bool value);

        /// <summary>
        /// Gets the float value associated with the specified property name.
        /// </summary>
        /// <param name="name">Property name.</param>
        /// <param name="defaultValue">Default value for property if it does not exist.</param>
        /// <returns>Property value (or specified default value if the property does not exist).</returns>
        float GetFloat(string name, float defaultValue);

        /// <summary>
        /// Sets the property to the specified float value.
        /// </summary>
        /// <param name="name">Property name.</param>
        /// <param name="value">Property value.</param>
        void SetFloat(string name, float value);

        /// <summary>
        /// Gets the decimal value associated with the specified property name.
        /// </summary>
        /// <param name="name">Property name.</param>
        /// <param name="defaultValue">Default value for property if it does not exist.</param>
        /// <returns>Property value (or specified default value if the property does not exist).</returns>
        decimal GetDecimal(string name, decimal defaultValue);

        /// <summary>
        /// Sets the property to the specified decimal value.
        /// </summary>
        /// <param name="name">Property name.</param>
        /// <param name="value">Property value.</param>
        void SetDecimal(string name, decimal value);

        /// <summary>
        /// List of property names contained within the property set.
        /// </summary>
        string[] Names { get; }

        /// <summary>
        /// Remove a property.
        /// </summary>
        /// <param name="name">Name of property to remove.</param>
        void Remove(string name);

        /// <summary>
        /// Check whether the property set contains a property of a specified name.
        /// </summary>
        /// <param name="name">Property name.</param>
        /// <returns>True if the property set contains the property, otherwise false.</returns>
        bool Contains(string name);

        /// <summary>
        /// List of sub-properties (nested property sets) contained within the property set
        /// </summary>
        string[] SubPropertyNames { get; }

        /// <summary>
        /// Retrieve an interface to a nested property set
        /// </summary>
        /// <param name="name">Sub-property set name.</param>
        /// <returns>Interface to specified property set (automatically creates a sub-properties if one of the
        /// specified name does not already exist).</returns>
        IProperties GetSubProperties(string name);

        /// <summary>
        /// Remove a sub-property set.
        /// </summary>
        /// <param name="name">Sub-property set name.</param>
        void RemoveSubProperties(string name);

        /// <summary>
        /// Check whether the property set contains a sub-properties of a specified name.
        /// </summary>
        /// <param name="name">Sub-property set name.</param>
        /// <returns>True if the property set contains the sub-properties, otherwise false.</returns>
        bool ContainsSubProperties(string name);

        /// <summary>
        /// Remove all properties and sub-properties from this property set.
        /// </summary>
        void RemoveAll();
    }
}
