using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ShaheLauncher.Tests
{
    [TestClass]
    [TestCategory("Common")]
    public sealed class UpdateHttpClientContractTests
    {
        private const string TypeName = "ShaheLauncher.Core.Updates.UpdateHttpClient";

        [TestMethod]
        public void ReturnsAllowedErrorBodyOnlyForApprovedUrl()
        {
            Type type = ReflectionTestHelpers.RequireType(TypeName);
            using (var allowed = (IDisposable)Activator.CreateInstance(
                type,
                new StubHandler(HttpStatusCode.NotFound, "allowed")))
            {
                byte[] data = (byte[])ReflectionTestHelpers.InvokeInstance(
                    allowed,
                    "DownloadBytes",
                    new Uri("https://lqh.viyf.net/update.json"),
                    8,
                    true);
                Assert.AreEqual("allowed", Encoding.UTF8.GetString(data));
            }

            using (var disallowed = (IDisposable)Activator.CreateInstance(
                type,
                new StubHandler(HttpStatusCode.NotFound, "blocked")))
            {
                Exception error = ReflectionTestHelpers.CaptureInvocationException(
                    () => ReflectionTestHelpers.InvokeInstance(
                        disallowed,
                        "DownloadBytes",
                        new Uri("https://example.com/update.json"),
                        8,
                        true));
                Assert.IsInstanceOfType(error, typeof(HttpRequestException));
            }
        }

        private sealed class StubHandler : HttpMessageHandler
        {
            private readonly HttpStatusCode _statusCode;
            private readonly string _body;

            public StubHandler(HttpStatusCode statusCode, string body)
            {
                _statusCode = statusCode;
                _body = body;
            }

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                return Task.FromResult(
                    new HttpResponseMessage(_statusCode)
                    {
                        Content = new StringContent(_body, Encoding.UTF8),
                        RequestMessage = request
                    });
            }
        }
    }
}
