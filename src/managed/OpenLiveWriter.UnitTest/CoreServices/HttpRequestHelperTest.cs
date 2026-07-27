// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.UnitTest.CoreServices
{
    [TestFixture]
    public class HttpRequestHelperTest
    {
        /// <summary>
        /// Tests that HttpClient is lazily initialized and reusable.
        /// </summary>
        [Test]
        public void TestHttpClientIsReusable()
        {
            // Act
            var client1 = HttpRequestHelper.HttpClient;
            var client2 = HttpRequestHelper.HttpClient;

            // Assert
            Assert.IsNotNull(client1);
            Assert.AreSame(client1, client2, "HttpClient should be a singleton");
        }

        /// <summary>
        /// Tests that HttpClient has proper default headers.
        /// </summary>
        [Test]
        public void TestHttpClientHasDefaultHeaders()
        {
            // Act
            var client = HttpRequestHelper.HttpClient;

            // Assert
            Assert.IsTrue(client.DefaultRequestHeaders.UserAgent.Count > 0, "Should have User-Agent header");
            Assert.IsTrue(client.DefaultRequestHeaders.AcceptLanguage.Count > 0, "Should have Accept-Language header");
        }

        /// <summary>
        /// Tests CheckUrlReachable returns false for invalid URLs.
        /// </summary>
        [Test]
        public void TestCheckUrlReachableInvalidUrl()
        {
            // Act
            bool result = HttpRequestHelper.CheckUrlReachable("http://this-domain-does-not-exist-12345.com/", 1000);

            // Assert
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Tests SendRequestAsync with GET method.
        /// </summary>
        [Test]
        [Category("Network")]
        [Explicit("Requires a live external service (httpbin.org), which can be unreachable or return 503")]
        public void TestSendRequestAsyncGet()
        {
            // This test requires network access to a known working URL
            // Using a reliable public endpoint
            try
            {
                // Act
                using var response = HttpRequestHelper.SendRequestAsync("https://httpbin.org/get");

                // Assert
                Assert.IsNotNull(response);
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            }
            catch (HttpRequestException)
            {
                // Network not available - skip test
                Assert.Ignore("Network not available for integration test");
            }
        }

        /// <summary>
        /// Tests SendRequestAsync with POST method and content.
        /// </summary>
        [Test]
        [Category("Network")]
        [Explicit("Requires a live external service (httpbin.org), which can be unreachable or return 503")]
        public void TestSendRequestAsyncPost()
        {
            try
            {
                // Arrange
                var content = new StringContent("test=value", Encoding.UTF8, "application/x-www-form-urlencoded");

                // Act
                using var response = HttpRequestHelper.SendRequestAsync(HttpMethod.Post, "https://httpbin.org/post", content);

                // Assert
                Assert.IsNotNull(response);
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            }
            catch (HttpRequestException)
            {
                Assert.Ignore("Network not available for integration test");
            }
        }

        /// <summary>
        /// Tests DownloadStream returns null for invalid URL.
        /// </summary>
        [Test]
        public void TestDownloadStreamInvalidUrl()
        {
            // Act
            var stream = HttpRequestHelper.DownloadStream("http://this-domain-does-not-exist-12345.com/file.txt", out string responseUri);

            // Assert
            Assert.IsNull(stream);
        }

        /// <summary>
        /// Tests GetResponse throws for non-existent URL.
        /// </summary>
        [Test]
        public void TestGetResponseThrowsForBadUrl()
        {
            // Act & Assert
            Assert.Throws<HttpRequestException>(() =>
            {
                HttpRequestHelper.GetResponse("http://this-domain-does-not-exist-12345.com/");
            });
        }

        /// <summary>
        /// Tests GetResponseStream with valid URL.
        /// </summary>
        [Test]
        [Category("Network")]
        [Explicit("Requires a live external service (httpbin.org), which can be unreachable or return 503")]
        public void TestGetResponseStreamValid()
        {
            try
            {
                // Act
                using var stream = HttpRequestHelper.GetResponseStream("https://httpbin.org/html", out Uri responseUri);

                // Assert
                Assert.IsNotNull(stream);
                Assert.IsNotNull(responseUri);
                using var reader = new StreamReader(stream);
                string content = reader.ReadToEnd();
                Assert.IsTrue(content.Contains("<html"), "Should contain HTML content");
            }
            catch (HttpRequestException)
            {
                Assert.Ignore("Network not available for integration test");
            }
        }

        /// <summary>
        /// Tests PostForm with valid data.
        /// </summary>
        [Test]
        [Category("Network")]
        [Explicit("Requires a live external service (httpbin.org), which can be unreachable or return 503")]
        public void TestPostFormValid()
        {
            try
            {
                // Arrange
                byte[] formData = Encoding.UTF8.GetBytes("field1=value1&field2=value2");

                // Act
                using var response = HttpRequestHelper.PostForm("https://httpbin.org/post", formData);

                // Assert
                Assert.IsNotNull(response);
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            }
            catch (HttpRequestException)
            {
                Assert.Ignore("Network not available for integration test");
            }
        }

        /// <summary>
        /// Tests PostFormStream returns stream and URI.
        /// </summary>
        [Test]
        [Category("Network")]
        [Explicit("Requires a live external service (httpbin.org), which can be unreachable or return 503")]
        public void TestPostFormStreamValid()
        {
            try
            {
                // Arrange
                byte[] formData = Encoding.UTF8.GetBytes("test=data");

                // Act
                using var stream = HttpRequestHelper.PostFormStream("https://httpbin.org/post", formData, out Uri responseUri);

                // Assert
                Assert.IsNotNull(stream);
                Assert.IsNotNull(responseUri);
                using var reader = new StreamReader(stream);
                string content = reader.ReadToEnd();
                Assert.IsTrue(content.Contains("test"), "Response should echo back form data");
            }
            catch (HttpRequestException)
            {
                Assert.Ignore("Network not available for integration test");
            }
        }

        /// <summary>
        /// Tests PostForm throws for bad URL.
        /// </summary>
        [Test]
        public void TestPostFormThrowsForBadUrl()
        {
            // Arrange
            byte[] formData = Encoding.UTF8.GetBytes("test=data");

            // Act & Assert
            Assert.Throws<HttpRequestException>(() =>
            {
                HttpRequestHelper.PostForm("http://this-domain-does-not-exist-12345.com/", formData);
            });
        }
    }
}
