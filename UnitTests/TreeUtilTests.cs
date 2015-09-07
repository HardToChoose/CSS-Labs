using Language;
using Language.Analyzers;
using Language.Tokens;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace UnitTests
{
    [TestClass]
    public class TreeUtilTests
    {
        public static IList<Token> GetTokens(string expression)
        {
            LexicalAnalyzer lex = new LexicalAnalyzer();
            lex.Feed(expression);            
            return lex.Analyze();
        }

        [TestMethod]
        public void TokensToOpsTest()
        {
            var test = "-5 + (-sin(3.4) * 8 + -1.27) / 2 - 4";
            var expected = new[] { "-5", "+", "(", "-sin(3.4)", "*", "8", "+", "-1.27", ")", "/", "2", "-", "4" };
            
            var tokens = GetTokens(test);
            var infix = Utils.TokensToOps(tokens);

            Assert.IsTrue(infix.SequenceEqual(expected));
        }

        [TestMethod]
        public void InfixToPrefixTest()
        {
            var test = "(2 * 6.1 + (5) / 3 - -4.15) + (-2 - 7) * 9";
            var expected = new[] { "2", "6.1", "*", "5", "3", "/", "+", "-4.15", "-", "-2", "7", "-", "9", "*", "+" };

            var tokens = GetTokens(test);
            var infix = Utils.TokensToOps(tokens);
            var prefix = Utils.InfixToPostfix(infix);

            Assert.IsTrue(prefix.SequenceEqual(expected));
        }
    }
}
