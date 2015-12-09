// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using OpenLiveWriter.SpellChecker;

namespace OpenLiveWriter.UnitTest.SpellChecker
{
    [TestFixture]
    public class SortedMarkupRangeListTest
    {
        [Test]
        public void Test()
        {
            List<int> nums = new List<int>() {2, 4, 6, 8, 10};
            Comparer<int> comparer = Comparer<int>.Default;

            Assert.AreEqual(SortedMarkupRangeList.BinarySearch(nums, 2, comparer), 0);
            Assert.AreEqual(SortedMarkupRangeList.BinarySearch(nums, 4, comparer), 1);
            Assert.AreEqual(SortedMarkupRangeList.BinarySearch(nums, 6, comparer), 2);
            Assert.AreEqual(SortedMarkupRangeList.BinarySearch(nums, 8, comparer), 3);
            Assert.AreEqual(SortedMarkupRangeList.BinarySearch(nums, 10, comparer), 4);

            Assert.AreEqual(SortedMarkupRangeList.BinarySearch(nums, 11, comparer), ~5);
            Assert.AreEqual(SortedMarkupRangeList.BinarySearch(nums, 0, comparer), ~0);
            Assert.AreEqual(SortedMarkupRangeList.BinarySearch(nums, 1, comparer), ~0);
            Assert.AreEqual(SortedMarkupRangeList.BinarySearch(nums, 3, comparer), ~1);

            nums = new List<int>() {2, 4};
            Assert.AreEqual(SortedMarkupRangeList.BinarySearch(nums, 2, comparer), 0);
            Assert.AreEqual(SortedMarkupRangeList.BinarySearch(nums, 4, comparer), 1);
            Assert.AreEqual(SortedMarkupRangeList.BinarySearch(nums, 0, comparer), ~0);
            Assert.AreEqual(SortedMarkupRangeList.BinarySearch(nums, 3, comparer), ~1);
            Assert.AreEqual(SortedMarkupRangeList.BinarySearch(nums, 5, comparer), ~2);

            nums = new List<int>() { 2 };
            Assert.AreEqual(SortedMarkupRangeList.BinarySearch(nums, 2, comparer), 0);
            Assert.AreEqual(SortedMarkupRangeList.BinarySearch(nums, 0, comparer), ~0);
            Assert.AreEqual(SortedMarkupRangeList.BinarySearch(nums, 3, comparer), ~1);

            nums = new List<int>();
            Assert.AreEqual(SortedMarkupRangeList.BinarySearch(nums, 0, comparer), ~0);
            Assert.AreEqual(SortedMarkupRangeList.BinarySearch(nums, 3, comparer), ~0);
        }
    }
}
