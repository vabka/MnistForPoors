using System;

namespace MnistReader
{
    public readonly struct MyBitmap
    {
        public int Rows { get; }
        public int Columns { get; }

        public byte this[int row, int column] => GetPixel(row, column);

        private byte GetPixel(int row, int column)
        {
            if (row >= Rows)
                throw new ArgumentOutOfRangeException(nameof(row));
            if (column >= Columns)
                throw new ArgumentOutOfRangeException(nameof(row));
            return _rawData.Span[_rawData.Length / Rows * row + column];
        }

        public byte[] RawDataArray() => _rawData.ToArray();
        private readonly Memory<byte> _rawData;

        public MyBitmap(int rows, int columns, Memory<byte> rawData)
        {
            Rows = rows;
            Columns = columns;
            _rawData = rawData;
        }
    }
}