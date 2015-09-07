using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Language.Tokens
{
    public class Delimiter : Token
    {
        private Delimiter(string symbol) : base(symbol) {}

        public string Symbol
        {
            get { return base.Value; }
        }

        public static Delimiter Comma = new Delimiter(",");
        public static Delimiter LeftParenthesis = new Delimiter("(");
        public static Delimiter RightParenthesis = new Delimiter(")");

        private static Delimiter[] AllDelimiters = typeof(Delimiter).GetFields()
                                                                    .Where(f => f.IsStatic &&
                                                                                f.FieldType == typeof(Delimiter))
                                                                    .Select(f => f.GetValue(null) as Delimiter)
                                                                    .ToArray();

        public static IEnumerable<Delimiter> Delimiters
        {
            get { return AllDelimiters; }
        }

        public static bool Recognize(string token, out Token delimiter)
        {
            delimiter = null;
            Token tmp = Delimiters.FirstOrDefault(d => d.Symbol == token);

            if (tmp == null)
                return false;

            delimiter = (Token)tmp.Clone();
            return true;
        }
    }
}
