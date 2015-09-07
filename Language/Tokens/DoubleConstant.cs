using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Language.Tokens
{
    public class DoubleConstant : Token, ISignedValue
    {
        private DoubleConstant(double value, string token) : base(token)
        {
            this.Value = value;
            this.IsNegative = false;
        }

        public new double Value { get; private set; }

        public bool IsNegative { get; set; }

        public override string ToString()
        {
            string valueStr = Value.ToString(CultureInfo.InvariantCulture);
            return (IsNegative) ? ("-" + valueStr) : valueStr;
        }

        public static bool Recognize(string token, out Token result)
        {
            double number;
            result = null;
            
            if (double.TryParse(token, NumberStyles.AllowDecimalPoint, NumberFormatInfo.InvariantInfo, out number))
            {
                result = new DoubleConstant(number, token);
                return true;
            }
            return false;
        }
    }
}
