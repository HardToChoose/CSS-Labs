using Transformation;

using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class CommutativityTests
    {


        [TestMethod]
        public void SimplePermutationTest()
        {
            var test = "x * y - 3 + (-1 / 4 - b * a + 8) + a + (2 - 4) * 6";
            var tokens = TreeUtilTests.GetTokens(test);
            var expected = "(2 - 4) * 6 + (-1 / 4 - b * a + 8) + a + x * y - 3";

            Commutativity tool = new Commutativity(tokens);
            var result = tool.GetEquivalentForms().Last().Expression;

            Assert.IsTrue(result == expected);
        }

        [TestMethod]
        public void DuplicatePermutationTest()
        {
            var test = "x * y + (a + b - c) + x * y";
            var tokens = TreeUtilTests.GetTokens(test);

            Commutativity tool = new Commutativity(tokens);
            var result = tool.GetEquivalentForms().Count();

            Assert.IsTrue(result == 2);
        }

        [TestMethod]
        public void Test_1()
        {
            var test = "(a * b + c * d) + (1 / 2 - 3)";
            var tokens = TreeUtilTests.GetTokens(test);

            Commutativity tool = new Commutativity(tokens);
            var result = tool.GetEquivalentForms().Count();

            Assert.IsTrue(result == 2);
        }
    }
}
