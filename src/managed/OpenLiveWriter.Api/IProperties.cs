// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.Api
{
    using JetBrains.Annotations;

    /// <summary>
    /// Interface that describes a hierarchical property-set.
    /// </summary>
    public interface IProperties
    {
        /// <summary>
        /// Gets the list of property names contained within the property set.
        /// </summary>
        [NotNull]
        string[] Names { get; }

        /// <summary>
        /// Gets the list of sub-properties (nested property sets) contained within the property set
        /// </summary>
        [NotNull]
        string[] SubPropertyNames { get; }

        /// <summary>
        /// Gets or sets the string value associated with the specified property name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The string value associated with the specified property name.</returns>
        [CanBeNull]
        string this[[NotNull] string name]
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
        [CanBeNull]
        string GetString([NotNull]string name, [CanBeNull] string defaultValue);

        /// <summary>
        /// Sets the property to the specified string value.
        /// </summary>
        /// <param name="name">Property name.</param>
        /// <param name="value">Property value.</param>
        void SetString([NotNull] string name, [CanBeNull] string value);

        /// <summary>
        /// Gets the integer value associated with the specified property name.
        /// </summary>
        /// <param name="name">Property name.</param>
        /// <param name="defaultValue">Default value for property if it does not exist.</param>
        /// <returns>Property value (or specified default value if the property does not exist).</returns>
        int GetInt([NotNull] string name, int defaultValue);

        /// <summary>
        /// Sets the property to the specified integer value.
        /// </summary>
        /// <param name="name">Property name.</param>
        /// <param name="value">Property value.</param>
        void SetInt([NotNull] string name, int value);

        /// <summary>
        /// Gets the boolean value associated with the specified property name.
        /// </summary>
        /// <param name="name">Property name.</param>
        /// <param name="defaultValue">Default value for property if it does not exist.</param>
        /// <returns>Property value (or specified default value if the property does not exist).</returns>
        bool GetBoolean([NotNull] string name, bool defaultValue);

        /// <summary>
        /// Sets the property to the specified boolean value.
        /// </summary>
        /// <param name="name">Property name.</param>
        /// <param name="value">Property value.</param>
        void SetBoolean([NotNull] string name, bool value);

        /// <summary>
        /// Gets the float value associated with the specified property name.
        /// </summary>
        /// <param name="name">Property name.</param>
        /// <param name="defaultValue">Default value for property if it does not exist.</param>
        /// <returns>Property value (or specified default value if the property does not exist).</returns>
        float GetFloat([NotNull] string name, float defaultValue);

        /// <summary>
        /// Sets the property to the specified float value.
        /// </summary>
        /// <param name="name">Property name.</param>
        /// <param name="value">Property value.</param>
        void SetFloat([NotNull] string name, float value);

        /// <summary>
        /// Gets the decimal value associated with the specified property name.
        /// </summary>
        /// <param name="name">Property name.</param>
        /// <param name="defaultValue">Default value for property if it does not exist.</param>
        /// <returns>Property value (or specified default value if the property does not exist).</returns>
        decimal GetDecimal([NotNull] string name, decimal defaultValue);

        /// <summary>
        /// Sets the property to the specified decimal value.
        /// </summary>
        /// <param name="name">Property name.</param>
        /// <param name="value">Property value.</param>
        void SetDecimal([NotNull] string name, decimal value);

        /// <summary>
        /// Remove a property.
        /// </summary>
        /// <param name="name">Name of property to remove.</param>
        void Remove([NotNull] string name);

        /// <summary>
        /// Check whether the property set contains a property of a specified name.
        /// </summary>
        /// <param name="name">Property name.</param>
        /// <returns>True if the property set contains the property, otherwise false.</returns>
        bool Contains([NotNull] string name);

        /// <summary>
        /// Retrieve an interface to a nested property set
        /// </summary>
        /// <param name="name">Sub-property set name.</param>
        /// <returns>Interface to specified property set (automatically creates a sub-properties if one of the
        /// specified name does not already exist).</returns>
        [NotNull]
        IProperties GetSubProperties([NotNull] string name);

        /// <summary>
        /// Remove a sub-property set.
        /// </summary>
        /// <param name="name">Sub-property set name.</param>
        void RemoveSubProperties([NotNull] string name);

        /// <summary>
        /// Check whether the property set contains a sub-properties of a specified name.
        /// </summary>
        /// <param name="name">Sub-property set name.</param>
        /// <returns>True if the property set contains the sub-properties, otherwise false.</returns>
        bool ContainsSubProperties([NotNull] string name);

        /// <summary>
        /// Remove all properties and sub-properties from this property set.
        /// </summary>
        void RemoveAll();
    }
}
