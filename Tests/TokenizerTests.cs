using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Meka;
using System.IO;

namespace Tests
{
    [TestClass]
    public class TokenizerTests
    {
        [TestMethod]
        public void StringsTest()
        {
            string toTest = "\"hello world\\t\\n 0 1-2%5139ud\\tlk1238-1hdakjn\\t\\n\\r\"";
            string expected = "<String \"hello world\\t\\n 0 1-2%5139ud\\tlk1238-1hdakjn\\t\\n\\r\">";

            Tokenizer t = new Tokenizer(toTest);
            Stream str = t.Parse();
            Assert.AreEqual(expected, new StreamReader(str).ReadToEnd());
        }

        [TestMethod]
        public void CommentTest()
        {
            string toTest = "using lol from loli;\\\\\"hello world\\t\\n 0 1-2%5139ud\\tlk1238-1hdakjn\\t\\n\\r\"\n";
            string expected = "<Import lol:loli><Comment \\*\\\"hello world\\t\\n 0 1-2%5139ud\\tlk1238-1hdakjn\\t\\n\\r\"*\\>";

            Tokenizer t = new Tokenizer(toTest);
            Stream str = t.Parse();
            Assert.AreEqual(expected, new StreamReader(str).ReadToEnd());
        }

        [TestMethod]
        public void ImportTest()
        {
            string toTest = "using lol from loli;\nusing    all from    Meka;";
            string expected = "<Import lol:loli><Import all:Meka>";

            Tokenizer t = new Tokenizer(toTest);
            Stream str = t.Parse();
            Assert.AreEqual(expected, new StreamReader(str).ReadToEnd());
        }
    }
}
