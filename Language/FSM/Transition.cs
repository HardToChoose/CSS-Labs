using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Language.Tokens;

namespace Language
{
    internal enum TransitionPriority
    {
        Lowest,
        Normal,
        Highest
    }

    internal class Transition
    {
        public int SourceState { get; private set; }
        public int DestState { get; private set; }
        public TransitionPriority Priority { get; private set; }

        public Predicate<Token> Condition { get; private set; }
        public Func<Token, int> Action { get; private set; }

        public Transition(int sourceState, int destState, Predicate<Token> condition,
                          Func<Token, int> action, TransitionPriority priority = TransitionPriority.Normal)
        {
            this.SourceState = sourceState;
            this.DestState = destState;

            this.Priority = priority;

            this.Condition = condition;
            this.Action = action;
        }
    }
}
