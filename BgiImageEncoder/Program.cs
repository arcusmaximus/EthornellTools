using System;
using System.IO;

namespace Arc.Ddsi.BgiImageEncoder
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                if (args.Length == 1)
                {
                    string inputFilePath = args[0];
                    if (!File.Exists(inputFilePath))
                    {
                        Console.WriteLine("Specified input file not found.");
                        return;
                    }

                    string outputFilePath = Path.Combine(Path.GetDirectoryName(inputFilePath), Path.GetFileNameWithoutExtension(inputFilePath));
                    CbgEncoder.EncodeFile(inputFilePath, outputFilePath);
                }
                else if (args.Length == 2)
                {
                    string inputFolderPath = args[0];
                    string outputFolderPath = args[1];
                    if (!Directory.Exists(inputFolderPath))
                    {
                        Console.WriteLine("Specified input folder not found.");
                        return;
                    }
                    if (!Directory.Exists(outputFolderPath))
                    {
                        Console.WriteLine("Specified output folder not found.");
                        return;
                    }
                    CbgEncoder.EncodeFolder(inputFolderPath, outputFolderPath);
                }
                else
                {
                    PrintUsage();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void PrintUsage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("    BgiImageEncoder file.png");
            Console.WriteLine(@"    BgiImageEncoder path\to\png\folder path\to\bgi\folder");
        }
    }
}
