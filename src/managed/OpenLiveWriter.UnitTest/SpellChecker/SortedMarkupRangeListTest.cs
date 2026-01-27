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

            Assert.That(SortedMarkupRangeList.BinarySearch(nums, 2, comparer), Is.EqualTo(0));
            Assert.That(SortedMarkupRangeList.BinarySearch(nums, 4, comparer), Is.EqualTo(1));
            Assert.That(SortedMarkupRangeList.BinarySearch(nums, 6, comparer), Is.EqualTo(2));
            Assert.That(SortedMarkupRangeList.BinarySearch(nums, 8, comparer), Is.EqualTo(3));
            Assert.That(SortedMarkupRangeList.BinarySearch(nums, 10, comparer), Is.EqualTo(4));

            Assert.That(SortedMarkupRangeList.BinarySearch(nums, 11, comparer), Is.EqualTo(~5));
            Assert.That(SortedMarkupRangeList.BinarySearch(nums, 0, comparer), Is.EqualTo(~0));
            Assert.That(SortedMarkupRangeList.BinarySearch(nums, 1, comparer), Is.EqualTo(~0));
            Assert.That(SortedMarkupRangeList.BinarySearch(nums, 3, comparer), Is.EqualTo(~1));

            nums = new List<int>() {2, 4};
            Assert.That(SortedMarkupRangeList.BinarySearch(nums, 2, comparer), Is.EqualTo(0));
            Assert.That(SortedMarkupRangeList.BinarySearch(nums, 4, comparer), Is.EqualTo(1));
            Assert.That(SortedMarkupRangeList.BinarySearch(nums, 0, comparer), Is.EqualTo(~0));
            Assert.That(SortedMarkupRangeList.BinarySearch(nums, 3, comparer), Is.EqualTo(~1));
            Assert.That(SortedMarkupRangeList.BinarySearch(nums, 5, comparer), Is.EqualTo(~2));

            nums = new List<int>() { 2 };
            Assert.That(SortedMarkupRangeList.BinarySearch(nums, 2, comparer), Is.EqualTo(0));
            Assert.That(SortedMarkupRangeList.BinarySearch(nums, 0, comparer), Is.EqualTo(~0));
            Assert.That(SortedMarkupRangeList.BinarySearch(nums, 3, comparer), Is.EqualTo(~1));

            nums = new List<int>();
            Assert.That(SortedMarkupRangeList.BinarySearch(nums, 0, comparer), Is.EqualTo(~0));
            Assert.That(SortedMarkupRangeList.BinarySearch(nums, 3, comparer), Is.EqualTo(~0));
        }
    }
}
