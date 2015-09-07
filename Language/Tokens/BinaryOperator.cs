using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Language.Tokens
{
    public sealed class BinaryOperator : Token
    {
        private BinaryOperator(string symbol) : base(symbol) { }

        public string Symbol
        {
            get { return base.Value; }
        }

        public static BinaryOperator Addition = new BinaryOperator("+");
        public static BinaryOperator Subtraction = new BinaryOperator("-");
        public static BinaryOperator Multiplication = new BinaryOperator("*");
        public static BinaryOperator Division = new BinaryOperator("/");
        
        public static BinaryOperator[] Operators = typeof(BinaryOperator).GetFields()
                                                                          .Where(f => f.IsStatic &&
                                                                                      f.FieldType == typeof(BinaryOperator))
                                                                          .Select(f => f.GetValue(null) as BinaryOperator)
                                                                          .ToArray();
        
        public static bool Recognize(string token, out Token op)
        {
            op = null;
            Token tmp = Operators.FirstOrDefault(o => o.Value == token);

            if (tmp == null)
                return false;

            op = (Token)tmp.Clone();
            return true;
        }
    }
}
