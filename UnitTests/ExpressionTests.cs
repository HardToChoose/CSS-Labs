using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Transformation;

namespace UnitTests
{
    [TestClass]
    public class ExpressionTests
    {
        [TestMethod]
        public void OptimizeParenthesesTest()
        {
            var test = "((a + (((-3) * w)) - var))";
            var tokens = TreeUtilTests.GetTokens(test);
            var expected = "a + (-3 * w) - var";

            Expression expr = Expression.FromTokens(tokens);
            expr.OptimizeParentheses();

            Assert.IsTrue(expr.ToString() == expected);
        }
    }
}
