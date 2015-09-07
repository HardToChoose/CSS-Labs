using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Language.Tokens
{
    public interface ISignedValue
    {
        bool IsNegative { get; set; }
    }
}
