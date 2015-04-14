using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Meka;
using System.IO;

namespace Tests
{
    [TestClass]
    public class ParserTests
    {
        [TestMethod]
        public void TagExtractionTest()
        {

            string toTest = File.ReadAllText("TestCode.mek");
            Tokenizer t = new Tokenizer(toTest);
            Parser p = new Parser(t.Parse());
            var tmp = p.Parse();
        }
    }
}
