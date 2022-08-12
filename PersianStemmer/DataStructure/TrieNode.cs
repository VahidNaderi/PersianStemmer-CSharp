using System;
using System.Collections.Generic;
using System.Linq;

namespace Stemming
{
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///
    /// Non-sparse Trie Node
    ///
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    public class TrieNode<TValue> : TrieNodeBase<TValue>
    {
        private TrieNodeBase<TValue>[] nodes = null;
        private char m_base;

        public override int ChildCount => (nodes != null) ? nodes.Count(e => e != null) : 0;
        public int AllocatedChildCount => (nodes != null) ? nodes.Length : 0;

        public override TrieNodeBase<TValue>[] Nodes { get { return nodes; } }

        public override void SetLeaf() { nodes = null; }

        public override KeyValuePair<char, TrieNodeBase<TValue>>[] CharNodePairs()
        {
            KeyValuePair<char, TrieNodeBase<TValue>>[] rg = new KeyValuePair<char, TrieNodeBase<TValue>>[ChildCount];
            char ch = m_base;
            int i = 0;
            foreach (TrieNodeBase<TValue> child in nodes)
            {
                if (child != null)
                    rg[i++] = new KeyValuePair<char, TrieNodeBase<TValue>>(ch, child);
                ch++;
            }
            return rg;
        }

        public override TrieNodeBase<TValue> this[char c]
        {
            get
            {
                if (nodes != null && m_base <= c && c < m_base + nodes.Length)
                    return nodes[c - m_base];
                return null;
            }
        }

        public override TrieNodeBase<TValue> AddChild(char c, ref int node_count)
        {
            if (nodes == null)
            {
                m_base = c;
                nodes = new TrieNodeBase<TValue>[1];
            }
            else if (c >= m_base + nodes.Length)
            {
                Array.Resize(ref nodes, c - m_base + 1);
            }
            else if (c < m_base)
            {
                char c_new = (char)(m_base - c);
                TrieNodeBase<TValue>[] tmp = new TrieNodeBase<TValue>[nodes.Length + c_new];
                nodes.CopyTo(tmp, c_new);
                m_base = c;
                nodes = tmp;
            }

            TrieNodeBase<TValue> node = nodes[c - m_base];
            if (node == null)
            {
                node = new TrieNode<TValue>();
                node_count++;
                nodes[c - m_base] = node;
            }
            return node;
        }

        public override void ReplaceChild(char c, TrieNodeBase<TValue> n)
        {
            if (nodes == null || c >= m_base + nodes.Length || c < m_base)
                throw new Exception();
            nodes[c - m_base] = n;
        }

        public override bool ShouldOptimize
        {
            get
            {
                if (nodes == null)
                    return false;
                return (ChildCount * 9 < nodes.Length);     // empirically determined optimal value (space & time)
            }
        }

        public override bool IsLeaf { get { return nodes == null; } }
    }
}
