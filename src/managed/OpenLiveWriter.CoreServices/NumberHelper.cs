// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace OpenLiveWriter.CoreServices
{
    public static class NumberHelper
    {

        public static string IntToString(int n)
        {
            string pattern = "{0}";
            string negativeSymbol = CultureInfo.CurrentCulture.NumberFormat.NegativeSign;

            if (n < 0)
            {
                // http://msdn.microsoft.com/en-us/library/system.globalization.numberformatinfo.numbernegativepattern.aspx
                switch (CultureInfo.CurrentCulture.NumberFormat.NumberNegativePattern)
                {
                    case 0:
                        pattern = "({0})";
                        break;
                    case 1:
                        pattern = negativeSymbol + "{0}";
                        break;
                    case 2:
                        pattern = negativeSymbol + " {0}";
                        break;
                    case 3:
                        pattern = "{0}" + negativeSymbol;
                        break;
                    case 4:
                        pattern = "{0} " + negativeSymbol;
                        break;
                }
            }

            return String.Format(CultureInfo.CurrentCulture, pattern, Math.Abs(n));
        }

        // http://www.taygeta.com/random/gaussian.html
        // http://en.wikipedia.org/wiki/Marsaglia_polar_method
        public static void GaussianDistributionRandom(Random rand, out double r1, out double r2)
        {
            double x;
            double y;
            double s;
            do
            {
                x = 2 * rand.NextDouble() - 1;
                y = 2 * rand.NextDouble() - 1;
                s = x * x + y * y;
            } while (s >= 1.0);

            double f = Math.Sqrt(-2 * Math.Log(s) / s);
            r1 = x * f;
            r2 = y * f;
        }
    }
}
