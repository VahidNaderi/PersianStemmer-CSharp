using Stemming;
using Stemming.Persian;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PersianStemmer.Stemming
{
    public class DataManager
    {
        private const string PATTERN_FILE_NAME = "Patterns.fa";
        private const string VERB_FILE_NAME = "VerbList.fa";
        private const string DIC_FILE_NAME = "Dictionary.fa";
        private const string MOKASSAR_FILE_NAME = "Mokassar.fa";

        public string[] LoadData(string resourceName)
        {
            try
            {
                string _dataDirectoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", resourceName);

                return File.ReadAllLines(_dataDirectoryPath).Where(l => !string.IsNullOrWhiteSpace(l)).ToArray();
            }
            catch (FileNotFoundException ex)
            {
                System.Console.WriteLine(ex.ToString());
            }

            return null;
        }

        public Trie<Verb> LoadVerbDic()
        {
            var verbDic = new Trie<Verb>();

            string[] sLines = LoadData(VERB_FILE_NAME);
            foreach (string sLine in sLines)
            {
                string[] arr = sLine.Split('\t');
                try
                {
                    verbDic.Add(arr[0].Trim(), new Verb(arr[1].Trim(), arr[2].Trim()));
                }
                catch
                {
                    //log.Warn("Verb " + sLine + " cannot be added. Is it duplicated?");
                }
            }
            return verbDic;
        }

        public List<Rule> LoadRules()
        {
            var ruleList = new List<Rule>();

            string[] sLines = LoadData(PATTERN_FILE_NAME);
            foreach (string sLine in sLines)
            {
                string[] arr = sLine.Split(',');
                ruleList.Add(new Rule(arr[0], arr[1], arr[2][0], byte.Parse(arr[3]), bool.Parse(arr[4])));
            }
            return ruleList;
        }

        public Trie<int> LoadLexicon()
        {
            var lexicons = new Trie<int>();

            string[] sLines = LoadData(DIC_FILE_NAME);
            foreach (string sLine in sLines)
            {
                lexicons.Add(sLine.Trim(), 1);
            }
            return lexicons;
        }

        public Trie<string> LoadMokassarDic()
        {
            var mokassarDic = new Trie<string>();
            string[] sLines = LoadData(MOKASSAR_FILE_NAME);
            foreach (string sLine in sLines)
            {
                string[] arr = sLine.Split('\t');
                mokassarDic.Add(arr[0].Trim(), arr[1].Trim());
            }
            return mokassarDic;
        }
    }
}
