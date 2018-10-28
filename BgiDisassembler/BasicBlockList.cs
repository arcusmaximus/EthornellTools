using System.Collections;
using System.Collections.Generic;

namespace Arc.Ddsi.BgiDisassembler
{
    internal class BasicBlockList : IEnumerable<BasicBlock>
    {
        private readonly SortedList<int, BasicBlock> _blocks = new SortedList<int, BasicBlock>();

        public BasicBlock Create(int offset)
        {
            BasicBlock block = GetBlockContaining(offset);
            if (block == null)
            {
                block = new BasicBlock(offset);
                _blocks.Add(offset, block);
            }
            else if (offset > block.Offset)
            {
                block = block.Split(offset);
                _blocks.Add(offset, block);
            }
            return block;
        }

        public BasicBlock GetBlockContaining(int offset)
        {
            int index = GetIndexOfBlockContaining(offset);
            if (index >= _blocks.Count)
                return null;

            BasicBlock block = _blocks.Values[index];
            return block.Contains(offset) ? block : null;
        }

        public BasicBlock GetBlockContainingOrAfter(int offset)
        {
            int index = GetIndexOfBlockContaining(offset);
            return index < _blocks.Count ? _blocks.Values[index] : null;
        }

        public void Clear()
        {
            _blocks.Clear();
        }

        private int GetIndexOfBlockContaining(int offset)
        {
            int start = 0;
            int end = _blocks.Count;
            while (start < end)
            {
                int pivot = (start + end) / 2;
                BasicBlock block = _blocks.Values[pivot];
                if (offset < block.Offset)
                    end = pivot;
                else if (offset >= block.Offset + block.Length)
                    start = pivot + 1;
                else
                    return pivot;
            }
            return start;
        }

        public IEnumerator<BasicBlock> GetEnumerator()
        {
            return _blocks.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
