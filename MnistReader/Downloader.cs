using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace MnistReader
{
    public class Downloader : IDisposable
    {
        private readonly HttpClient httpClient;

        public Downloader(HttpClient httpClient) =>
            this.httpClient = httpClient;

        public async Task<Stream> Download(string url)
        {
            var stream = await httpClient.GetStreamAsync(url);
            return stream;
        }

        public void Dispose() => httpClient.Dispose();
    }
}