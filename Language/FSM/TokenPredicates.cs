using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Language.Tokens;

namespace Language.FSM
{
    internal class TokenPredicates
    {
        public static bool Any(Token token)
        {
            return true;
        }

        public static bool IsComma(Token token)
        {
            return token.Value == Delimiter.Comma.Value;
        }

        public static bool IsOperator(Token token)
        {
            return token is BinaryOperator;
        }

        public static bool IsVariableOrConstant(Token token)
        {
            return (token is Variable) ||
                   (token is IntegerConstant) ||
                   (token is DoubleConstant);
        }

        public static bool IsMathFunctionName(Token token)
        {
            return MathFunction.AllFunctions.Any(func => func.Name == token.Value);
        }

        public static bool IsLeftParethesis(Token token)
        {
            return Delimiter.LeftParenthesis == token;
        }

        public static bool IsRightParethesis(Token token)
        {
            return token == Delimiter.RightParenthesis;
        }
    }
}
