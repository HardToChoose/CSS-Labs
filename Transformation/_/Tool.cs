using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Language;
using Language.Tokens;

namespace Transformation
{
    public abstract class Tool
    {
        #region Private fields

        /// <summary>
        /// A linked list of expression parts
        /// </summary>
        protected Expression expression;

        /// <summary>
        /// The sequence of equivalent forms - from the original expression
        /// to the final parentheses removal result
        /// </summary>
        protected List<EquivalentForm> steps;

        #endregion

        public Tool(IList<Token> tokens)
        {
            steps = new List<EquivalentForm>();

            /* Transform all occurrences of a - -b into a + b */
            bool optimized = Utils.OptimizeUnaries(tokens);
            expression = Expression.FromTokens(tokens);

            if (optimized)
                LogTransformation(true);
            if (expression.OptimizeParentheses())
                LogTransformation(true);
        }

        public abstract IEnumerable<EquivalentForm> GetEquivalentForms();

        /// <summary>
        /// Adds a new transformation step (equivalent form) to the list
        /// </summary>
        /// <param name="isMinor">Determines whether the equivalent form to add is transitive
        /// (i.e. obtained by an operation other than parentheses removal).</param>
        protected virtual void LogTransformation(bool isMinor = false, int groupID = -1, string expr = "")
        {
            steps.Add(new EquivalentForm(expr, isMinor, groupID));
        }
    }
}
