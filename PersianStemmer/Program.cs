using PersianStemmer.Stemming;
using Stemming.Persian;
using System;

namespace PersianStemmer
{
    class Program
    {
        static void Main(string[] args)
        {
            var dataManager = new DataManager();
            var ps = new Stemmer(dataManager.LoadRules(), dataManager.LoadLexicon(), dataManager.LoadMokassarDic(), dataManager.LoadVerbDic());
            //Console.WriteLine(ps.run("زیباست"));
            Console.WriteLine(ps.Run("پدران"));

            Console.ReadKey();
        }
    }
}
