using System.Collections.Generic;
using System.Linq;

namespace Stemming
{
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///
    /// Sparse Trie Node
    ///
    /// currently, this one's "nodes" value is never null, because we leave leaf nodes as the non-sparse type,
    /// (with nodes==null) and they currently never get converted back. Consequently, IsLeaf should always be 'false'.
    /// However, we're gonna do the check anyway.
    /// 
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    public class SparseTrieNode<TValue> : TrieNodeBase<TValue>
    {
        Dictionary<char, TrieNodeBase<TValue>> d;

        public SparseTrieNode(IEnumerable<KeyValuePair<char, TrieNodeBase<TValue>>> ie)
        {
            d = new Dictionary<char, TrieNodeBase<TValue>>();
            foreach (var kvp in ie)
                d.Add(kvp.Key, kvp.Value);
        }

        public override TrieNodeBase<TValue> this[char c]
        {
            get
            {
                TrieNodeBase<TValue> node;
                return d.TryGetValue(c, out node) ? node : null;
            }
        }

        public override TrieNodeBase<TValue>[] Nodes { get { return d.Values.ToArray(); } }

        /// <summary>
        /// do not use in current form. This means, run OptimizeSparseNodes *after* any pruning
        /// </summary>
        public override void SetLeaf() { d = null; }

        public override int ChildCount { get { return d.Count; } }

        public override KeyValuePair<char, TrieNodeBase<TValue>>[] CharNodePairs()
        {
            return d.ToArray();
        }

        public override TrieNodeBase<TValue> AddChild(char c, ref int node_count)
        {
            TrieNodeBase<TValue> node;
            if (!d.TryGetValue(c, out node))
            {
                node = new TrieNode<TValue>();
                node_count++;
                d.Add(c, node);
            }
            return node;
        }

        public override void ReplaceChild(char c, TrieNodeBase<TValue> n)
        {
            d[c] = n;
        }

        public override bool ShouldOptimize { get { return false; } }
        public override bool IsLeaf { get { return d == null; } }

    }
}
