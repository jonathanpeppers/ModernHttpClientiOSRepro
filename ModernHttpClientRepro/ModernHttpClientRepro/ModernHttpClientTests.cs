using System;
using System.IO;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using ModernHttpClient;

namespace ModernHttpClientRepro
{
    [TestFixture]
    public class ModernHttpClientTests
    {
        [Test, Timeout(int.MaxValue)]
        public async Task CreateADeadlock()
        {
            var tasks = new List<Task>();

            var urls = new[]
            {
                "http://auctionlook-dev.azurewebsites.net/api/categories?" + Guid.NewGuid().ToString("N"),
                "https://auctionlookdev.blob.core.windows.net/photos/KY6AF6ABCD3A7643C98CBFCDA86C47C0C7-05AD0D0B2CB0412EAED0EF1D11342EA9.jpg",
                "http://auctionlook-dev.azurewebsites.net/api/categories?" + Guid.NewGuid().ToString("N"),
                "https://auctionlookdev.blob.core.windows.net/photos/KY6AF6ABCD3A7643C98CBFCDA86C47C0C7-0707DF4F1BA9490BB9AEC532663B5B76.jpg",
                "http://auctionlook-dev.azurewebsites.net/api/categories?" + Guid.NewGuid().ToString("N"),
                "https://auctionlookdev.blob.core.windows.net/photos/KY6AF6ABCD3A7643C98CBFCDA86C47C0C7-0BF3B6E95FD24336A4DB6C7CDD9CC9F1.jpg",
                "http://auctionlook-dev.azurewebsites.net/api/categories?" + Guid.NewGuid().ToString("N"),
                "https://auctionlookdev.blob.core.windows.net/photos/KY6AF6ABCD3A7643C98CBFCDA86C47C0C7-1C076106D9E244EFB2815FAC3376093E.jpg",
                "http://auctionlook-dev.azurewebsites.net/api/categories?" + Guid.NewGuid().ToString("N"),
                "https://auctionlookdev.blob.core.windows.net/photos/KY6AF6ABCD3A7643C98CBFCDA86C47C0C7-457411DCE08E471681AD9B31C2E47499.jpg",
                "http://auctionlook-dev.azurewebsites.net/api/categories?" + Guid.NewGuid().ToString("N"),
                "https://auctionlookdev.blob.core.windows.net/photos/KY6AF6ABCD3A7643C98CBFCDA86C47C0C7-6F912CF7F27E4B8BA3621F740DD893EB.jpg",
                "http://auctionlook-dev.azurewebsites.net/api/categories?" + Guid.NewGuid().ToString("N"),
                "https://auctionlookdev.blob.core.windows.net/photos/KY6AF6ABCD3A7643C98CBFCDA86C47C0C7-71A2E3B42E4842D4BBD8A2B746C33CE6.jpg",
                "http://auctionlook-dev.azurewebsites.net/api/categories?" + Guid.NewGuid().ToString("N"),
                "https://auctionlookdev.blob.core.windows.net/photos/KY6AF6ABCD3A7643C98CBFCDA86C47C0C7-7CF7CD095F9740BDBE996F4F3EC3B2BF.jpg",
                "http://auctionlook-dev.azurewebsites.net/api/categories?" + Guid.NewGuid().ToString("N"),
                "https://auctionlookdev.blob.core.windows.net/photos/KY6AF6ABCD3A7643C98CBFCDA86C47C0C7-841E69A67CE4499E9C5DE57E7C84DC60.jpg",
                "http://auctionlook-dev.azurewebsites.net/api/categories?" + Guid.NewGuid().ToString("N"),
                "https://auctionlookdev.blob.core.windows.net/photos/KYE3D8EDD9B98845CD891FA12A1D74875A-DD69D1576A174EB693B2838AB15CD8F0.jpg",
            };
            //Fire a bunch of requests in parallel
            for (int i = 0; i < urls.Length; i++)
            {
                var handler = new NativeMessageHandler();
                var client = new HttpClient(handler);
                var request = new HttpRequestMessage(HttpMethod.Get, urls[i]);
                tasks.Add(Task.Factory.StartNew(async () =>
                {
                    var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, new CancellationToken()).ConfigureAwait(false);
                    Console.WriteLine("GET: " + response.RequestMessage.RequestUri + ", " + response.StatusCode);

                    using (var memoryStream = new MemoryStream())
                    using (var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                    {
                        await stream.CopyToAsync(memoryStream).ConfigureAwait(false);
                    }

                    Console.WriteLine("READ: " + response.RequestMessage.RequestUri);

                }).Unwrap());
            }

            //await the request tasks in order
            foreach (var task in tasks)
            {
                await task;
            }
        }
    }
}

