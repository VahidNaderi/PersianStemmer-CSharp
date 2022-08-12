using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;


namespace Stemming.Persian
{
    public class Stemmer
    {
        //private static readonly ILog log = LogManager.GetLogger(typeof(Stemmer));

        private readonly Trie<int> _lexicon;
        private readonly Trie<string> _mokassarDic;
        private readonly Trie<Verb> _verbDic;
        private readonly List<Rule> _ruleList;
        private static readonly Trie<string> _cache = new Trie<string>();

        private static readonly string[] _verbAffixes = { "*ش", "*نده", "*ا", "*ار", "وا*", "اثر*", "فرو*", "پیش*", "گرو*", "*ه", "*گار", "*ن" };
        private static readonly string[] _suffixes = { "كار", "ناك", "وار", "آسا", "آگین", "بار", "بان", "دان", "زار", "سار", "سان", "لاخ", "مند", "دار", "مرد", "کننده", "گرا", "نما", "متر" };
        private static readonly string[] _prefixes = { "بی", "با", "پیش", "غیر", "فرو", "هم", "نا", "یک" };
        private static readonly string[] _prefixException = { "غیر" };
        private static readonly string[] _suffixZamir = { "م", "ت", "ش" };
        private static readonly string[] suffixException = { "ها", "تر", "ترین", "ام", "ات", "اش" };

        private static readonly int _patternCount = 1;
        private static readonly bool _enableCache = true;
        private static readonly bool _enableVerb = true;

        public Stemmer(List<Rule> rules = null, Trie<int> lexicons = null, Trie<string> mokassarDic = null, Trie<Verb> verbDic = null)
        {
            _ruleList = rules ?? new List<Rule>();
            _lexicon = lexicons ?? new Trie<int>();
            _mokassarDic = mokassarDic ?? new Trie<string>();
            if (_enableVerb)
                _verbDic = verbDic;
            if (_verbDic == null) _verbDic = new Trie<Verb>();

        }

        private string Normalize(string s)
        {
            StringBuilder newString = new StringBuilder();
            for (int i = 0; i < s.Length; i++)
            {
                switch (s[i])
                {
                    case 'ي':
                        newString.Append('ی');
                        break;
                    //case 'ة':
                    case 'ۀ':
                        newString.Append('ه');
                        break;
                    case '‌':
                        newString.Append(' ');
                        break;
                    case '‏':
                        newString.Append(' ');
                        break;
                    case 'ك':
                        newString.Append('ک');
                        break;
                    case 'ؤ':
                        newString.Append('و');
                        break;
                    case 'إ':
                    case 'أ':
                        newString.Append('ا');
                        break;
                    case '\u064B': //FATHATAN
                    case '\u064C': //DAMMATAN
                    case '\u064D': //KASRATAN
                    case '\u064E': //FATHA
                    case '\u064F': //DAMMA
                    case '\u0650': //KASRA
                    case '\u0651': //SHADDA
                    case '\u0652': //SUKUN
                        break;
                    default:
                        newString.Append(s[i]);
                        break;
                }
            }
            return newString.ToString();

        }

        private bool Validate(string sWord)
        {
            return _lexicon.Contains(sWord);
        }

        private string IsMokassar(string input, bool state)
        {
            string rule = "^(?<stem>.+?)((?<=(ا|و))ی)?(ها)?(ی)?((ات)?( تان|تان| مان|مان| شان|شان)|ی|م|ت|ش|ء)$";
            if (state)
                rule = "^(?<stem>.+?)((?<=(ا|و))ی)?(ها)?(ی)?(ات|ی|م|ت|ش| تان|تان| مان|مان| شان|شان|ء)$";

            return ExtractStem(input, rule);
        }

        private string GetMokassarStem(string word)
        {
            string temp = _mokassarDic.ContainsKey(word);
            if (string.IsNullOrEmpty(temp))
            {
                string newWord = IsMokassar(word, true);
                temp = _mokassarDic.ContainsKey(newWord);
                if (string.IsNullOrEmpty(temp))
                {
                    newWord = IsMokassar(word, false);
                    temp = _mokassarDic.ContainsKey(newWord);
                    if (!string.IsNullOrEmpty(temp))
                        return temp;
                }
                else
                {
                    return temp;
                }
            }
            else
            {
                return temp;
            }

            return "";
        }

        private string ValidateVerb(string word)
        {
            if (word.IndexOf(' ') > -1)
                return "";

            for (int j = 0; j < _verbAffixes.Length; j++)
            {
                string temp = "";
                if (j == 0 && (word[word.Length - 1] == 'ا' || word[word.Length - 1] == 'و'))
                {
                    temp = _verbAffixes[j].Replace("*", word + "ی");
                }
                else
                {
                    temp = _verbAffixes[j].Replace("*", word);
                }

                if (NormalizeValidation(temp, true))
                    return _verbAffixes[j];
            }

            return "";
        }

        private bool IsInRange(int d, int from, int to)
        {
            return (d >= from && d <= to);
        }

        private string GetPrefix(string word)
        {
            foreach (string sPrefix in _prefixes)
            {
                if (word.StartsWith(sPrefix))
                    return sPrefix;
            }

            return "";
        }

        private string GetPrefixException(string word)
        {
            foreach (string sPrefix in Stemmer._prefixException)
            {
                if (word.StartsWith(sPrefix))
                    return sPrefix;
            }

            return "";
        }

        private string GetSuffix(string word)
        {
            foreach (string sSuffix in _suffixes)
            {
                if (word.EndsWith(sSuffix))
                    return sSuffix;
            }

            return "";
        }

        private bool NormalizeValidation(string word, bool removeSpace)
        {
            int l = word.Trim().Length - 2;
            word = word.Trim();
            bool result = Validate(word);

            if (!result && word.IndexOf('ا') == 0)
            {
                result = Validate(ReplaceFirst(word, "ا", "آ"));
            }

            if (!result && IsInRange(word.IndexOf('ا'), 1, l))
            {
                result = Validate(word.Replace('ا', 'أ'));
            }

            if (!result && IsInRange(word.IndexOf('ا'), 1, l))
            {
                result = Validate(word.Replace('ا', 'إ'));
            }

            if (!result && IsInRange(word.IndexOf("ئو"), 1, l))
            {
                result = Validate(word.Replace("ئو", "ؤ"));
            }

            if (!result && word.EndsWith("ء"))
                result = Validate(word.Replace("ء", ""));

            if (!result && IsInRange(word.IndexOf("ئ"), 1, l))
                result = Validate(word.Replace("ئ", "ی"));

            if (removeSpace)
            {
                if (!result && IsInRange(word.IndexOf(' '), 1, l))
                {
                    result = Validate(word.Replace(" ", ""));
                }
            }
            // دیندار
            // دین دار
            if (!result)
            {
                string suffix = GetSuffix(word);
                if (!string.IsNullOrEmpty(suffix))
                    result = Validate(suffix == ("مند") ? word.Replace(suffix, "ه " + suffix) : word.Replace(suffix, " " + suffix));
            }

            if (!result)
            {
                string sPrefix = GetPrefix(word);
                if (!string.IsNullOrEmpty(sPrefix))
                {
                    if (word.StartsWith(sPrefix + " "))
                        result = Validate(word.Replace(sPrefix + " ", sPrefix));
                    else
                        result = Validate(word.Replace(sPrefix, sPrefix + " "));
                }
            }

            if (!result)
            {
                string prefix = GetPrefixException(word);
                if (!string.IsNullOrEmpty(prefix))
                {
                    if (word.StartsWith(prefix + " "))
                        result = Validate(ReplaceFirst(word, prefix + " ", ""));
                    else
                        result = Validate(ReplaceFirst(word, prefix, ""));
                }
            }

            return result;
        }
        public string ReplaceFirst(string word, string oldValue, string newValue)
        {
            int i = word.IndexOf(oldValue);
            if (i >= 0)
            {
                return word.Substring(0, i) + newValue + word.Substring(i + oldValue.Length);
            }
            return word;
        }

        private bool IsMatch(string input, string rule)
        {
            return Regex.IsMatch(input, rule);
        }

        private string ExtractStem(string input, string rule, string replacement)
        {
            return Regex.Replace(input, rule, replacement).Trim();
        }

        private string ExtractStem(string input, string rule)
        {
            return ExtractStem(input, rule, "${stem}");
        }

        private string GetVerb(string input)
        {
            var tmpNode = _verbDic.FindNode(input);
            if (tmpNode != null)
            {
                Verb vs = tmpNode.Value;
                if (Validate(vs.Present))
                    return vs.Present;

                return vs.Past;
            }

            return "";
        }

        private bool PatternMatching(string input, List<string> stemList)
        {
            bool terminate = false;
            string s = "";
            string sTemp = "";
            foreach (Rule rule in _ruleList)
            {
                if (terminate)
                    return terminate;

                string[] sReplace = rule.Substitution.Split(';');
                string pattern = rule.Body;

                if (!IsMatch(input, pattern))
                    continue;

                int k = 0;
                foreach (string t in sReplace)
                {
                    if (k > 0)
                        break;

                    s = ExtractStem(input, pattern, t);
                    if (s.Length < rule.MinLength)
                        continue;

                    switch (rule.PoS)
                    {
                        case 'K': // Kasre Ezafe
                            if (stemList.Count == 0)
                            {
                                sTemp = GetMokassarStem(s);
                                if (!string.IsNullOrEmpty(sTemp))
                                {
                                    stemList.Add(sTemp);//, pattern + " [جمع مکسر]");
                                    k++;
                                }
                                else if (NormalizeValidation(s, true))
                                {
                                    stemList.Add(s);//, pattern);
                                    k++;
                                }
                                else
                                {
                                    //addToLog("", pattern + " : {" + s + "}");
                                }
                            }
                            break;
                        case 'V': // Verb

                            sTemp = ValidateVerb(s);
                            if (!string.IsNullOrEmpty(sTemp))
                            {
                                stemList.Add(s/* pattern + " : [" + sTemp + "]"*/);
                                k++;
                            }
                            else
                            {
                                //addToLog("", pattern + " : {تمام وندها}");
                            }
                            break;
                        default:
                            if (NormalizeValidation(s, true))
                            {
                                stemList.Add(s/*, pattern*/);
                                if (rule.State)
                                    terminate = true;
                                k++;
                            }
                            else
                            {
                                //addToLog("", pattern + " : {" + s + "}");
                            }
                            break;
                    }
                }
            }
            return terminate;
        }

        public string Run(string input)
        {
            input = Normalize(input).Trim();

            if (string.IsNullOrEmpty(input))
                return "";

            //Integer or english 
            if (Utils.IsEnglish(input) || Utils.IsNumber(input) || (input.Length <= 2))
                return input;

            if (_enableCache)
            {
                var stm = _cache.ContainsKey(input);
                if (!string.IsNullOrEmpty(stm))
                    return stm;
            }

            string s = GetMokassarStem(input);
            if (NormalizeValidation(input, false))
            {
                //stemList.add(input/*, "[فرهنگ لغت]"*/);
                if (_enableCache)
                    _cache.Add(input, input);
                return input;
            }
            else if (!string.IsNullOrEmpty(s))
            {
                //addToLog(s/*, "[جمع مکسر]"*/);
                //stemList.add(s);
                if (_enableCache)
                    _cache.Add(input, s);
                return s;
            }

            List<string> stemList = new List<string>();
            bool terminate = PatternMatching(input, stemList);

            if (_enableVerb)
            {
                s = GetVerb(input);
                if (!string.IsNullOrEmpty(s))
                {
                    stemList.Clear();
                    stemList.Add(s);
                }
            }

            if (stemList.Count == 0)
            {
                if (NormalizeValidation(input, true))
                {
                    //stemList.add(input, "[فرهنگ لغت]");
                    if (_enableCache)
                        _cache.Add(input, input); //stemList.get(0));
                    return input;//stemList.get(0);
                }
                stemList.Add(input);//, "");            
            }

            if (terminate && stemList.Count > 1)
            {
                return NounValidation(stemList);
            }

            const int I = 0;
            if (_patternCount != 0)
            {
                if (_patternCount < 0)
                    stemList.Reverse();
                else
                    stemList.Sort();

                while (I < stemList.Count && (stemList.Count > Math.Abs(_patternCount)))
                {
                    stemList.RemoveAt(I);
                    //patternList.remove(I);
                }
            }

            if (_enableCache)
                _cache.Add(input, stemList[0]);
            return stemList[0];
        }

        /*private void addToLog(string sStem) {
        
            if (sStem.isEmpty() || stemList.contains(sStem)) 
                return;

            stemList.add(sStem);
            //patternList.add(sRule);
        }    */

        public int Stem(char[] s, int len) /*throws Exception*/
        {

            StringBuilder input = new StringBuilder();
            for (int i = 0; i < len; i++)
            {
                input.Append(s[i]);
            }
            string sOut = this.Run(input.ToString());

            if (sOut.Length > s.Length)
                s = new char[sOut.Length];
            for (int i = 0; i < sOut.Length; i++)
            {
                s[i] = sOut[i];
            }
            /*try {
                for (int i=0; i< Math.min(sOut.length(), s.length); i++) {
                    s[i] = sOut.charAt(i);
                }    
            }
            catch (Exception e) {
                throw new Exception("stem: "+sOut+" - input: "+ input.toString());
            }*/

            return sOut.Length;

        }

        private string NounValidation(List<string> stemList)
        {
            stemList.Sort();
            int lastIdx = stemList.Count - 1;
            string lastStem = stemList[lastIdx];

            if (lastStem.EndsWith("ان"))
            {
                return lastStem;
            }
            else
            {
                string firstStem = stemList[0];
                string secondStem = stemList[1].Replace(" ", "");

                /*if (secondStem.equals(firstStem.concat("م"))) {
                    return firstStem;
                }
                else if (secondStem.equals(firstStem.concat("ت"))) {
                    return firstStem;
                }
                else if (secondStem.equals(firstStem.concat("ش"))) {
                    return firstStem;
                }*/

                foreach (string sSuffix in _suffixZamir)
                {
                    if (secondStem.Equals(firstStem + sSuffix))
                        return firstStem;
                }
            }
            return lastStem;
        }
    }
}