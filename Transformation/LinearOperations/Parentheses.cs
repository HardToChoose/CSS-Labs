using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Transformation
{
    public class Parentheses : BlockChain
    {
        public Expression Expression { get; private set; }

        public Parentheses(Expression expr, Block start, Block end)
        {
            Contract.Requires(start.Value == "(");
            Contract.Requires(end.Value == ")");

            this.Expression = expr;
            this.Start = start;
            this.End = end;
        }

        /// <summary>
        /// Concatenates the terms inside the parentheses into addends.
        /// 
        /// For example: '(', 'a', '/', 'b', '-', '4.5', '*', 'y', '+', '6'
        /// transforms into '(', 'a / b', '-', '4.5 * y', '+', '6'
        /// 
        /// Precondition: there is no inner parentheses inside the current one.
        /// </summary>
        public void GroupAddends()
        {
            Block addendStart = Start.Next;

            for (Block current = Start.Next; current != End; current = current.Next)
            {
                if (current.Value == "+" || current.Value == "-")
                {
                    Expression.Collapse(addendStart, current.Prev);
                    addendStart = current.Next;
                }
            }
            Expression.Collapse(addendStart, End.Prev);
        }

        #region Interface implementations and overriden methods

        public override IEnumerator<Block> GetEnumerator()
        {
            for (Block current = Start.Next; current != End; current = current.Next)
                yield return current;
        }

        #endregion
    }
}
