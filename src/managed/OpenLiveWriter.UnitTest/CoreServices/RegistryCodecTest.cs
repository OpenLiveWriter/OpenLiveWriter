// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using NUnit.Framework;
using OpenLiveWriter.CoreServices.Settings;

namespace OpenLiveWriter.UnitTest.CoreServices
{
    [TestFixture]
    public class RegistryCodecTest
    {
        [Test]
        public void ByteArrayRoundTrip()
        {
            byte[] original = new byte[] { 1, 2, 3, 4, 250 };
            object encoded = RegistryCodec.Instance.Encode(original);
            object decoded = RegistryCodec.Instance.Decode(encoded, typeof(byte[]));

            // Regression test: JSON-backed SerializableCodec used to return a
            // JsonElement here (target type was dropped on decode), which threw
            // InvalidCastException in SettingsPersisterHelper.GetByteArray.
            Assert.IsInstanceOf<byte[]>(decoded);
            Assert.AreEqual(original, decoded);
        }

        [Test]
        public void StringRoundTrip()
        {
            const string original = "the quick brown fox";
            object encoded = RegistryCodec.Instance.Encode(original);
            object decoded = RegistryCodec.Instance.Decode(encoded, typeof(string));

            Assert.AreEqual(original, decoded);
        }
    }
}
