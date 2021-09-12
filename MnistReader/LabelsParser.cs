using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MnistReader
{
    public class LabelsParser
    {
        public async IAsyncEnumerable<byte> ParseLabels(Stream data)
        {
            var buffer = await FillMemory(data, 8);
            var magicNumberBuf = buffer.Slice(0, 4);
            var countBuf = buffer.Slice(4, 4);
            var magicNumber = BinaryPrimitives.ReadInt32BigEndian(magicNumberBuf.Span);
            if (magicNumber != 2049)
                throw new InvalidOperationException();
            var count = BinaryPrimitives.ReadInt32BigEndian(countBuf.Span);
            var dataBuf = await FillMemory(data, count);

            for (var i = 0; i < dataBuf.Length; i++)
            {
                yield return dataBuf.Span[i];
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
    }
}