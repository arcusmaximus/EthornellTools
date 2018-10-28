using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Arc.Ddsi.BgiDisassembler
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                PrintUsage();
                return;
            }

            string path = args[0];
            if (File.Exists(path))
                DisassembleFile(path);
            else if (Directory.Exists(path))
                DisassembleFolder(path);
            else
                Console.WriteLine("Specified file/folder does not exist.");
        }

        private static void DisassembleFolder(string folderPath)
        {
            foreach (string filePath in Directory.EnumerateFiles(folderPath, "*._bp"))
            {
                DisassembleFile(filePath);
            }
        }

        private static void DisassembleFile(string inputFilePath)
        {
            string outputFilePath = Path.ChangeExtension(inputFilePath, ".txt");
            using (Stream inputStream = File.OpenRead(inputFilePath))
            using (Stream outputStream = File.Open(outputFilePath, FileMode.Create, FileAccess.Write))
            {
                new Disassembler(inputStream).Disassemble(outputStream);
            }

            //AssignOpcodeHandlerNames(outputFilePath);
        }

        private static void AssignOpcodeHandlerNames(string filePath)
        {
            string content = File.ReadAllText(filePath);
            foreach (Match match in Regex.Matches(content, @"\w{8}    push 14FE0\r\n\w{8}    push (\w+)\r\n\w{8}    push 2\r\n\w{8}    shl\r\n\w{8}    add\r\n\w{8}    push (sub_\w{8})"))
            {
                string opcode = match.Groups[1].Value;
                string funcName = match.Groups[2].Value;
                content = content.Replace(funcName, $"Opcode{opcode}");
            }
            File.WriteAllText(filePath, content);
        }

        private static void PrintUsage()
        {
            string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
            Console.WriteLine($"Usage: {assemblyName} folder|file._bp");
        }
    }
}
