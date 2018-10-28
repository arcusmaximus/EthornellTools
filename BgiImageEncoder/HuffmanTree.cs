using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Arc.Ddsi.BgiImageEncoder
{
    internal class HuffmanTree
    {
        public HuffmanTree(IEnumerable<byte> input)
        {
            Node root = BuildTree(input);
            Codes = new Code[256];
            AddCodes(root, 0, 0, 0);
        }

        public Code[] Codes
        {
            get;
        }

        public void Encode(IEnumerable<byte> input, Stream output)
        {
            BitWriter writer = new BitWriter(output);
            foreach (int value in input)
            {
                ref Code code = ref Codes[value];
                writer.Write(code.Value, code.NumBits);
            }
            writer.Flush();
        }

        private void AddCodes(Node node, int depth, int currentCode, int currentCodeLength)
        {
            if (node.IsLeaf)
            {
                ref Code code = ref Codes[node.Value];
                code.Value = currentCode;
                code.NumBits = currentCodeLength;
                code.Depth = depth;
                code.Weight = node.Weight;
            }
            else
            {
                AddCodes(node.LeftChild, depth + 1, currentCode << 1, currentCodeLength + 1);
                AddCodes(node.RightChild, depth + 1, (currentCode << 1) | 1, currentCodeLength + 1);
            }
        }

        private static Node BuildTree(IEnumerable<byte> data)
        {
            Node[] leafNodes = new Node[256];
            for (int value = 0; value < 256; value++)
            {
                leafNodes[value] = new Node(value);
            }

            foreach (int value in data)
            {
                leafNodes[value].Weight++;
            }

            SortedSet<Node> sortedNodes = new SortedSet<Node>(leafNodes.Where(n => n.Weight > 0));
            while (sortedNodes.Count > 1)
            {
                Node node1 = sortedNodes.First();
                sortedNodes.Remove(node1);

                Node node2 = sortedNodes.First();
                sortedNodes.Remove(node2);

                Node parent = new Node(node1, node2);
                sortedNodes.Add(parent);
            }

            return sortedNodes.First();
        }

        private class Node : IComparable<Node>
        {
            private static int _nextParentValue = 256;

            public Node(int value)
            {
                IsLeaf = true;
                Value = value;
            }

            public Node(Node leftChild, Node rightChild)
            {
                LeftChild = leftChild;
                RightChild = rightChild;
                Value = _nextParentValue++;
                Weight = leftChild.Weight + rightChild.Weight;
            }

            public readonly bool IsLeaf;
            public readonly int Value;
            public int Weight;
            public readonly Node LeftChild;
            public readonly Node RightChild;

            public int CompareTo(Node other)
            {
                int comparison = Weight - other.Weight;
                if (comparison == 0)
                    comparison = Value - other.Value;

                return comparison;
            }

            public override string ToString()
            {
                return $"{(IsLeaf ? "Leaf" : "Parent")}, value {Value}, weight {Weight}";
            }
        }

        public struct Code
        {
            public int Value;
            public int NumBits;
            public int Depth;
            public int Weight;

            public override string ToString()
            {
                return Value.ToString();
            }
        }
    }
}
