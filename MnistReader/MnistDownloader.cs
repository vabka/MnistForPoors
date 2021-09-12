using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace MnistReader
{
    public class MnistDownloader
    {
        public async IAsyncEnumerable<(byte Label, MyBitmap Image)> Download(string labels, string images)
        {
            using var loader = new Downloader(new HttpClient());
            var unpacker = new Unpacker();
            var labelsStream = loader.Download(labels)
                .ContinueWith(x => new LabelsParser().ParseLabels(unpacker.Unpack(x.Result)));
            var imagesStream = loader.Download(images)
                .ContinueWith(x => new ImageParser().ParseImages(unpacker.Unpack(x.Result)));
            await Task.WhenAll(labelsStream, imagesStream);
            await foreach (var (label, image) in (await labelsStream).Zip(await imagesStream))
                yield return (label, image);
        }
    }
}