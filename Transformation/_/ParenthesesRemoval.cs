using Language;
using Language.Tokens;

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace Transformation
{
    public class ParenthesesRemoval : Tool
    {
        #region Public methods

        public ParenthesesRemoval(IList<Token> tokens) : base(tokens) { }

        /// <summary>
        /// Returns the equivalent forms of the expression passed to the constructor
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<EquivalentForm> GetEquivalentForms()
        {
            Parentheses innerMost = expression.InnerMostParentheses();
            while (innerMost != null)
            {
                ExpandParentheses(innerMost);
                innerMost = expression.InnerMostParentheses();
            }

            if (steps.Count > 0)
            {
                steps.Last().IsMinor = false;
            }
            return steps;
        }

        #endregion

        #region Private methods

        private bool BlockIsOperator(Block block)
        {
            return (block != null) && Utils.IsOperator(block.Value);
        }

        private void ExpandParentheses(Parentheses pases, bool rightDirection = false, bool isMinor = false)
        {
            /* Split the expression inside the parentheses into terms by + and - */
            pases.GroupAddends();
            Block firstTerm = pases.Start.Next;
            Block lastTerm = pases.End.Prev;

            /* Get operators preceding and succeeding the parentheses */
            Block leftOperator = pases.Start.Prev;
            Block rightOperator = pases.End.Next;

            string leftOp = BlockIsOperator(leftOperator) ? leftOperator.Value : "";
            string rightOp = BlockIsOperator(rightOperator) ? rightOperator.Value : "";

            /* Remove redundant parentheses */
            if (leftOp + rightOp == "")
            {
                expression.RemoveBlock(pases.Start);
                expression.RemoveBlock(pases.End);

                steps.Last().IsMinor = true;
                LogTransformation();
                return;
            }

            var lowPriority = new string[] { "+", "-" };

            /* No operator at the left and + or - at the right */
            if (leftOp == "" && lowPriority.Contains(rightOp))
            {
                expression.RemoveParentheses(pases);
                LogTransformation();
            }

            /* The succeeding operator has lower priority */
            else if (Utils.ComparePrecedence(leftOp, rightOp) < 0 || rightDirection)
            {
                ExpandRight(rightOperator, pases, firstTerm, lastTerm, isMinor);
            }

            /* The operator at the left has higher or same priority */
            else
            {
                ExpandLeft(leftOperator, pases, firstTerm, lastTerm, isMinor);
            }
        }

        private void ExpandLeft(Block leftOperator, Parentheses pases, Block firstTerm, Block lastTerm, bool minor)
        {
            switch (leftOperator.Value)
            {
                case "(":
                    expression.RemoveParentheses(pases);
                    LogTransformation(minor);
                    break;

                case "+":
                    LeftAddition(leftOperator, firstTerm, lastTerm);
                    expression.RemoveParentheses(pases);
                    LogTransformation(minor);
                    break;

                case "-":
                    LeftSubtraction(leftOperator, firstTerm, lastTerm);
                    expression.RemoveParentheses(pases);
                    LogTransformation(minor);
                    break;

                case "*":
                    /* Bring the multiplier at the left into the parentheses
                     * For example: 1.2 - x * (a - b)  ->  1.2 - (x * a - x * b) 
                     */
                    LeftMultiplication(leftOperator, firstTerm, lastTerm);
                    expression.RemoveBlock(leftOperator.Prev);
                    expression.RemoveBlock(leftOperator);
                    LogTransformation(minor || true);

                    /* Expand the parentheses again if possible
                     * 1.2 - (x * a - x * b)  ->  1.2 - x * a + x * b
                     */
                    if (!TryRemoveParentheses(pases, minor))
                    {
                        /* Do not mark the previous transformation as minor otherwise */
                        steps.Last().IsMinor = false;
                    }
                    break;

                case "/":
                    Block rightOperation = pases.End.Next;

                    /* Replace x / (..) / (--) with x / (.. * --) */
                    if (rightOperation != null && rightOperation.Value == "/" &&
                        rightOperation.Next != null && rightOperation.Next.Value == "(")
                    {
                        Parentheses rightOperand = Expression.ExtractParentheses(rightOperation.Next);
                        rightOperand.Start.Prev.Value = "*";

                        MultiplyParentheses(pases, rightOperand);
                        LogTransformation(minor);
                    }

                    /* Transform x / (..) * (--) into x * (--) / (..) */
                    else if (MoveToEnd(leftOperator, pases))
                    {
                        LogTransformation(minor || true);
                        expression.Collapse(pases.Start, pases.End);
                    }
                    break;
            }
        }

        private void ExpandRight(Block rightOperator, Parentheses pases, Block firstTerm, Block lastTerm, bool minor)
        {
            switch (rightOperator.Value)
            {
                case "*":
                    if (rightOperator.Next.Value == "(")
                    {
                        /* Multiply the parentheses */
                        MultiplyParentheses(pases, Expression.ExtractParentheses(rightOperator.Next));
                        LogTransformation(minor);
                    }

                    /* The operand at the right of * is a single term */
                    else
                    {
                        SimpleRightDivMul(rightOperator, pases, firstTerm, lastTerm, minor);
                    }
                    break;

                case "/":
                    if (rightOperator.Next.Value == "(")
                    {
                        /* Expand the operand at the right of the / if its another parentheses */
                        ExpandParentheses(Expression.ExtractParentheses(rightOperator.Next), isMinor: minor);
                    }

                    /* The operand at the right of / is a single term */
                    else
                    {
                        SimpleRightDivMul(rightOperator, pases, firstTerm, lastTerm, minor);
                    }
                    break;
            }
        }

        /// <summary>
        /// Returns the left operand of a given operation containing another terms
        /// and operations having the same priority or higher.
        /// </summary>
        private Block LongestLeftOperand(Block operation)
        {
            Block result = operation.Prev;
            StringBuilder operand = new StringBuilder(result.Value);

            for (Block current = result.Prev; current != null; current = result.Prev)
            {
                if (current.Value == "(" ||
                    Utils.IsOperator(current.Value) &&
                    Utils.ComparePrecedence(current.Value, operation.Value) < 0)
                {
                    break;
                }

                operand.Insert(0, ' ');
                operand.Insert(0, current.Value);

                expression.RemoveBlock(current);
            }

            result.Value = operand.ToString();
            return result;
        }

        private Block LongestRightOperand(Block operation)
        {
            Block result = operation.Next;
            StringBuilder operand = new StringBuilder(result.Value);

            for (Block current = result.Next; current != null; current = result.Next)
            {
                if (current.Value == ")" ||
                    Utils.IsOperator(current.Value) &&
                    Utils.ComparePrecedence(current.Value, operation.Value) < 0)
                {
                    break;
                }

                operand.Append(' ');
                operand.Append(current.Value);

                expression.RemoveBlock(current);
            }

            result.Value = operand.ToString();
            return result;
        }

        private void LeftAddition(Block op, Block firstTerm, Block lastTerm)
        {
            Contract.Requires(op.Value == "+");

            /* Eliminate the unary minus of the first term */
            if (firstTerm.Value.StartsWith("-"))
            {
                firstTerm.Value = firstTerm.Value.Substring(1);
                op.Value = "-";
            }
        }

        private void LeftSubtraction(Block op, Block firstTerm, Block lastTerm)
        {
            Contract.Requires(op.Value == "-");

            /* Eliminate the unary minus of the first term */
            if (firstTerm.Value.StartsWith("-"))
            {
                firstTerm.Value = firstTerm.Value.Substring(1);
                op.Value = "+";
            }

            /* Replace + with - and vice versa */
            for (op = firstTerm.Next; op != lastTerm.Next; op = op.Next.Next)
            {
                op.Value = Utils.GetInverse(op.Value);
            }
        }

        private void LeftMultiplication(Block op, Block firstTerm, Block lastTerm)
        {
            Contract.Requires(op.Value == "*");

            Block leftOperand = LongestLeftOperand(op);
            for (Block current = firstTerm; ; current = current.Next.Next)
            {
                Combine(leftOperand, op, current, current);

                /* Eliminate the unary minus */
                if (current.Value.StartsWith("-") && current != firstTerm)
                {
                    current.Value = current.Value.Substring(1);
                    current.Prev.Value = Utils.GetInverse(current.Prev.Value);
                }

                if (current == lastTerm)
                    break;
            }
        }

        private void RightDivMul(Block op, Block firstTerm, Block lastTerm)
        {
            Contract.Requires(op.Value == "/" || op.Value == "*");

            Block rightOperand = LongestRightOperand(op);
            for (Block current = firstTerm; ; current = current.Next.Next)
            {
                Combine(current, op, rightOperand, current);

                /* Eliminate the unary minus */
                if (current.Value.StartsWith("-") && current != firstTerm)
                {
                    current.Value = current.Value.Substring(1);
                    current.Prev.Value = Utils.GetInverse(current.Prev.Value);
                }

                if (current == lastTerm)
                    break;
            }
        }

        private void SimpleRightDivMul(Block rightOperator, Parentheses pases, Block firstTerm, Block lastTerm, bool minor)
        {
            /* Bring the multiplier at the right into the parentheses */
            RightDivMul(rightOperator, firstTerm, lastTerm);
            expression.RemoveBlock(rightOperator.Next);
            expression.RemoveBlock(rightOperator);
            LogTransformation(true);

            /* Expand the parentheses again */
            if (!TryRemoveParentheses(pases, minor))
            {
                steps.Last().IsMinor = false;
            }
        }

        private void MultiplyParentheses(Parentheses left, Parentheses right)
        {
            Contract.Requires(left.End.Next.Value == "*" &&
                              left.End.Next.Next == right.Start);

            List<Block> innerParentheses = new List<Block>();

            foreach (Block leftTerm in left.Reverse())
            {
                if (!Utils.IsOperator(leftTerm.Value))
                {
                    Block newOperation = Block.New(expression, "*");
                    BlockChain newParentheses = right.Copy();

                    expression.InsertRange(leftTerm, newParentheses.Start, newParentheses.End);
                    expression.Insert(leftTerm, newOperation);

                    innerParentheses.Add(leftTerm.Next.Next);
                }
            }

            expression.RemoveBlock(left.End.Next);
            expression.RemoveRange(right.Start, right.End);
            LogTransformation(true);

            foreach (Block openingBrace in innerParentheses)
            {
                ExpandParentheses(Expression.ExtractParentheses(openingBrace), isMinor: true);
            }
            steps.RemoveAt(steps.Count - 1);
        }        

        private bool TryRemoveParentheses(Parentheses pases, bool minor)
        {
            string prevOp = (pases.Start.Prev == null) ? "" : pases.Start.Prev.Value;
            string nextOp = (pases.End.Next == null) ? "" : pases.End.Next.Value;

            if (prevOp == "(" && nextOp == ")" ||
                prevOp == ""  && nextOp == "")
            {
                expression.RemoveBlock(pases.Start);
                expression.RemoveBlock(pases.End);

                LogTransformation(true);
                return true;
            }

            if ((prevOp == "+" || prevOp == "-") &&
                Utils.ComparePrecedence(prevOp, nextOp) >= 0 ||
                (nextOp == "+" || nextOp == "-") && (prevOp == "("))
            {
                ExpandParentheses(pases, isMinor: minor);
                return true;
            }
            return false;
        }        

        /// <summary>
        /// Combines two given terms inserting the specified operator between them.
        /// Unary minuses of the terms are considered.
        /// </summary>
        private void Combine(Block left, Block op, Block right, Block result)
        {
            string leftTerm = left.Value;
            string rightTerm = right.Value;

            /* Minus * Minus is Plus */
            if (leftTerm.StartsWith("-"))
            {
                if (rightTerm.StartsWith("-"))
                {
                    rightTerm = rightTerm.Substring(1);
                    leftTerm = leftTerm.Substring(1);
                }
            }

            /* Transfer right operand unary minus to the left one */
            else if (rightTerm.StartsWith("-"))
            {
                rightTerm = rightTerm.Substring(1);
                leftTerm = "-" + leftTerm;
            }
            result.Value = string.Format("{0} {1} {2}", leftTerm, op.Value, rightTerm);
        }

        /// <summary>
        /// Moves a given parentheses to the end of the expression block it is a part of.
        /// For example: a / (1 - b) * (c + d) -> a * (c + d) / (1 - b)
        /// </summary>
        private bool MoveToEnd(Block operation, Parentheses pases)
        {
            Contract.Requires(operation.Next == pases.Start);

            if (pases.End.Next == null || pases.End.Next.Value == ")")
                return false;

            /* Save old position */
            Block oldPos = operation.Prev;
            Block pos = pases.End.Next;

            /* Find insertion position */
            for ( ; pos != null; pos = pos.Next)
            {
                if (pos.Value == ")" ||
                    Utils.IsOperator(pos.Value) &&
                    Utils.ComparePrecedence(pos.Value, operation.Value) < 0)
                {
                    break;
                }

                /* Skip the parentheses */
                if (pos.Value == "(")
                {
                    while (pos.Value != ")")
                        pos = pos.Next;
                }
            }
            pos = (pos == null) ? expression.End : pos.Prev;

            expression.RemoveRange(operation, pases.End);
            expression.InsertRange(pos, operation, pases.End);

            return (oldPos != operation.Prev);
        }

        #endregion

        protected override void LogTransformation(bool isMinor = false, int groupID = -1, string expr = "")
        {
            steps.Add(new EquivalentForm(expression.ToString(), isMinor, groupID));
        }
    }
}
