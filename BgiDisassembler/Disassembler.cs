using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Arc.Ddsi.BgiDisassembler
{
    internal partial class Disassembler
    {
        private static readonly Encoding Encoding = Encoding.GetEncoding(932);

        private static readonly string[] OperandWidths =
            {
                "byte",             // 00
                "word",             // 01
                "dword"             // 02
            };

        private static readonly string[] JumpTypes =
            {
                "jnz",              // 00
                "jz",               // 01
                "jg",               // 02
                "jge",              // 03
                "jle",              // 04
                "jl"                // 05
            };

        private readonly Func<bool>[] _opcodeHandlers = new Func<bool>[256];
        private readonly Action[] _operandHandlers = new Action[256];
        private readonly BinaryReader _reader;

        private readonly Queue<int> _remainingOffsets = new Queue<int>();
        private readonly HashSet<int> _functionOffsets = new HashSet<int>();
        private readonly BasicBlockList _blocks = new BasicBlockList();

        private BasicBlock _currentBlock;
        private readonly StringBuilder _currentInstr = new StringBuilder();

        public Disassembler(Stream input)
        {
            _opcodeHandlers[0x04] = ProcessGetLocalVariableAddress;
            _opcodeHandlers[0x05] = ProcessGetString;
            _opcodeHandlers[0x06] = ProcessJump;
            _opcodeHandlers[0x17] = ProcessReturn;

            _operandHandlers[0x00] = ProcessByteOperand;
            _operandHandlers[0x01] = ProcessShortOperand;
            _operandHandlers[0x02] = ProcessIntOperand;
            _operandHandlers[0x08] = ProcessWidthOperand;
            _operandHandlers[0x09] = ProcessWidthOperand;
            _operandHandlers[0x0A] = ProcessWidthOperand;
            _operandHandlers[0x0B] = ProcessArrayOperand;
            _operandHandlers[0x0C] = () => { ProcessWidthOperand(); _currentInstr.Append(" "); ProcessByteOperand(); };
            _operandHandlers[0x80] = () => ProcessSyscallOperand(SystemCalls80);
            _operandHandlers[0x81] = () => ProcessSyscallOperand(null);
            _operandHandlers[0x90] = () => ProcessSyscallOperand(null);
            _operandHandlers[0x91] = () => ProcessSyscallOperand(null);
            _operandHandlers[0x92] = () => ProcessSyscallOperand(null);
            _operandHandlers[0xA0] = () => ProcessSyscallOperand(null);
            _operandHandlers[0xB0] = () => ProcessSyscallOperand(null);
            _operandHandlers[0xC0] = () => ProcessSyscallOperand(null);
            _operandHandlers[0xFF] = ProcessUserScriptOperand;

            _reader = new BinaryReader(input);
        }

        public void Disassemble(Stream stream)
        {
            CreateBasicBlocks();
            WriteBasicBlocks(stream);
            _blocks.Clear();
        }

        private void CreateBasicBlocks()
        {
            _blocks.Clear();
            _remainingOffsets.Enqueue(0x10);
            while (_remainingOffsets.Count > 0)
            {
                int offset = _remainingOffsets.Dequeue();
                _currentBlock = _blocks.Create(offset);
                if (_currentBlock.Length > 0)
                    continue;

                int endOffset = _blocks.GetBlockContainingOrAfter(offset + 1)?.Offset ?? (int)_reader.BaseStream.Length;
                _reader.BaseStream.Position = offset;
                while (_reader.BaseStream.Position < endOffset && ProcessInstruction())
                    ;
            }
            _currentBlock = null;
        }

        private void WriteBasicBlocks(Stream stream)
        {
            using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8, 1024, true))
            {
                foreach (BasicBlock block in _blocks)
                {
                    if (_functionOffsets.Contains(block.Offset))
                        writer.WriteLine($"sub_{block.Offset:X08}:");

                    foreach (KeyValuePair<int, string> instr in block.Instructions)
                    {
                        writer.WriteLine($"{instr.Key:X08}    {instr.Value}");
                    }
                    writer.WriteLine();
                }
            }
        }

        private bool ProcessInstruction()
        {
            int offset = (int)_reader.BaseStream.Position;
            byte opcode = _reader.ReadByte();
            Func<bool> opcodeHandler = _opcodeHandlers[opcode];
            bool result;

            if (opcodeHandler != null)
            {
                result = opcodeHandler();
            }
            else
            {
                string mnemonic = Mnemonics[opcode];
                if (mnemonic == null)
                    throw new InvalidDataException($"Unknown opcode 0x{opcode:X02} encountered at offset 0x{offset:X08}");

                _currentInstr.Append(mnemonic);

                Action operandHandler = _operandHandlers[opcode];
                if (operandHandler != null)
                {
                    _currentInstr.Append(" ");
                    operandHandler();
                }
                result = true;
            }

            int length = (int)_reader.BaseStream.Position - offset;
            _currentBlock.AddInstruction(_currentInstr.ToString(), length);
            _currentInstr.Clear();
            return result;
        }

        private bool ProcessGetLocalVariableAddress()
        {
            int offset = _reader.ReadUInt16();
            _currentInstr.Append($"lea var{offset:X}");
            return true;
        }

        private bool ProcessGetString()
        {
            int pc = (int)_reader.BaseStream.Position - 1;
            int offset = _reader.ReadInt16();
            int target = pc + offset;

            _reader.BaseStream.Position = target;
            List<byte> chars = new List<byte>();
            byte c;
            while ((c = _reader.ReadByte()) != 0)
            {
                chars.Add(c);
            }
            string str = Encoding.GetString(chars.ToArray());
            _currentInstr.Append($"getstring \"{str.Replace("\"", "\\\"")}\"      // {target:X} - {target + chars.Count:X}");

            _reader.BaseStream.Position = pc + 3;
            return true;
        }

        private bool ProcessJump()
        {
            int pc = (int)_reader.BaseStream.Position - 1;
            int offset = _reader.ReadInt16();
            int target = pc + offset;

            byte jumpOpcode = _reader.ReadByte();
            switch (jumpOpcode)
            {
                case 0x14:
                    _currentInstr.Append($"jmp {target:X08}");
                    _remainingOffsets.Enqueue(target);
                    return false;

                case 0x15:
                    byte jumpType = _reader.ReadByte();
                    if (jumpType >= JumpTypes.Length)
                        throw new InvalidDataException($"Invalid jump type 0x{jumpType:X02} at offset 0x{_reader.BaseStream.Position - 1:X08}");

                    _currentInstr.Append($"{JumpTypes[jumpType]} {target:X08}");
                    _remainingOffsets.Enqueue((int)_reader.BaseStream.Position);
                    _remainingOffsets.Enqueue(target);
                    return false;

                case 0x16:
                    _currentInstr.Append($"call sub_{target:X08}");
                    _remainingOffsets.Enqueue(target);
                    _functionOffsets.Add(target);
                    return true;

                default:
                    _reader.BaseStream.Position--;
                    _currentInstr.Append($"push sub_{target:X08}");
                    _remainingOffsets.Enqueue(target);
                    _functionOffsets.Add(target);
                    return true;
            }
        }

        private bool ProcessReturn()
        {
            _currentInstr.Append("ret");
            return false;
        }

        private void ProcessByteOperand()
        {
            byte value = _reader.ReadByte();
            _currentInstr.Append(value.ToString("X"));
        }

        private void ProcessShortOperand()
        {
            ushort value = _reader.ReadUInt16();
            _currentInstr.Append(value.ToString("X"));
        }

        private void ProcessIntOperand()
        {
            uint value = _reader.ReadUInt32();
            _currentInstr.Append(value.ToString("X"));
        }

        private void ProcessWidthOperand()
        {
            byte width = _reader.ReadByte();
            if (width >= OperandWidths.Length)
                throw new InvalidDataException($"Invalid width specifier 0x{width:X02} at offset 0x{_reader.BaseStream.Position - 1:X08}");

            _currentInstr.Append(OperandWidths[width]);
        }

        private void ProcessArrayOperand()
        {
            byte length = _reader.ReadByte();
            byte[] data = _reader.ReadBytes(length);
            foreach (byte b in data)
            {
                _currentInstr.Append(b.ToString("X02"));
            }
        }

        private void ProcessSyscallOperand(string[] serviceTable)
        {
            _reader.BaseStream.Position--;
            byte opcode = _reader.ReadByte();
            byte serviceId = _reader.ReadByte();
            if (serviceTable == null || serviceId >= serviceTable.Length || serviceTable[serviceId] == null)
                _currentInstr.Append($"{opcode:X02}:{serviceId:X02}");
            else
                _currentInstr.Append(serviceTable[serviceId]);
        }

        private void ProcessUserScriptOperand()
        {
            byte scriptId = _reader.ReadByte();
            switch (scriptId)
            {
                case 0xF0:
                    _currentInstr.Append("load");
                    break;

                case 0xF1:
                    _currentInstr.Append("free");
                    break;

                case 0xF8:
                    _currentInstr.Append("ret");
                    break;

                default:
                    _currentInstr.Append($"call {scriptId:X02}");
                    break;
            }
        }
    }
}
