using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace WeatherForecastv2.Tests.TestHelpers
{
    public class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;

        public MockHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
        {
            _responder = responder ?? throw new ArgumentNullException(nameof(responder));
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = _responder(request);
            return Task.FromResult(response);
        }

        public static HttpClient CreateClient(string jsonResponse, HttpStatusCode status = HttpStatusCode.OK)
        {
            var handler = new MockHttpMessageHandler(_ =>
                new HttpResponseMessage(status)
                {
                    Content = new StringContent(jsonResponse ?? string.Empty, System.Text.Encoding.UTF8, "application/json")
                });

            return new HttpClient(handler);
        }

        public static HttpClient CreateClient(Func<HttpRequestMessage, HttpResponseMessage> responder)
        {
            var handler = new MockHttpMessageHandler(responder);
            return new HttpClient(handler);
        }
    }
}
