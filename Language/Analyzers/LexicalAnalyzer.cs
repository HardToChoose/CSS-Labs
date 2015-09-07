using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Language.FSM;
using Language.Tokens;

namespace Language.Analyzers
{
    public class LexicalAnalyzer
	{
		private StringBuilder code = new StringBuilder();

        private static readonly char[] separators = new char[] { '\n', '\t', '\0', ' ' };

        private static bool IsSeparator(char symbol)
		{
			return "\n \t\0".Contains(symbol);
		}

        private static bool Recognize(string token, out Token tokenObject)
        {
            if (MathFunction.Recognize(token, out tokenObject) ||
                IntegerConstant.Recognize(token, out tokenObject) ||
                DoubleConstant.Recognize(token, out tokenObject) ||
                Variable.Recognize(token, out tokenObject))
            {
                return true;
            }
            return false;
        }

        private void TryAddToken(TextReader reader, Location begin, Location end, List<Token> outList)
        {
            if (begin < end)
            {
                string prevToken = reader.GetPart(begin, end).Trim(separators);
                if (prevToken.All(ch => IsSeparator(ch)))
                    return;

                Token prev;
                if (Recognize(prevToken, out prev))
                {
                    prev.Location = begin;
                    outList.Add(prev);
                }
                else
                    ErrorLogger.LogError(ErrorType.LEXICAL, begin, "Нерозпізнана лексема '{0}'", prevToken);
            }
        }

		private IList<Token> ParseTokens(TextReader reader)
		{
            List<Token> tokens = new List<Token>();
            Token token;

            bool more = reader.MoveNext();
            Location lastDelimiter = reader.CurrentLocation;

            while (more)
            {
                if (Delimiter.Recognize(reader.Current.ToString(), out token) ||
                    BinaryOperator.Recognize(reader.Current.ToString(), out token))
                {
                    token.Location = reader.CurrentLocation;

                    /* Try to recognize the token between this and previous delimiter/operator */
                    TryAddToken(reader, lastDelimiter, token.Location, tokens);

                    more = reader.MoveNext();
                    lastDelimiter = reader.CurrentLocation;
                    tokens.Add(token);
                }
                else
                    more = reader.MoveNext();
            }

            TryAddToken(reader, lastDelimiter, reader.CurrentLocation, tokens);
            return tokens;
		}

		public void Feed(string code)
		{
			this.code.Append(code);
		}

        public void Clear()
        {
            this.code.Clear();
        }

		public IList<Token> Analyze()
		{
            using (var reader = new TextReader(this.code.ToString()))
            {
                IList<Token> tokens = ParseTokens(reader);

                /* Милиці для унарного оператора */
                for (int k = 1; k < tokens.Count; k++)
                {
                    var token = tokens[k] as ISignedValue;

                    if ((token != null) &&
                        (tokens[k - 1] == BinaryOperator.Subtraction))
                    {
                        if ((k == 1) ||
                            (BinaryOperator.Operators.Contains(tokens[k - 2]) ||
                             tokens[k - 2] == Delimiter.LeftParenthesis ||
                             tokens[k - 2] == Delimiter.Comma))
                        {
                            token.IsNegative = true;
                            tokens.RemoveAt(k - 1);
                            k--;
                        }
                    }
                }
                return tokens;
            }
		}
	}
}
