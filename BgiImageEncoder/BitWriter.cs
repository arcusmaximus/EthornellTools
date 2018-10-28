using System;
using System.IO;

namespace Arc.Ddsi.BgiImageEncoder
{
    internal class BitWriter
    {
        private readonly Stream _stream;
        private readonly byte[] _bitBuffer;
        private int _numFreeBits = 8;

        public BitWriter(Stream stream)
        {
            _stream = stream;
            _bitBuffer = new byte[1];
        }

        public void Write(int value, int numBits)
        {
            while (numBits > 0)
            {
                int numBitsInChunk = Math.Min(_numFreeBits, numBits);
                int mask = (1 << numBitsInChunk) - 1;

                numBits -= numBitsInChunk;
                _numFreeBits -= numBitsInChunk;

                _bitBuffer[0] |= (byte)(((value >> numBits) & mask) << _numFreeBits);

                if (_numFreeBits == 0)
                    Flush();
            }
        }

        public void Flush()
        {
            if (_numFreeBits == 8)
                return;

            _stream.Write(_bitBuffer, 0, 1);
            _bitBuffer[0] = 0;
            _numFreeBits = 8;
        }
    }
}
