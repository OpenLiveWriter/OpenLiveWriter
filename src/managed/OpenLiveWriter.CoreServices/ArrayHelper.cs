// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace OpenLiveWriter.CoreServices
{
    public sealed class ArrayHelper
    {
        /// <summary>
        /// Cannot be instantiated or subclassed
        /// </summary>
        private ArrayHelper()
        {
        }

        /// <summary>
        /// Removes nulls from an array of objects.
        /// </summary>
        public static object[] Compact(object[] arr)
        {
            int i = 0;
            int j = 0;
            for (; i < arr.Length; i++)
            {
                if (i != j)
                    arr[j] = arr[i];

                if (arr[i] != null)
                    j++;
            }

            if (i == j)
                return arr;
            else
            {
                Array a = (Array)arr;
                Truncate(ref a, j);
                return (object[])a;
            }

        }

        /// <summary>
        /// truncates the first dimension of any array
        /// </summary>
        public static void Truncate(ref Array arr, int len)
        {
            if (arr.Length == len)
                return;
            Array arr1 = Array.CreateInstance(arr.GetType().GetElementType(), len);
            Array.Copy(arr, arr1, len);
            arr = arr1;
        }

        /// <summary>
        /// Return the intersection of n arrays.
        /// </summary>
        /// <param name="arrays">
        ///		0..n arrays (or an array of arrays).  All arrays should have
        ///		the same underlying type.  Duplicate elements within an
        ///		input array is allowed, but the return value is guaranteed not
        ///		to contain duplicates.
        ///	 </param>
        /// <returns>
        ///		An array of the elements which are present in all arrays.
        ///		The order of the elements is not guaranteed.
        /// </returns>
        public static Array Intersection(Array[] arrays)
        {
            Type underlyingType = arrays.GetType().GetElementType().GetElementType();
            if (arrays.Length == 0)
                return Array.CreateInstance(underlyingType, 0);

            if (arrays.Length == 1)
                return arrays[0];

            // using Hashtable as a HashSet
            Hashtable outerTable = new Hashtable(arrays[0].Length * 2 + 1);
            bool isFirstTrip = true;
            foreach (Array array in arrays)
            {
                Hashtable innerTable = new Hashtable(array.Length);
                foreach (object o in array)
                {
                    // prevent per-array duplicates from being counted more than once
                    if (innerTable.ContainsKey(o))
                        continue;
                    innerTable[o] = "";

                    if (!isFirstTrip && !outerTable.ContainsKey(o))
                    {
                        // if this is not the first array we're looking at, don't even
                        // bother counting items that aren't already in the table--we
                        // know they won't be a part of the final set
                        continue;
                    }
                    else if (isFirstTrip)
                    {
                        outerTable[o] = 0;
                    }
                    outerTable[o] = (int)outerTable[o] + 1;
                }
                isFirstTrip = false;
            }

            Array intersection = new object[outerTable.Count];

            int pos = 0;
            foreach (DictionaryEntry entry in outerTable)
            {
                if ((int)entry.Value == arrays.Length)
                {
                    intersection.SetValue(entry.Key, pos++);
                }
            }
            Truncate(ref intersection, pos);
            if (intersection.Length > 0)
                return Narrow(intersection, underlyingType);
            else
                return intersection;
        }

        /// <summary>
        /// Return the union of n arrays.
        /// </summary>
        /// <param name="arrays">
        ///		0..n arrays (or an array of arrays).  All arrays should have
        ///		the same underlying type.  Duplicate elements within an
        ///		input array is allowed, but the return value is guaranteed not
        ///		to contain duplicates.
        ///	 </param>
        /// <returns>
        ///		An array of the elements which are present in any array.
        ///		The order of the elements is not guaranteed.
        /// </returns>
        public static T[] Union<T>(params T[][] arrays)
        {
            if (arrays.Length == 0)
                return new T[0];

            if (arrays.Length == 1)
                return arrays[0];

            // using Hashtable as a HashSet
            Hashtable table = new Hashtable(arrays[0].Length * arrays.Length + 1);
            foreach (Array array in arrays)
            {
                foreach (object o in array)
                {
                    table[o] = true;
                }
            }

            T[] union = new T[table.Count];

            int pos = 0;
            foreach (object o in table.Keys)
            {
                union.SetValue(o, pos++);
            }
            return union;
        }

        public static Array Narrow(Array array, Type type)
        {
            Array newArray = Array.CreateInstance(type, array.LongLength);
            Array.Copy(array, newArray, array.LongLength);
            return newArray;
        }

        public delegate object ArrayMapperDelegate(object input);
        /// <summary>
        /// Map the elements of an array onto a new array via a provided function.
        /// </summary>
        public static Array Map(Array array, Type newArrayType, ArrayMapperDelegate mapper)
        {
            Array newArray = Array.CreateInstance(newArrayType, array.LongLength);
            for (long i = 0; i < newArray.LongLength; i++)
            {
                newArray.SetValue(mapper(array.GetValue(i)), i);
            }
            return newArray;
        }

        public static Array CollectionToArray(ICollection collection, Type elementType)
        {
            Array newArray = Array.CreateInstance(elementType, collection.Count);
            collection.CopyTo(newArray, 0);
            return newArray;
        }

        public static Array EnumerableToArray(IEnumerable enumerable, int count, Type elementType)
        {
            Array newArray = Array.CreateInstance(elementType, count);
            int i = 0;
            foreach (object o in enumerable)
                newArray.SetValue(o, i++);
            return newArray;
        }

        /// <summary>
        /// Compares two arrays for equality.
        /// </summary>
        /// <param name="a">Array a.</param>
        /// <param name="b">Array b.</param>
        /// <returns>true if the string arrays are equal; otherwise, false.</returns>
        public static bool Compare(object[] a, object[] b)
        {
            if (a == b)
                return true;
            else if (a == null || b == null)
                return false;
            else if (a.Length != b.Length)
                return false;
            else if (a.GetType() != b.GetType())
                return false;
            else
            {
                for (int i = 0; i < a.Length; i++)
                    if (a[i] != b[i])
                        return false;
                return true;
            }
        }

        public static bool CompareBytes(byte[] a, byte[] b)
        {
            if (a == b)
                return true;
            else if (a == null || b == null)
                return false;
            else if (a.Length != b.Length)
                return false;
            else
            {
                for (int i = 0; i < a.Length; i++)
                    if (a[i] != b[i])
                        return false;
                return true;
            }
        }

        public static void Swap(Array array, int indexOne, int indexTwo)
        {
            int length = array.Length;
            if (length <= indexOne || length <= indexTwo || 0 > indexOne || 0 > indexTwo)
                throw new IndexOutOfRangeException();

            object tmp = array.GetValue(indexOne);
            array.SetValue(array.GetValue(indexTwo), indexOne);
            array.SetValue(tmp, indexTwo);
        }

        /// <summary>
        /// Searches an array for an item using an external comparer to determine if the items are equal.
        /// </summary>
        /// <param name="array">the array to search for the matching item.</param>
        /// <param name="searchState">parameter passed as state to the ArrayItemSearchHitComparer delegate</param>
        /// <param name="hitTester">the delegate for testing the whether the current index is a hit.</param>
        /// <returns></returns>
        public static int SearchForIndexOf<T, K>(K[] array, T searchState, ArraySearchHitTester<T, K> hitTester)
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (hitTester(searchState, array[i]))
                    return i;
            }
            return -1;
        }

        public delegate bool ArraySearchHitTester<L, M>(L searchState, M arrayItem);

        /// <summary>
        /// Stable sort, in-place, not too slow if the list is already mostly sorted
        /// </summary>
        public static void InsertionSort<T>(List<T> list, Comparison<T> comparison)
        {
            for (int i = 1; i < list.Count; i++)
            {
                T item = list[i];
                int j = i - 1;

                // TODO: Use binary search instead of linear probe
                while (j >= 0 && comparison(list[j], item) > 0)
                    --j;

                if (j + 1 != i)
                {
                    list.RemoveAt(i);
                    list.Insert(j + 1, item);
                }
            }
        }

        public static TElement[] Concat<TElement>(TElement[] a, TElement[] b)
        {
            TElement[] result = new TElement[a.Length + b.Length];
            Array.Copy(a, result, a.Length);
            Array.Copy(b, 0, result, a.Length, b.Length);
            return result;
        }

        /// <summary>
        /// Determines whether any element of an array satisfies a condition.
        /// </summary>
        /// <param name="array">The array to test.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <returns>true if any element of the array satisfies the condition, and false otherwise.</returns>
        public static bool Any<TElement>(TElement[] array, Predicate<TElement> predicate)
        {
            foreach (TElement element in array)
            {
                if (predicate(element))
                {
                    return true;
                }
            }

            return false;
        }
    }

}
