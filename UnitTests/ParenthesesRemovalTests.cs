using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Transformation;

namespace UnitTests
{
    [TestClass]
    public class ParenthesesRemovalTests
    {
        [TestMethod]
        public void LeftAdditionTest()
        {
            var test = "x * y + (-1 / 4 - b * a + 8)";
            var tokens = TreeUtilTests.GetTokens(test);
            var expected = "x * y - 1 / 4 - b * a + 8";

            ParenthesesRemoval tool = new ParenthesesRemoval(tokens);
            var result = tool.GetEquivalentForms().Last().Expression;

            Assert.IsTrue(result == expected);
        }

        [TestMethod]
        public void LeftSubtractionTest()
        {
            var test = "y - (-1 / 4 + b * a - 8)";
            var tokens = TreeUtilTests.GetTokens(test);
            var expected = "y + 1 / 4 - b * a + 8";

            ParenthesesRemoval tool = new ParenthesesRemoval(tokens);
            var result = tool.GetEquivalentForms().Last().Expression;

            Assert.IsTrue(result == expected);
        }

        [TestMethod]
        public void GetLeftOperandTest()
        {
            var test = "4.5 + a / b * c / e * (-1 - f)";
            var tokens = TreeUtilTests.GetTokens(test);
            var expected = "4.5 - a / b * c / e * 1 - a / b * c / e * f";

            ParenthesesRemoval tool = new ParenthesesRemoval(tokens);
            var result = tool.GetEquivalentForms().Last().Expression;

            Assert.IsTrue(result == expected);
        }

        [TestMethod]
        public void LeftMultiplicationTest()
        {
            var test = "a - y * (-1 / 4 + b * a - 8)";
            var tokens = TreeUtilTests.GetTokens(test);
            var expected = "a + y * 1 / 4 - y * b * a + y * 8";

            ParenthesesRemoval tool = new ParenthesesRemoval(tokens);
            var result = tool.GetEquivalentForms().Last().Expression;

            Assert.IsTrue(result == expected);
        }

        [TestMethod]
        public void MoveToEndTest()
        {
            var test = "-8 + a / (1 - b) * (c + d) / 6.1 - 2";
            var tokens = TreeUtilTests.GetTokens(test);
            var expected = "-8 + a * c / 6.1 / (1 - b) + a * d / 6.1 / (1 - b) - 2";

            ParenthesesRemoval tool = new ParenthesesRemoval(tokens);
            var result = tool.GetEquivalentForms().Last().Expression;

            Assert.IsTrue(result == expected);
        }

        [TestMethod]
        public void RightMultiplicationTest()
        {
            var test = "a - (-1 / 4 + b * a - 8) * -y / 3";
            var tokens = TreeUtilTests.GetTokens(test);
            var expected = "a - 1 / 4 * y / 3 + b * a * y / 3 - 8 * y / 3";

            ParenthesesRemoval tool = new ParenthesesRemoval(tokens);
            var result = tool.GetEquivalentForms().Last().Expression;

            Assert.IsTrue(result == expected);
        }

        [TestMethod]
        public void RightDivisionTest()
        {
            var test = "a - (-1 + b) / -r * 4";
            var tokens = TreeUtilTests.GetTokens(test);
            var expected = "a - 1 / r * 4 + b / r * 4";

            ParenthesesRemoval tool = new ParenthesesRemoval(tokens);
            var result = tool.GetEquivalentForms().Last().Expression;

            Assert.IsTrue(result == expected);
        }

        [TestMethod]
        public void MultiplyParenthesesTest()
        {
            var test = "a - (b - c) * (-d + 1) / 2";
            var tokens = TreeUtilTests.GetTokens(test);
            var expected = "a + b * d / 2 - b * 1 / 2 - c * d / 2 + c * 1 / 2";

            ParenthesesRemoval tool = new ParenthesesRemoval(tokens);
            var result = tool.GetEquivalentForms().Last().Expression;

            Assert.IsTrue(result == expected);
        }

        [TestMethod]
        public void DoubleParenthesesDivisionTest()
        {
            var test = "4 + a / (b - c) / (c - d) * -y - (1 - b) * (c * d)";
            var tokens = TreeUtilTests.GetTokens(test);
            var expected = "4 + a * -y / (b * c - b * d - c * c + c * d) - 1 * c * d + b * c * d";

            ParenthesesRemoval tool = new ParenthesesRemoval(tokens);
            var result = tool.GetEquivalentForms().Last().Expression;

            Assert.IsTrue(result == expected);
        }

        [TestMethod]
        public void DoubleParenthesesDivisionTest_2()
        {
            var test = "(1 + a) / (b - c) / (c - d) * x / 2";
            var tokens = TreeUtilTests.GetTokens(test);
            var expected = "1 * x / 2 / (b * c - b * d - c * c + c * d) + a * x / 2 / (b * c - b * d - c * c + c * d)";

            ParenthesesRemoval tool = new ParenthesesRemoval(tokens);
            var result = tool.GetEquivalentForms().Last().Expression;

            Assert.IsTrue(result == expected);
        }
    }
}
