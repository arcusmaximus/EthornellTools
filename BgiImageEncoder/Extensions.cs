using System.IO;

namespace Arc.Ddsi.BgiImageEncoder
{
    internal static class Extensions
    {
        public static void WriteVariableLength(this BinaryWriter writer, int value)
        {
            do
            {
                byte b = (byte)(value & 0x7F);
                value >>= 7;
                if (value != 0)
                    b |= 0x80;

                writer.Write(b);
            } while (value != 0);
        }
    }
}
