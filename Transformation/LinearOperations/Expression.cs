using Language;
using Language.Tokens;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Transformation
{
    /// <summary>
    /// Represents a double-linked list of expression blocks
    /// </summary>
    public class Expression : BlockChain
    {
        private Expression() { }

        #region Public methods

        /// <summary>
        /// Removes redundant parentheses from the expression
        /// </summary>
        /// <returns>True if the expression was changed</returns>
        public bool OptimizeParentheses()
        {
            bool changed = false;

            /* Remove parentheses around a single operand */
            for (var block = Start; block != End.Prev; )
            {
                if (block.Value == "(" && block.Next.Next.Value == ")")
                {
                    RemoveBlock(block);
                    RemoveBlock(block.Next.Next);

                    changed = true;
                    block = block.Prev.Next;
                }
                else
                    block = block.Next;
            }

            Stack<Tuple<Block, bool>> openingBrackets = new Stack<Tuple<Block, bool>>();
            List<Block> toDelete = new List<Block>();

            /* Find multiple parentheses around expression parts */
            foreach (var block in this)
            {
                switch (block.Value)
                {
                    case "(":
                        openingBrackets.Push(new Tuple<Block, bool>(block, block != Start && block.Prev.Value == "("));
                        break;

                    case ")":
                        Tuple<Block, bool> pases = openingBrackets.Pop();

                        /* Mark the parentheses for deletion */
                        if (pases.Item2 && (block != End && block.Next.Value == ")"))
                        {
                            toDelete.Add(pases.Item1);
                            toDelete.Add(block);
                        }
                        break;
                }
            }

            /* Remove duplicate parentheses found */
            foreach (var bracket in toDelete)
                RemoveBlock(bracket);

            ///* Remove leading and trailing parentheses in the expression */
            //while (Start.Value == "(" && End.Value == ")")
            //{
            //    RemoveBlock(Start);
            //    RemoveBlock(End);
            //    changed = true;
            //}

            return changed || (toDelete.Count != 0);
        }

        /// <summary>
        /// Returns the inner- and the left-most parentheses block
        /// </summary>
        /// <returns></returns>
        public Parentheses InnerMostParentheses()
        {
            Block opening = null;
            Block closing = null;

            foreach (var block in this)
            {
                /* Save the index of the last opening bracket */
                if (block.Value == "(")
                {
                    opening = block;
                }

                /* Save the index of the first closing bracket
                 * if the parentheses is not in the ignore list */
                else if (block.Value == ")")
                {
                    closing = block;
                    return new Parentheses(this, opening, closing);
                }
            }
            return null;
        }

        public void RemoveParentheses(Parentheses pases)
        {
            Contract.Requires(pases.Expression == this);

            RemoveBlock(pases.Start);
            RemoveBlock(pases.End);          
        }

        #endregion

        public Expression Clone()
        {
            return null;
        }

        public static Parentheses ExtractParentheses(Block opening)
        {
            int counter = 0;

            for (Block current = opening; current != null; current = current.Next)
            {
                if (current.Value == "(")
                {
                    counter++;
                }
                else if (current.Value == ")")
                {
                    if (--counter == 0)
                        return new Parentheses(opening.Expression, opening, current);
                }
            }
            return null;
        }

        public static Expression FromTokens(IList<Token> tokens)
        {
            var ops = Utils.TokensToOps(tokens);
            var result = new Expression();

            Block prev = result.Start = Block.New(result, ops[0]);
            for (int k = 1; k < ops.Count; k++)
            {
                prev = Block.New(result, ops[k], prev);
            }

            result.End = prev;
            return result;
        }
    }
}
