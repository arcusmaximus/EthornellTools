namespace Arc.Ddsi.BgiImageEncoder
{
    internal struct KeyGenerator
    {
        private uint _key;

        public byte Next()
        {
            uint v0 = 20021 * (_key & 0xffff);
            uint v1 = _key >> 16;
            v1 = v1 * 20021 + _key * 346;
            v1 = (v1 + (v0 >> 16)) & 0xffff;
            _key = (v1 << 16) + (v0 & 0xffff) + 1;
            return (byte)v1;
        }
    }
}
