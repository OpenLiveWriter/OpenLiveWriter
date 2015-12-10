// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.UnitTest.CoreServices
{
    [TestFixture]
    public class ArrayHelperTest
    {
        [Test]
        public void InsertionSortTest()
        {
            VerifyInsertionSort(
                new int[] {3, 1, 2, 4, 5},
                Comparer<int>.Default.Compare,
                new int[] {1, 2, 3, 4, 5});

            VerifyInsertionSort(
                new int[] {1, 1, 2, 5, 4},
                Comparer<int>.Default.Compare,
                new int[] {1, 1, 2, 4, 5});

            VerifyInsertionSort(
                new int[] {},
                Comparer<int>.Default.Compare,
                new int[] {});

            VerifyInsertionSort(
                new int[] {6, 3, 1, 4, 4, 5},
                delegate { return 0; },
                new int[] {6, 3, 1, 4, 4, 5});

            VerifyInsertionSort(
                new int[] {6, 3, 1, 4, 4, 5},
                delegate (int a, int b) { return a % 2 - b % 2; },
                new int[] {6, 4, 4, 3, 1, 5});
        }

        private static void VerifyInsertionSort<T>(T[] unsorted, Comparison<T> comparison, T[] expected)
        {
            Assert.AreEqual(unsorted.Length, expected.Length, "Error in test: inputs are different lengths");

            List<T> list = new List<T>(unsorted);
            ArrayHelper.InsertionSort(list, comparison);

            bool same = true;
            for (int i = 0; i < list.Count; i++)
            {
                same &= list[i].Equals(expected[i]);
            }
            Assert.IsTrue(same, "Orders were different");
        }
    }
}
