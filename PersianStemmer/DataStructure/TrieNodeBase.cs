using System.Collections.Generic;

namespace Stemming
{
    public abstract class TrieNodeBase<TValue>
    {
        public TValue Value { get; set; } = default(TValue);

        public string Key { get; set; } = "";

        public bool HasValue { get { return !Equals(Value, default(TValue)); } }
        public abstract bool IsLeaf { get; }

        public abstract TrieNodeBase<TValue> this[char c] { get; }

        public abstract TrieNodeBase<TValue>[] Nodes { get; }

        public abstract void SetLeaf();

        public abstract int ChildCount { get; }

        public abstract bool ShouldOptimize { get; }

        public abstract KeyValuePair<char, TrieNodeBase<TValue>>[] CharNodePairs();

        public abstract TrieNodeBase<TValue> AddChild(char c, ref int node_count);

        /// <summary>
        /// Includes current node value
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TValue> SubsumedValues()
        {
            if (Value != null)
                yield return Value;
            if (Nodes != null)
                foreach (TrieNodeBase<TValue> child in Nodes)
                    if (child != null)
                        foreach (TValue t in child.SubsumedValues())
                            yield return t;
        }

        /// <summary>
        /// Includes current node
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TrieNodeBase<TValue>> SubsumedNodes()
        {
            yield return this;
            if (Nodes != null)
                foreach (TrieNodeBase<TValue> child in Nodes)
                    if (child != null)
                        foreach (TrieNodeBase<TValue> n in child.SubsumedNodes())
                            yield return n;
        }

        /// <summary>
        /// Doesn't include current node
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TrieNodeBase<TValue>> SubsumedNodesExceptThis()
        {
            if (Nodes != null)
                foreach (TrieNodeBase<TValue> child in Nodes)
                    if (child != null)
                        foreach (TrieNodeBase<TValue> n in child.SubsumedNodes())
                            yield return n;
        }

        /// <summary>
        /// Note: doesn't de-optimize optimized nodes if re-run later
        /// </summary>
        public void OptimizeChildNodes()
        {
            if (Nodes != null)
                foreach (var q in CharNodePairs())
                {
                    TrieNodeBase<TValue> n_old = q.Value;
                    if (n_old.ShouldOptimize)
                    {
                        TrieNodeBase<TValue> n_new = new SparseTrieNode<TValue>(n_old.CharNodePairs());
                        n_new.Value = n_old.Value;
                        Trie<TValue>._sparseNodes++;
                        ReplaceChild(q.Key, n_new);
                    }
                    n_old.OptimizeChildNodes();
                }
        }

        public abstract void ReplaceChild(char c, TrieNodeBase<TValue> n);

    }
}
