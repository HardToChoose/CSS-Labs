using Language;
using Language.Tokens;

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace Transformation
{
    public class Commutativity : Tool
    {
        public Commutativity(IList<Token> tokens) : base(tokens) { }

        public override IEnumerable<EquivalentForm> GetEquivalentForms()
        {
            SplitAndShuffle(expression, "", "");

            if (steps.Count > 0)
            {
                steps.Last().IsMinor = false;
            }
            return steps;
        }

        #region Inner classes

        private struct SplitResult
        {
            /// <summary>
            /// The sequence of terms separated by the given operation
            /// </summary>
            public BlockChain[] Terms;
            public string Operation;

            /// <summary>
            /// The rightmost block preceding the sequence of terms
            /// </summary>
            public string LeftSide;
            /// <summary>
            /// The leftmost block succeeding the sequence of terms
            /// </summary>
            public string RightSide;
        }

        /// <summary>
        /// Represents a set of subsequent indices and their permutations
        /// </summary>
        private class Group
        {
            public int StartTermIndex;
            public int EndTermIndex;

            public List<int[]> IndexPermutations { get; private set; }

            public Group(IEnumerable<int> indices)
            {
                this.IndexPermutations = new List<int[]>();

                this.StartTermIndex = indices.First();
                this.EndTermIndex = indices.Last();
            }
        }

        #endregion

        #region Private methods

        private void SplitAndShuffle(BlockChain expr, string left, string right)
        {
            /* Split the expression into addends or multipliers */
            var terms = SplitByOperator(expr, "+");
            var operation = "+";

            if (terms.Length == 0)
            {
                terms = SplitByOperator(expr, "*");
                operation = "*";
            }

            if (terms.Length == 0)
                return;

            /* Sort the terms by complexity */
            var complexities = terms.Select(term => GetComplexity(term)).ToArray();
            Array.Sort(complexities, terms);

            /* Permute the non-equal terms having same complexity */
            SplitResult split = new SplitResult
            {
                Terms = terms,
                Operation = operation,
                LeftSide = left,
                RightSide = right
            };
            ShuffleTerms(split, complexities);

            /* Repeat the same for all addends (multipliers) recursively */
            for (int k = 0; k < terms.Length; k++)
            {
                left = (k > 0) ? Concat(terms.Take(k), operation, true) : "";
                right = (k < terms.Length - 1) ? Concat(terms.Skip(k + 1), operation, false) : "";

                SplitAndShuffle(terms[k], left, right);
            }
        }

        private string Concat(IEnumerable<BlockChain> terms, string op, bool left)
        {
            StringBuilder result = new StringBuilder();
            op = " " + op + " ";

            foreach (var term in terms)
            {
                if (!left)   result.Append(op);
                result.Append(term.ToString());
                if (left)    result.Append(op);
            }

            return result.ToString();
        }

        /// <summary>
        /// Works for commutative operations only (i.e. + and * )
        /// </summary>
        /// <param name="expr"></param>
        /// <param name="op"></param>
        /// <returns></returns>
        private BlockChain[] SplitByOperator(BlockChain expr, string op)
        {
            /* Remove trailing and leading parentheses if any */
            expr.ExcludeParentheses();

            List<BlockChain> terms = new List<BlockChain>();

            int braceCounter = 0;
            Block termStart = expr.Start;

            for (Block current = expr.Start; current != expr.End; current = current.Next)
            {
                /* Count parentheses in order to consider top level operations only */
                if (current.Value == "(")
                {
                    braceCounter++;
                }
                else if (current.Value == ")")
                {
                    braceCounter--;
                }

                /* Extract the term separated by the operators */
                else if (current.Value == op && braceCounter == 0)
                {
                    terms.Add(new BlockChain(termStart, current.Prev));
                    termStart = current.Next;
                }

                /* Top-level operator with lower precedence found -> split failed */
                else if (Utils.IsOperator(current.Value) && braceCounter == 0 &&
                         Utils.ComparePrecedence(current.Value, op) < 0)
                {
                    return new BlockChain[0];
                }
            }

            /* Extract the last term */
            if (termStart != expr.Start)
            {
                terms.Add(new BlockChain(termStart, expr.End));
            }

            expr.IncludeParentheses();
            return terms.ToArray();
        }

        private void Swap(int[] array, int first, int second)
        {
            int tmp = array[first];
            array[first] = array[second];
            array[second] = tmp;
        }

        private void Permutations(BlockChain[] terms, int[] indices, int from, List<int[]> outResult)
        {
            /* Return the final permutation */
            if (from == indices.Length - 1)
            {
                var permutation = new int[indices.Length];
                indices.CopyTo(permutation, 0);
                   
                outResult.Add(permutation);
                return;
            }

            /* Fixate the 'from' term, permute all succeeding */
            Permutations(terms, indices, from + 1, outResult);

            /* Swap the first ('from') term with each of the succeeding terms
             * permuting the sequence of terms excluding the first one recursively */
            for (int k = from + 1; k < indices.Length; k++)
            {
                /* Do not swap equal terms */
                if (!TermsEqual(terms[indices[from]], terms[indices[k]]))
                {
                    Swap(indices, from, k);
                    Permutations(terms, indices, from + 1, outResult);
                    Swap(indices, k, from);
                }
            }
        }

        private void ShuffleTerms(SplitResult split, int[] complexities)
        {
            Contract.Requires(split.Terms.Length == complexities.Length);
            List<Group> groups = new List<Group>();

            for (int k = 0; k < split.Terms.Length; )
            {
                int currentComp = complexities[k];
                List<int> sequence = new List<int>();

                /* Find the longest sequence of terms having the same complexity */
                while (k < split.Terms.Length && complexities[k] == currentComp)
                {
                    sequence.Add(k);
                    k++;
                }

                /* Permute the non-equal terms */
                if (sequence.Count > 1 && currentComp > 0)
                {
                    var group = new Group(sequence);
                    Permutations(split.Terms, sequence.ToArray(), 0, group.IndexPermutations);
                    groups.Add(group);
                }
            }

            if (groups.Count == 1 && groups.First().IndexPermutations.Count == 1)
                return;

            if (groups.Count > 0)
                PermuteGroups(split, groups);
        }

        private void PermuteGroups(SplitResult split, List<Group> groups)
        {
            var groupSizes = groups.Select(group => group.IndexPermutations.Count).ToList();
            groupSizes.Add(1);

            /* The number of combinations of groups' permutations */
            int combinations = 1;
            foreach (var size in groupSizes)
            {
                combinations *= size;
            }

            int[] row = new int[groups.Count];
            int groupID = -1;

            for (int k = 0; k < combinations; k++)
            {
                int tmp = k;

                /* Select permutation for each group basing on current combination number */
                for (int g = groups.Count - 1; g >= 0; g--)
                {
                    row[g] = tmp / groupSizes[g + 1] % groupSizes[g];
                    tmp -= row[g];
                }

                if (k % groupSizes[groups.Count - 1] == 0)
                {
                    groupID++;
                }

                string permutation = ConcatTerms(split.Terms, groups, row, split.Operation);
                LogTransformation(false, groupID, split.LeftSide + permutation + split.RightSide);
            }
        }

        private string ConcatTerms(BlockChain[] terms, List<Group> groups, int[] selection, string operation)
        {
            StringBuilder result = new StringBuilder();
            operation = " " + operation + " ";

            int[] indices = Enumerable.Range(0, terms.Length).ToArray();

            /* Replace each group of indices by the permutation of indices */
            for (int groupIndex = 0; groupIndex < groups.Count; groupIndex++)
            {
                int start = groups[groupIndex].StartTermIndex;

                for (int termIndex = start; termIndex <= groups[groupIndex].EndTermIndex; termIndex++)
                {
                    indices[termIndex] = groups[groupIndex].IndexPermutations[selection[groupIndex]][termIndex - start];
                }
            }

            foreach (var index in indices)
            {
                result.Append(terms[index].ToString());
                result.Append(operation);
            }
            return result.ToString(0, result.Length - operation.Length);
        }

        /// <summary>
        /// Returns true if two given expression parts contain the same number of
        /// addition, subtraction, multiplication and division operations
        /// </summary>
        private bool TermsEqual(BlockChain first, BlockChain second)
        {
            string firstStr = first.ToString();
            string secondStr = second.ToString();

            return (firstStr == secondStr) ||
                   ((firstStr.Count(ch => ch == '+') + firstStr.Count(ch => ch == '-') ==
                    secondStr.Count(ch => ch == '+') + secondStr.Count(ch => ch == '-')) &&
                    (firstStr.Count(ch => ch == '*') == secondStr.Count(ch => ch == '*')) &&
                    (firstStr.Count(ch => ch == '/') == secondStr.Count(ch => ch == '/')));
        }

        /// <summary>
        /// Returns the operator complexity of a given expression part
        /// </summary>
        private int GetComplexity(BlockChain expr)
        {
            /* The complexity of a single term is 0 */
            if (expr.Start == expr.End)
                return 0;

            var operators = new [] { "+", "-", "*", "/" };
            int complexity = 0;

            foreach (var op in operators)
            {
                complexity += expr.Count(op) * Utils.GetOperationComplexity(op);
            }
            return complexity;
        }

        private BlockChain LongestLeftOperand(Block operation, Block leftEnd)
        {
            Block end = operation.Prev;
            Block start = end;

            //if (end.Value == ")")
            //{
            //    while (start.Value != "(")
            //        start = start.Prev;
            //}
            //else
            //{
            //    while (start.Prev != null)
            //    {
            //        if (start.Value == "(" ||
            //            Utils.IsOperator(start.Value) &&
            //            Utils.ComparePrecedence(start.Value, operation.Value) <= 0)
            //        {
            //            start = start.Next;
            //            break;
            //        }
            //        start = start.Prev;
            //    }
            //}

            int pases = 0;
            do
            {
                if (start.Value == "(")
                    pases--;
                else if (start.Value == ")")
                    pases++;
                else if (Utils.IsOperator(start.Value))
                {

                }
            }
            while (start != leftEnd);
            return new BlockChain(start, end);
        }

        #endregion
    }
}
