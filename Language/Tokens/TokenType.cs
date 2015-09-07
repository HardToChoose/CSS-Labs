using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Language
{
    public enum TokenType
    {
        UNKNOWN,
        TYPE,
        ENTRY_POINT,
        OPERATION,
        RELATION,
        ASSIGNMENT,
        LOOP_START,
        LOOP_END,
        BEGIN,
        END,
        SEMICOLON,
        OPENING_BRACKET,
        CLOSING_BRACKET,
        IDENTIFIER,
        INT_CONSTANT
    };
}
