﻿using System;
using System.Collections.Generic;

namespace Stemming
{
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///
    ///
    ///
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    public static class TrieExtension
    {
        public static Trie<TValue> ToTrie<TValue>(this IEnumerable<String> src, Func<String, int, TValue> selector)
        {
            Trie<TValue> t = new Trie<TValue>();
            int idx = 0;
            foreach (String s in src)
                t.Add(s, selector(s, idx++));
            return t;
        }

        public static Trie<TValue> ToTrie<TValue>(this Dictionary<String, TValue> src)
        {
            Trie<TValue> t = new Trie<TValue>();
            foreach (var kvp in src)
                t.Add(kvp.Key, kvp.Value);
            return t;
        }

        public static IEnumerable<TValue> AllSubstringValues<TValue>(this String s, Trie<TValue> trie)
        {
            return trie.AllSubstringValues(s);
        }

        public static void AddToValueHashset<TKey, TValue>(this Dictionary<TKey, HashSet<TValue>> d, TKey k, TValue v)
        {
            HashSet<TValue> hs;
            if (d.TryGetValue(k, out hs))
                hs.Add(v);
            else
                d.Add(k, new HashSet<TValue> { v });
        }
    }
}
