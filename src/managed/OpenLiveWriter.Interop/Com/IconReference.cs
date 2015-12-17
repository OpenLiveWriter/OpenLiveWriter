// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;

namespace OpenLiveWriter.Interop.Com
{
    /// <summary>
    /// A refence to an icon resource
    /// </summary>
    public struct IconReference
    {
        #region Private members

        private string moduleName;
        private int resourceId;
        private string referencePath;
        static private char[] commaSeparator = new char[] { ',' };

        #endregion

        /// <summary>
        /// Overloaded constructor takes in the module name and resource id for the icon reference.
        /// </summary>
        /// <param name="moduleName">String specifying the name of an executable file, DLL, or icon file</param>
        /// <param name="resourceId">Zero-based index of the icon</param>
        public IconReference(string moduleName, int resourceId)
        {
            if (string.IsNullOrEmpty(moduleName))
                throw new ArgumentNullException("moduleName", "Module name cannot be null or empty string");

            this.moduleName = moduleName;
            this.resourceId = resourceId;
            referencePath = moduleName + "," + resourceId;
        }

        /// <summary>
        /// Overloaded constructor takes in the module name and resource id separated by a comma.
        /// </summary>
        /// <param name="refPath">Reference path for the icon consisting of the module name and resource id.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.Int32.Parse(System.String)", Justification = "We are not currently handling globalization or localization")]
        public IconReference(string refPath)
        {
            if (string.IsNullOrEmpty(refPath))
                throw new ArgumentNullException("refPath", "Reference path cannot be null or empty string");

            string[] refParams = refPath.Split(commaSeparator);

            if (refParams.Length != 2 || string.IsNullOrEmpty(refParams[0]) || string.IsNullOrEmpty(refParams[1]))
                throw new ArgumentException("Reference path is invalid.");

            moduleName = refParams[0];
            resourceId = int.Parse(refParams[1]);

            this.referencePath = refPath;
        }

        /// <summary>
        /// String specifying the name of an executable file, DLL, or icon file
        /// </summary>
        public string ModuleName
        {
            get
            {
                return moduleName;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException("value", "Module name cannot be null or empty string");

                moduleName = value;
            }
        }

        /// <summary>
        /// Zero-based index of the icon
        /// </summary>
        public int ResourceId
        {
            get
            {
                return resourceId;
            }
            set
            {
                resourceId = value;
            }
        }

        /// <summary>
        /// Reference to a specific icon within a EXE, DLL or icon file.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.Int32.Parse(System.String)", Justification = "We are not currently handling globalization or localization")]
        public string ReferencePath
        {
            get
            {
                return referencePath;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException("value", "Reference path cannot be null or empty string");

                string[] refParams = value.Split(commaSeparator);

                if (refParams.Length != 2 || string.IsNullOrEmpty(refParams[0]) || string.IsNullOrEmpty(refParams[1]))
                    throw new ArgumentException("Reference path is invalid.");

                ModuleName = refParams[0];
                ResourceId = int.Parse(refParams[1]);

                referencePath = value;
            }
        }

    }
}
