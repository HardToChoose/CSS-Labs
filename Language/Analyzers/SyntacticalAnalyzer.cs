using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Language.FSM;
using Language.Tokens;

namespace Language.Analyzers
{
    public class SyntacticalAnalyzer
	{
		public void Analyze(IList<Token> tokens, bool allErrors = false)
		{
            FsmMain fsm = new FsmMain();
            fsm.Run(tokens, allErrors);
		}
	}
}
