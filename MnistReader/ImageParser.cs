using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MnistReader
{
    public class ImageParser
    {
        public async IAsyncEnumerable<MyBitmap> ParseImages(Stream data)
        {
            var buffer = await FillMemory(data, 16);
            var magicNumberBuf = buffer.Slice(0, 4);
            var countBuf = buffer.Slice(4, 4);
            var numberOfRowsBuf = buffer.Slice(8, 4);
            var numberOfColumnsBuf = buffer.Slice(12, 4);
            var magicNumber = BinaryPrimitives.ReadInt32BigEndian(magicNumberBuf.Span);
            if (magicNumber != 2051)
                throw new InvalidOperationException();
            var numberOfRows = BinaryPrimitives.ReadInt32BigEndian(numberOfRowsBuf.Span);
            var numberOfColumns = BinaryPrimitives.ReadInt32BigEndian(numberOfColumnsBuf.Span);
            var count = BinaryPrimitives.ReadInt32BigEndian(countBuf.Span);

            var imageSize = numberOfRows * numberOfColumns;
            var dataBuffer = new Memory<byte>(new byte[count * imageSize]);
            for (var i = 0; i < count; i++)
            {
                var buf = dataBuffer.Slice(i * imageSize, imageSize);
                await FillMemory(data, buf);
                yield return new MyBitmap(numberOfRows, numberOfColumns, buf);
            }

        }

        private static async Task<Memory<byte>> FillMemory(Stream data, int count)
        {
            var buffer = new Memory<byte>(new byte[count]);
            var dataBuf = buffer;
            do
            {
                var read = await data.ReadAsync(buffer);
                buffer = buffer.Slice(read);
            } while (buffer.Length > 0);

            return dataBuf;
        }

        private static async Task FillMemory(Stream data, Memory<byte> memory)
        {
            while (memory.Length > 0)
            {
                var read = await data.ReadAsync(memory);
                memory = memory.Slice(read);
                
            }
        }
    }
}