using System;
using System.Collections.Generic;
using System.Linq;

namespace Stemming
{
    public partial class Trie<TValue> : System.Collections.IEnumerable, IEnumerable<TrieNodeBase<TValue>>
    {

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// 
        /// Trie proper begins here
        ///
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private TrieNodeBase<TValue> _root = new TrieNode<TValue>();
        public int _nodes = 0;
        public static int _sparseNodes = 0;

        // in combination with Add(...), enables C# 3.0 initialization syntax, even though it never seems to call it
        public System.Collections.IEnumerator GetEnumerator()
        {
            return _root.SubsumedNodes().GetEnumerator();
        }

        IEnumerator<TrieNodeBase<TValue>> IEnumerable<TrieNodeBase<TValue>>.GetEnumerator()
        {
            return _root.SubsumedNodes().GetEnumerator();
        }

        public IEnumerable<TValue> Values { get { return _root.SubsumedValues(); } }

        public void OptimizeSparseNodes()
        {
            if (_root.ShouldOptimize)
            {
                _root = new SparseTrieNode<TValue>(_root.CharNodePairs());
                _sparseNodes++;
            }
            _root.OptimizeChildNodes();
        }

        public TrieNodeBase<TValue> Root { get { return _root; } }

        public TrieNodeBase<TValue> Add(string s, TValue v)
        {
            TrieNodeBase<TValue> node = _root;
            foreach (char c in s)
                node = node.AddChild(c, ref _nodes);

            node.Value = v;
            node.Key = s;
            return node;
        }

        public bool Contains(string s)
        {
            TrieNodeBase<TValue> node = _root;
            foreach (char c in s)
            {
                node = node[c];
                if (node == null)
                    return false;
            }
            return node.HasValue;
        }

        /// <summary>
        /// Debug only; this is hideously inefficient
        /// </summary>
        public string GetKey(TrieNodeBase<TValue> seek)
        {
            string sofar = string.Empty;

            GetKeyHelper fn = null;
            fn = (TrieNodeBase<TValue> cur) =>
            {
                sofar += " ";   // placeholder
                foreach (var kvp in cur.CharNodePairs())
                {
                    //Util.SetStringChar(ref sofar, sofar.Length - 1, kvp.Key);
                    if (kvp.Value == seek)
                        return true;
                    if (kvp.Value.Nodes != null && fn(kvp.Value))
                        return true;
                }
                sofar = sofar.Substring(0, sofar.Length - 1);
                return false;
            };

            if (fn(_root))
                return sofar;
            return null;
        }


        /// <summary>
        /// Debug only; this is hideously inefficient
        /// </summary>
        delegate bool GetKeyHelper(TrieNodeBase<TValue> cur);
        public string GetKey(TValue seek)
        {
            string sofar = string.Empty;

            GetKeyHelper fn = null;
            fn = (TrieNodeBase<TValue> cur) =>
            {
                sofar += " ";   // placeholder
                foreach (var kvp in cur.CharNodePairs())
                {
                    //Util.SetStringChar(ref sofar, sofar.Length - 1, kvp.Key);
                    if (kvp.Value.Value != null && kvp.Value.Value.Equals(seek))
                        return true;
                    if (kvp.Value.Nodes != null && fn(kvp.Value))
                        return true;
                }
                sofar = sofar.Substring(0, sofar.Length - 1);
                return false;
            };

            if (fn(_root))
                return sofar;
            return null;
        }

        public TrieNodeBase<TValue> FindNode(string s_in)
        {
            TrieNodeBase<TValue> node = _root;
            foreach (char c in s_in)
                if ((node = node[c]) == null)
                    return null;
            return node;
        }

        public TValue GetKey(string s_in)
        {
            TrieNodeBase<TValue> node = FindNode(s_in);
            if (node == null || !node.HasValue)
                return default(TValue);
            return node.Value;
        }

        public bool IsEmpty()
        {
            return _root.ChildCount == 0;
        }

        /// <summary>
        /// If continuation from the terminal node is possible with a different input string, then that node is not
        /// returned as a 'last' node for the given input. In other words, 'last' nodes must be leaf nodes, where
        /// continuation possibility is truly unknown. The presense of a nodes array that we couldn't match to 
        /// means the search fails; it is not the design of the 'OrLast' feature to provide 'closest' or 'best'
        /// matching but rather to enable truncated tails still in the context of exact prefix matching.
        /// </summary>
        public TrieNodeBase<TValue> FindNodeOrLast(string s_in, out bool f_exact)
        {
            TrieNodeBase<TValue> node = _root;
            foreach (char c in s_in)
            {
                if (node.IsLeaf)
                {
                    f_exact = false;
                    return node;
                }
                if ((node = node[c]) == null)
                {
                    f_exact = false;
                    return null;
                }
            }
            f_exact = true;
            return node;
        }

        // even though I found some articles that attest that using a foreach enumerator with arrays (and Lists)
        // returns a value type, thus avoiding spurious garbage, I had already changed the code to not use enumerator.
        /*public unsafe TValue Find(String s_in)
		{
			TrieNodeBase node = _root;
			fixed (Char* pin_s = s_in)
			{
				Char* p = pin_s;
				Char* p_end = p + s_in.Length;
				while (p < p_end)
				{
					if ((node = node[*p]) == null)
						return default(TValue);
					p++;
				}
				return node.Value;
			}
		}

		public unsafe TValue Find(Char* p_tag, int cb_ctag)
		{
			TrieNodeBase node = _root;
			Char* p_end = p_tag + cb_ctag;
			while (p_tag < p_end)
			{
				if ((node = node[*p_tag]) == null)
					return default(TValue);
				p_tag++;
			}
			return node.Value;
		}
        */
        public IEnumerable<TValue> FindAll(string s_in)
        {
            TrieNodeBase<TValue> node = _root;
            foreach (char c in s_in)
            {
                if ((node = node[c]) == null)
                    break;
                if (node.Value != null)
                    yield return node.Value;
            }
        }

        public IEnumerable<TValue> SubsumedValues(string s)
        {
            var node = FindNode(s);
            return node == null ? Enumerable.Empty<TValue>() : node.SubsumedValues();
        }

        public IEnumerable<TrieNodeBase<TValue>> SubsumedNodes(string s)
        {
            var node = FindNode(s);
            return node == null ? Enumerable.Empty<TrieNodeBase<TValue>>() : node.SubsumedNodes();
        }

        public IEnumerable<TValue> GetAllValues(IEnumerable<string> words)
        {
            return words.Select(FindNode).Where(node => node != null).Select(node => node.Value);
        }

        public IEnumerable<TValue> GetAllValuesWithDef(IEnumerable<string> words, Func<string, TValue> def)
        {
            return words.Select(z => (FindNode(z) == null || FindNode(z).Value == null) ? def(z) : FindNode(z).Value);
        }

        public IEnumerable<TValue> AllSubstringValues(string s)
        {
            var i_cur = 0;
            while (i_cur < s.Length)
            {
                TrieNodeBase<TValue> node = _root;
                int i = i_cur;
                while (i < s.Length)
                {
                    node = node[s[i]];
                    if (node == null)
                        break;
                    if (node.Value != null)
                        yield return node.Value;
                    i++;
                }
                i_cur++;
            }
        }

        /// <summary>
        /// note: only returns nodes with non-null values
        /// </summary>
        public void DepthFirstTraverse(Action<string, TrieNodeBase<TValue>> callback)
        {
            char[] rgch = new char[100];
            int depth = 0;

            Action<TrieNodeBase<TValue>> fn = null;
            fn = (TrieNodeBase<TValue> cur) =>
            {
                if (depth >= rgch.Length)
                {
                    char[] tmp = new char[rgch.Length * 2];
                    Buffer.BlockCopy(rgch, 0, tmp, 0, rgch.Length * sizeof(char));
                    rgch = tmp;
                }
                foreach (var kvp in cur.CharNodePairs())
                {
                    rgch[depth] = kvp.Key;
                    TrieNodeBase<TValue> n = kvp.Value;
                    if (n.Nodes != null)
                    {
                        depth++;
                        fn(n);
                        depth--;
                    }
                    else if (n.Value == null)       // leaf nodes should always have a value
                        throw new Exception();

                    if (n.Value != null)
                        callback(new string(rgch, 0, depth + 1), n);
                }
            };

            fn(_root);
        }


        /// <summary>
        /// note: only returns nodes with non-null values
        /// </summary>
        public void EnumerateLeafPaths(Action<string, IEnumerable<TrieNodeBase<TValue>>> callback)
        {
            Stack<TrieNodeBase<TValue>> stk = new Stack<TrieNodeBase<TValue>>();
            char[] rgch = new char[100];

            Action<TrieNodeBase<TValue>> fn = null;
            fn = (TrieNodeBase<TValue> cur) =>
            {
                if (stk.Count >= rgch.Length)
                {
                    char[] tmp = new char[rgch.Length * 2];
                    Buffer.BlockCopy(rgch, 0, tmp, 0, rgch.Length * sizeof(char));
                    rgch = tmp;
                }
                foreach (var kvp in cur.CharNodePairs())
                {
                    rgch[stk.Count] = kvp.Key;
                    TrieNodeBase<TValue> n = kvp.Value;
                    stk.Push(n);
                    if (n.Nodes != null)
                        fn(n);
                    else
                    {
                        if (n.Value == null)        // leaf nodes should always have a value
                            throw new Exception();
                        callback(new string(rgch, 0, stk.Count), stk);
                    }
                    stk.Pop();
                }
            };

            fn(_root);
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///
        /// Convert a trie with one value type to another
        ///
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public Trie<TNew> ToTrie<TNew>(Func<TValue, TNew> value_converter)
        {
            Trie<TNew> t = new Trie<TNew>();
            DepthFirstTraverse((s, n) =>
            {
                t.Add(s, value_converter(n.Value));
            });
            return t;
        }
    };
}
