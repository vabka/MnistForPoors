using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using MnistReader;
using Xunit;

namespace LoaderTests
{
    public class Test
    {
        [Fact]
        public async Task FullPipeline()
        {
            using var loader = new Downloader(new HttpClient());
            var unpacker = new Unpacker();
            var labelsParser = new LabelsParser();
            var stream = await loader.Download(Constants.TrainingLabelsLink);
            var data = unpacker.Unpack(stream);
            await foreach (var label in labelsParser.ParseLabels(data))
            {
                Debug.WriteLine(label);
            }
        }

        [Fact]
        public async Task FullPipelineForImages()
        {
            using var loader = new Downloader(new HttpClient());
            var unpacker = new Unpacker();
            var labelsParser = new ImageParser();
            var stream = await loader.Download(Constants.TrainingImagesLink);
            var data = unpacker.Unpack(stream);
            await foreach (var image in labelsParser.ParseImages(data))
            {
                Debug.WriteLine(FormatToAsciiArt(image));
            }
        }

        [Fact]
        public async Task FullPipelineWithLabelsImagesAndBitmap()
        {
            using var loader = new Downloader(new HttpClient());
            var unpacker = new Unpacker();
            var labelsStream = loader.Download(Constants.TrainingLabelsLink)
                .ContinueWith(x => new LabelsParser().ParseLabels(unpacker.Unpack(x.Result)));
            var imagesStream = loader.Download(Constants.TrainingImagesLink)
                .ContinueWith(x => new ImageParser().ParseImages(unpacker.Unpack(x.Result)));
            await Task.WhenAll(labelsStream, imagesStream);
            var i = DateTime.Now.Ticks;
            Directory.CreateDirectory("dataset");
            await foreach (var (label, image) in (await labelsStream).Zip(await imagesStream))
            {
                var bitmap = image.ToBitmap();
                bitmap.Save($"./dataset/{label}_{i++}.png", ImageFormat.Png);
            }
        }

        static string FormatToAsciiArt(MyBitmap bitmap)
        {
            var sb = new StringBuilder();
            for (var r = 0; r < bitmap.Rows; r++)
            {
                for (var c = 0; c < bitmap.Columns; c++)
                {
                    var pixel = bitmap[r, c];
                    sb.Append(pixel switch
                    {
                        0 => ' ',
                        >0 and <200 => '░',
                        _ => '█',
                    });
                }

                sb.Append('\n');
            }

            return sb.ToString();
        }
    }

    public static class MyBitmapExtensions
    {
        public static Bitmap ToBitmap(this MyBitmap image)
        {
            var bitmap = new Bitmap(image.Rows, image.Columns);
            for (var r = 0; r < image.Rows; r++)
            {
                for (var c = 0; c < image.Columns; c++)
                {
                    var pixel = image[r, c];
                    var color = Color.FromArgb(255, 255 - pixel, 255 - pixel, 255 - pixel);
                    bitmap.SetPixel(c, r, color);
                }
            }

            return bitmap;
        }
    }
}