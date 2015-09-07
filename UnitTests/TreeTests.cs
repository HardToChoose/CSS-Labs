using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Parallel.Tree;

namespace UnitTests
{
    [TestClass]
    public class TreeTests
    {
        //[TestMethod]
        //public void FromTokensTest()
        //{
        //    var test = "2 / a + (-5 - 1.5) - bc3";
        //    var expected = new Arc[]
        //    {
        //        new Arc("/", "2"),
        //        new Arc("/", "a"),
        //        new Arc("+", "/"),
        //        new Arc("+", "-"),
        //        new Arc("-", "-5"),
        //        new Arc("-", "1.5"),
        //        new Arc("-", "+"),
        //        new Arc("-", "bc3")
        //    };

        //    var tokens = TreeUtilTests.GetTokens(test);
        //    Tree tree = Tree.FromTokens(tokens);
        //    var arcs = tree.Arcs;

        //    Assert.IsTrue(arcs.Except(expected).Count() == 0);
        //}
    }
}
