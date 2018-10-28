using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace Arc.Ddsi.BgiImageEncoder
{
    internal static class CbgEncoder
    {
        private static readonly byte[] Magic = { 0x43, 0x6F, 0x6D, 0x70, 0x72, 0x65, 0x73, 0x73, 0x65, 0x64, 0x42, 0x47, 0x5F, 0x5F, 0x5F, 0x00 };

        public static void EncodeFolder(string inputFolderPath, string outputFolderPath)
        {
            foreach (string inputFilePath in Directory.EnumerateFiles(inputFolderPath, "*.png"))
            {
                string outputFilePath = Path.Combine(outputFolderPath, Path.GetFileNameWithoutExtension(inputFilePath));
                EncodeFile(inputFilePath, outputFilePath);
            }
        }

        public static void EncodeFile(string inputFilePath, string outputFilePath)
        {
            using (Image image = Image.FromFile(inputFilePath))
            using (Stream output = File.Open(outputFilePath, FileMode.Create, FileAccess.Write))
            {
                Encode(image, output);
            }
        }

        public static void Encode(Image image, Stream stream)
        {
            ArraySegment<byte> pixelData = GetRunLengthEncodedPixelData(image);
            HuffmanTree tree = new HuffmanTree(pixelData);

            BinaryWriter writer = new BinaryWriter(stream);
            WriteHeader(image, writer);
            WriteHuffmanWeights(tree, writer);
            WriteCompressedData(pixelData, tree, writer);
        }

        private static ArraySegment<byte> GetRunLengthEncodedPixelData(Image image)
        {
            byte[] pixels = GetPixels(image);
            ApplyAverageSampling(pixels, image.Width, image.Height);

            MemoryStream stream = new MemoryStream();
            RunLengthEncode(pixels, stream);
            stream.TryGetBuffer(out ArraySegment<byte> data);
            return data;
        }

        private static byte[] GetPixels(Image image)
        {
            using (Bitmap bitmap = new Bitmap(image))
            {
                Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);
                BitmapData bitmapData = bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                byte[] pixels = new byte[bitmapData.Stride * bitmap.Height];
                Marshal.Copy(bitmapData.Scan0, pixels, 0, pixels.Length);
                bitmap.UnlockBits(bitmapData);
                return pixels;
            }
        }

        private static void ApplyAverageSampling(byte[] pixels, int width, int height)
        {
            int stride = width * 4;
            for (int y = height - 1; y >= 0; y--)
            {
                int lineOffset = y * stride;
                for (int x = width - 1; x >= 0; x--)
                {
                    int pixelOffset = lineOffset + x * 4;
                    for (int p = 3; p >= 0; p--)
                    {
                        int avg = 0;
                        if (x > 0)
                            avg += pixels[pixelOffset + p - 4];

                        if (y > 0)
                            avg += pixels[pixelOffset + p - stride];

                        if (x > 0 && y > 0)
                            avg /= 2;

                        if (avg != 0)
                            pixels[pixelOffset + p] -= (byte)avg;
                    }
                }
            }
        }

        private static void RunLengthEncode(byte[] pixels, Stream stream)
        {
            BinaryWriter writer = new BinaryWriter(stream);
            int zeroStartIndex = -1;
            int prevZeroEndIndex = 0;
            for (int i = 0; i < pixels.Length; i++)
            {
                if (pixels[i] == 0)
                {
                    if (zeroStartIndex < 0)
                        zeroStartIndex = i;
                }
                else
                {
                    if (zeroStartIndex >= 0 && i - zeroStartIndex > 4)
                    {
                        writer.WriteVariableLength(zeroStartIndex - prevZeroEndIndex);
                        stream.Write(pixels, prevZeroEndIndex, zeroStartIndex - prevZeroEndIndex);
                        writer.WriteVariableLength(i - zeroStartIndex);
                        prevZeroEndIndex = i;
                    }
                    zeroStartIndex = -1;
                }
            }

            if (zeroStartIndex >= 0 && pixels.Length - zeroStartIndex > 4)
            {
                writer.WriteVariableLength(zeroStartIndex - prevZeroEndIndex);
                stream.Write(pixels, prevZeroEndIndex, zeroStartIndex - prevZeroEndIndex);
                writer.WriteVariableLength(pixels.Length - zeroStartIndex);
            }
            else
            {
                writer.WriteVariableLength(pixels.Length - prevZeroEndIndex);
                stream.Write(pixels, prevZeroEndIndex, pixels.Length - prevZeroEndIndex);
            }
        }

        private static void WriteHeader(Image image, BinaryWriter writer)
        {
            writer.Write(Magic);

            writer.Write((short)image.Width);
            writer.Write((short)image.Height);
            writer.Write(32);       // BPP
            writer.Write(0);        // Padding
            writer.Write(0);        // Padding

            writer.Write(0);        // Size of run length-encoded data
            writer.Write(0);        // Key
            writer.Write(0);        // Size of variable-length encoded Huffman tree weights
            writer.Write((byte)0);  // Checksum of weights
            writer.Write((byte)0);  // Check XOR of weights
            writer.Write((short)1); // Version
        }

        private static void WriteHuffmanWeights(HuffmanTree tree, BinaryWriter writer)
        {
            MemoryStream plainStream = new MemoryStream();
            BinaryWriter plainWriter = new BinaryWriter(plainStream);
            for (int i = 0; i < 256; i++)
            {
                plainWriter.WriteVariableLength(tree.Codes[i].Weight);
            }

            byte[] plainWeights = plainStream.GetBuffer();
            KeyGenerator key = new KeyGenerator();
            byte sum = 0;
            byte xor = 0;
            for (int i = 0; i < plainStream.Length; i++)
            {
                byte b = plainWeights[i];
                sum += b;
                xor ^= b;
                writer.Write((byte)(b + key.Next()));
            }

            writer.BaseStream.Position = 0x28;
            writer.Write((int)plainStream.Length);
            writer.Write(sum);
            writer.Write(xor);

            writer.BaseStream.Position = writer.BaseStream.Length;
        }

        private static void WriteCompressedData(ArraySegment<byte> data, HuffmanTree tree, BinaryWriter writer)
        {
            Stream stream = writer.BaseStream;
            tree.Encode(data, stream);

            stream.Position = 0x20;
            writer.Write(data.Count);
            stream.Position = stream.Length;
        }
    }
}
