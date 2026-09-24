using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using ShaheLauncher.Core.Common;

namespace ShaheLauncher.Core.Updates
{
    public sealed class UpdateHttpClient : IDisposable
    {
        private readonly HttpClient _client;

        public UpdateHttpClient(HttpMessageHandler handler)
        {
            _client = new HttpClient(handler ?? throw new ArgumentNullException(nameof(handler)), true);
        }

        public byte[] DownloadBytes(Uri uri, int timeoutSeconds, bool allowErrorBody)
        {
            return DownloadBytes(uri, timeoutSeconds, allowErrorBody, null);
        }

        public byte[] DownloadBytes(
            Uri uri,
            int timeoutSeconds,
            bool allowErrorBody,
            IDictionary<string, string> headers)
        {
            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            using (var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds)))
            using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
            {
                foreach (KeyValuePair<string, string> header in headers ?? new Dictionary<string, string>())
                {
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }

                using (HttpResponseMessage response = _client.SendAsync(request, cancellation.Token).GetAwaiter().GetResult())
                {
                    byte[] body = response.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
                    if (response.IsSuccessStatusCode
                        || allowErrorBody && CommonUtilities.IsAllowedUpdateBodyStatus((int)response.StatusCode, uri.AbsoluteUri))
                    {
                        return body;
                    }

                    response.EnsureSuccessStatusCode();
                    return body;
                }
            }
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}
