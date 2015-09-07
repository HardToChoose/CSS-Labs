using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Language.Tokens;

namespace Language.FSM
{
    internal class FsmMain : Fsm
    {
        private int parentheses;

        public FsmMain() : base(1)
        {
            base.transitions = new Transition[]
            {
                new Transition(1, 2,         TokenPredicates.IsLeftParethesis,     CheckLeftParenthesis),
                new Transition(1, 3,         TokenPredicates.IsRightParethesis,    CheckRightParenthesis),
                new Transition(1, 3,         TokenPredicates.IsVariableOrConstant, GotoNextState),
                new Transition(1, 4,         TokenPredicates.IsMathFunctionName,   GotoNextState, TransitionPriority.Highest),
                new Transition(1, END_STATE, TokenPredicates.IsRightParethesis,    token =>
                                                                                   {
                                                                                       this.parentheses--;
                                                                                       return Error.ReportLex(token, 2);
                                                                                   }),
                new Transition(1, END_STATE, TokenPredicates.IsOperator,           Error.ReportLex(6)),

                new Transition(2, 3,         TokenPredicates.IsVariableOrConstant, GotoNextState),
                new Transition(2, 4,         TokenPredicates.IsMathFunctionName,   GotoNextState, TransitionPriority.Highest),
                new Transition(2, 2,         TokenPredicates.IsLeftParethesis,     CheckLeftParenthesis),
                new Transition(2, END_STATE, TokenPredicates.IsRightParethesis,    token =>
                                                                                   {
                                                                                       this.parentheses--;
                                                                                       return Error.ReportLex(token, 7);
                                                                                   }),

                new Transition(3, 1,         TokenPredicates.IsOperator,           GotoNextState),
                new Transition(3, 3,         TokenPredicates.IsRightParethesis,    CheckRightParenthesis),
                new Transition(3, END_STATE, TokenPredicates.IsLeftParethesis,     Error.ReportLex(4)),
                new Transition(3, END_STATE, TokenPredicates.Any,                  Error.ReportLex(3), TransitionPriority.Lowest),

                new Transition(4, 5,         TokenPredicates.IsLeftParethesis,     CheckLeftParenthesis),
                new Transition(4, END_STATE, TokenPredicates.Any,                  Error.ReportLex(0), TransitionPriority.Lowest),

                new Transition(5, 4,         TokenPredicates.IsMathFunctionName,   GotoNextState, TransitionPriority.Highest),
                new Transition(5, 6,         TokenPredicates.IsVariableOrConstant, GotoNextState),
                new Transition(5, END_STATE, TokenPredicates.Any,                  Error.ReportLex(1), TransitionPriority.Lowest),

                new Transition(6, 3,         TokenPredicates.IsRightParethesis,    CheckRightParenthesis),
                new Transition(6, 5,         TokenPredicates.IsComma,              GotoNextState),
                new Transition(6, END_STATE, TokenPredicates.Any,                  Error.ReportLex(8), TransitionPriority.Lowest),
            };
            this.parentheses = 0;
        }

        private int GotoNextState(Token token)
        {
            return NONE_STATE;
        }

        private int CheckLeftParenthesis(Token token)
        {
            this.parentheses++;
            return NONE_STATE;
        }

        private int CheckRightParenthesis(Token token)
        {
            if (this.parentheses <= 0)
                return Error.ReportLex(token, 2);

            this.parentheses--;
            return NONE_STATE;
        }

        protected sealed override void FinalCheck(IEnumerator<Token> tokens, Token lastToken, int lastState)
        {
            if (!tokens.MoveNext() &&
                (TokenPredicates.IsOperator(lastToken) ||
                 TokenPredicates.IsComma(lastToken)) ||
                 TokenPredicates.IsLeftParethesis(lastToken))
            {
                Error.ReportLex(lastToken, 5);
            }

            if (this.parentheses > 0)
                ErrorLogger.LogError(ErrorType.LEXICAL, Location.None, "Не закрито {0} дужок", this.parentheses);
            else if (this.parentheses < 0)
                ErrorLogger.LogError(ErrorType.LEXICAL, Location.None, "Не вистачає {0} відкривних дужок", -this.parentheses);
        }
    }
}
