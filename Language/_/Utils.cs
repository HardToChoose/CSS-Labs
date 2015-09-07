using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Language.Tokens;

namespace Language
{
    public class Utils
    {
        private static readonly string[] operators = new string[] { "+", "-", "*", "/", "(", ")" };
        private static readonly string[] inverseOps = new string[] { "-", "+", "/", "*" };

        private static readonly int[] complexity = new int[] { 1, 1, 2, 4, 0, 0 };
        private static readonly int[] predecence = new int[] { 1, 1, 2, 2, 0, 0 };

        public static int ComparePrecedence(string operator_1, string operator_2)
        {
            if (operator_1 == operator_2)
                return 0;

            if (string.IsNullOrWhiteSpace(operator_1))
                return -1;
            if (string.IsNullOrWhiteSpace(operator_2))
                return 1;

            return predecence[Array.IndexOf(operators, operator_1)] -
                   predecence[Array.IndexOf(operators, operator_2)];
        }

        public static int GetOperationComplexity(string operation)
        {
            int index = Array.IndexOf(operators, operation);
            return (index == -1) ? 0 : complexity[index];
        }

        public static bool IsOperator(string token)
        {
            return operators.Contains(token) && (token != "(") && (token != ")");
        }

        public static string GetInverse(string op)
        {
            return inverseOps[Array.IndexOf(operators, op)];
        }

        public static IList<string> TokensToOps(IList<Token> tokens)
        {
            IList<string> result = new List<string>();

            for (int k = 0; k < tokens.Count; k++)
            {
                Token current = tokens[k];

                if (current == Delimiter.LeftParenthesis ||
                    current == Delimiter.RightParenthesis ||
                    !(current is MathFunction))
                {
                    result.Add(current.ToString());
                    continue;
                }
                
                if (current is MathFunction)
                {
                    /* Join function with its arguments into a single string */
                    StringBuilder func = new StringBuilder();
                    for ( ; tokens[k] != Delimiter.RightParenthesis; k++)
                        func.Append(tokens[k].ToString());

                    /* Add closing parenthesis */
                    func.Append(tokens[k]);
                    func.Replace(",", ", ");

                    result.Add(func.ToString());
                }
            }
            return result;
        }

        public static bool OptimizeUnaries(IList<Token> tokens)
        {
            bool optimized = false;

            for (int k = 0; k < tokens.Count - 1; k++)
            {
                ISignedValue nextToken = tokens[k + 1] as ISignedValue;

                if (tokens[k].Value == "-" &&
                    nextToken != null && nextToken.IsNegative)
                {
                    tokens[k].Value = "+";
                    nextToken.IsNegative = false;

                    optimized = true;
                }
            }
            return optimized;
        }

        public static IList<string> InfixToPostfix(IList<string> ops)
        {
            List<string> postfix = new List<string>();
            Stack<string> stack = new Stack<string>();

            for (int k = 0; k < ops.Count; k++)
            {
                string current = ops[k];

                /* Operator */
                if (IsOperator(current))
                {
                    while (stack.Count > 0 &&
                           ComparePrecedence(stack.Peek(), current) >= 0)
                    {
                        postfix.Add(stack.Pop());
                    }
                    stack.Push(current);
                }

                /* Right parenthesis */
                else if (current == "(")
                    stack.Push(current);

                /* Left parenthesis */
                else if (current == ")")
                {
                    while (stack.Count > 0 &&
                           stack.Peek() != "(")
                    {
                        postfix.Add(stack.Pop());
                    }

                    if (stack.Count != 0)
                        stack.Pop();
                }

                /* Operand */
                else
                    postfix.Add(current);
            }

            /* Push operators left in stack to the prefix expression */
            while (stack.Count != 0)
            {
                if (IsOperator(stack.Peek()))
                    postfix.Add(stack.Pop());
                else
                    throw new ApplicationException("Error while converting infix to prefix");
            }
            return postfix;
        }
    }
}
