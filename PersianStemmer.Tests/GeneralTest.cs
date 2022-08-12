using PersianStemmer.Stemming;
using Stemming.Persian;
using Xunit;

namespace PersianStemmer.Tests
{
    public class GeneralTest
    {
        [Fact]
        public void Simple_Test()
        {
            var dataManager = new DataManager();
            var ps = new Stemmer(dataManager.LoadRules(), dataManager.LoadLexicon(), dataManager.LoadMokassarDic());
            Assert.Equal("پدر", ps.Run("پدران"));
        }

        [Fact]
        public void Simple_Test2()
        {
            var dataManager = new DataManager();
            var ps = new Stemmer(dataManager.LoadRules(), dataManager.LoadLexicon(), dataManager.LoadMokassarDic());
            Assert.Equal("زیبا", ps.Run("زیباست"));
        }
    }
}