using System.IO;
using System.IO.Compression;

namespace MnistReader
{
    public class Unpacker
    {
        public Stream Unpack(Stream package) =>
            new GZipStream(package, CompressionMode.Decompress);
    }
}