using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Language.Tokens;

namespace Language.FSM
{
    internal delegate object Action(object arg);

    internal abstract class Fsm 
	{
        public const int NONE_STATE = -2;
		public const int END_STATE = -1;

        private int startState;
		
        protected Transition[] transitions = new Transition[0];

        protected Fsm(int startState)
        {
            this.startState = startState;
        }

        public void Run(IEnumerable<Token> tokens, bool stubbornMode)
		{
            var currentState = startState;
            var enumerator = tokens.GetEnumerator();
            Token lastToken = null;

            while (enumerator.MoveNext() && currentState != END_STATE)
            {
                lastToken = enumerator.Current;
                var transition = this.transitions.Where(t => (t.SourceState == currentState) &&
                                                              t.Condition(enumerator.Current))
                                                 .OrderByDescending(t => t.Priority)
                                                 .FirstOrDefault();
                if (transition == null)
                    break;

                var nextState = transition.Action(enumerator.Current);
                nextState = (nextState == NONE_STATE) ? transition.DestState : nextState;

                if (!(stubbornMode && nextState == END_STATE))
                    currentState = nextState;
            }

            this.FinalCheck(enumerator, lastToken, currentState);
		}

        protected virtual void FinalCheck(IEnumerator<Token> tokens, Token lastToken, int lastState) { }
	}
}
