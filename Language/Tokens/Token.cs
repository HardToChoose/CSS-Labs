using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Language.Tokens
{
	public class Token : ICloneable
	{
        public string Value { get; set; }
        public Location Location { get; set; }

        #region Public methods

        public Token(string value)
        {
            this.Value = value;
        }

		public Token(string value, Location location)
		{
			this.Value = value;
            this.Location = location;
		}

        public static string Concat(IEnumerable<Token> tokens)
        {
            StringBuilder result = new StringBuilder();

            foreach (var token in tokens)
            {
                if (token is BinaryOperator)
                {
                    result.Append(' ');
                    result.Append(token.Value);
                    result.Append(' ');
                }
                else if (token == Delimiter.Comma)
                {
                    result.Append(token.Value);
                    result.Append(' ');
                }
                else
                    result.Append(token.Value);
            }
            return result.ToString();
        }

        #endregion

        #region Interface implemetations and overriden methods

        public object Clone()
        {
            return base.MemberwiseClone();
        }

        public override bool Equals(object obj)
        {
            if (obj is Token)
            {
                return (obj as Token).Value == this.Value;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public override string ToString()
        {
            return Value;
        }

        #endregion

        #region Operator overloads

        public static bool operator ==(Token a, Token b)
        {
            if (object.ReferenceEquals(a, b))
                return true;

            if ((object)a == null || (object)b == null)
                return false;

            return a.Equals(b);
        }

        public static bool operator !=(Token a, Token b)
        {
            return !(a == b);
        }

        #endregion
    }
}
