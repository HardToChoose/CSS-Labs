using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Language.Tokens
{
    public class Variable : Token, ISignedValue
    {
        private Variable(string name) : base(name)
        {
            this.IsNegative = false;
        }

        public string Name
        {
            get
            {
                return base.Value;
            }
        }

        public bool IsNegative { get; set; }

        public override string ToString()
        {
            return (IsNegative) ? ("-" + Name) : Name;
        }

        public static bool Recognize(string token, out Token variable)
        {
            variable = null;
            if (string.IsNullOrWhiteSpace(token))
                return false;

            if (char.IsLetter(token[0]) &&
                token.All(ch => char.IsLetter(ch) || char.IsDigit(ch) || ch == '_'))
            {
                variable = new Variable(token);
                return true;
            }
            return false;
        }
    }
}
