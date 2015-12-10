// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Math helper class.
    /// </summary>
    public class MathHelper
    {
        /// <summary>
        /// Initializes a new instance of the MathHelper class.
        /// </summary>
        private MathHelper()
        {
        }

        public static int HexToInt(char h)
        {
            if ((h >= '0') && (h <= '9'))
            {
                return (h - '0');
            }
            if ((h >= 'a') && (h <= 'f'))
            {
                return ((h - 'a') + 10);
            }
            if ((h >= 'A') && (h <= 'F'))
            {
                return ((h - 'A') + 10);
            }
            return -1;
        }

        /// <summary>
        /// Limits a value to be within a specified range.  If value is less than minimumValue,
        /// minimumValue will returned.  If value is greater than maximumValue, maximumValue will
        /// be returned.  Otherwise, value will be returned.
        /// </summary>
        /// <param name="value">The value to compare.</param>
        /// <param name="minimumValue">The minimum value to return.</param>
        /// <param name="maximumValue">The maximum value to return.</param>
        /// <returns></returns>
        public static int Clip(int value, int minimumValue, int maximumValue)
        {
            if (value < minimumValue)
                return minimumValue;
            else if (value > maximumValue)
                return maximumValue;
            else
                return value;
        }

        public static int Max(params int[] values)
        {
            if (values.Length == 0)
                throw new ArgumentException("At least one value is required", "values");

            int max = values[0];
            for (int i = 1; i < values.Length; i++)
                max = Math.Max(max, values[i]);

            return max;
        }

        public static int Min(params int[] values)
        {
            if (values.Length == 0)
                throw new ArgumentException("At least one value is required", "values");

            int min = values[0];
            for (int i = 1; i < values.Length; i++)
                min = Math.Min(min, values[i]);

            return min;
        }
    }
}
