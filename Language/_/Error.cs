using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Language.FSM;
using Language.Tokens;

namespace Language
{
	public enum ErrorType { LEXICAL, SYNTACTIC, SEMANTIC };

	public class Error
	{
        public ErrorType Type { get; set; }
        public string Message { get; set; }
        public Location Location { get; set; }

		public Error(ErrorType type, string message, Location location)
		{
			this.Type = type;
			this.Message = message;
            this.Location = location;
		}

        internal static readonly string[] Messages = new string[]
        {
            "Очікувана відкривна дужка після імені функції",
            "Параметром функції може бути константа, змінна або виклик ще однієї функції",
            "Закривна дужка явно тут зайва",
            "За константою, змінною або закривною дужкою може слідувати лише оператор",
            "Недоречна відкривна дужка",
            "Вираз не може закінчуватись оператором, комою або відкривною дужкою",
            "Оператор на початку виразу, або одразу після іншого оператора",
            "Порожні дужки недопустимі",
            "Недопустимий параметр функції"
        };

        internal static int ReportLex(Token token, int messageId)
        {
            return ReportLex(messageId)(token);
        }

        internal static Func<Token, int> ReportLex(int messageId)
        {
            string message = (messageId >= 0 && messageId < Messages.Length) ? Messages[messageId] : null;
            if (message == null)
                return (token) => Fsm.END_STATE;

            return (token) =>
            {
                ErrorLogger.LogError(ErrorType.LEXICAL, token.Location, message);
                return Fsm.END_STATE;
            };
        }
	}
}
