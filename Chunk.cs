using System;

namespace AlumniStaticWeb
{
    public class Chunk
    {
        private byte[] _header;
        private byte[] _footer = System.Text.Encoding.UTF8.GetBytes("\r\n");
        private byte[] _data;
        public int Size => _data.Length;
        private Chunk() => _data = System.Text.Encoding.UTF8.GetBytes("0\r\n\r\n");
        private Chunk(byte[] data, int size)
        {
            _header = System.Text.Encoding.UTF8.GetBytes(size.ToString("x") + "\r\n");
            _data = new byte[size + _header.Length + _footer.Length];
            Buffer.BlockCopy(_header, 0, _data, 0, _header.Length);
            Buffer.BlockCopy(data, 0, _data, _header.Length, size);
            Buffer.BlockCopy(_footer, 0, _data, size + _header.Length, _footer.Length);
        }
        public static Chunk Create(byte[] data, int size) => new Chunk(data, size);
        public static Chunk CreateLast() => new Chunk();
        public static implicit operator byte[](Chunk c) => c._data;
    }
}
