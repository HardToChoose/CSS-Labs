using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Language.Tokens
{
    public class IntegerConstant : Token, ISignedValue
    {
        private IntegerConstant(int value, string token) : base(token)
        {
            this.Value = value;
            this.IsNegative = false;
        }

        public new int Value { get; private set; }

        public bool IsNegative { get; set; }

        public override string ToString()
        {
            return (IsNegative) ? ("-" + Value.ToString()) : Value.ToString();
        }

        public static bool Recognize(string token, out Token result)
        {
            int number;
            result = null;

            if (int.TryParse(token, out number))
            {
                result = new IntegerConstant(number, token);
                return true;
            }
            return false;
        }
    }
}
