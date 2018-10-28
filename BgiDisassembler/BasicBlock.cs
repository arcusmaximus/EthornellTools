using System;
using System.Collections.Generic;

namespace Arc.Ddsi.BgiDisassembler
{
    internal class BasicBlock
    {
        private readonly SortedList<int, string> _instructions = new SortedList<int, string>();

        public BasicBlock(int offset)
        {
            Offset = offset;
        }

        public int Offset
        {
            get;
        }

        public int Length
        {
            get;
            private set;
        }

        public void AddInstruction(string instr, int length)
        {
            _instructions.Add(Offset + Length, instr);
            Length += length;
        }

        public IEnumerable<KeyValuePair<int, string>> Instructions
        {
            get { return _instructions; }
        }

        public bool Contains(int offset)
        {
            return offset >= Offset && offset < Offset + Length;
        }

        public BasicBlock Split(int offset)
        {
            int splitIndex = _instructions.IndexOfKey(offset);
            if (splitIndex <= 0)
                throw new ArgumentException("Invalid splitting offset");

            BasicBlock newBlock = new BasicBlock(offset) { Length = Offset + Length - offset };
            for (int i = splitIndex; i < _instructions.Count; i++)
            {
                newBlock._instructions.Add(_instructions.Keys[i], _instructions.Values[i]);
            }

            Length = offset - Offset;
            for (int i = _instructions.Count - 1; i >= splitIndex; i--)
            {
                _instructions.RemoveAt(i);
            }

            return newBlock;
        }
    }
}
