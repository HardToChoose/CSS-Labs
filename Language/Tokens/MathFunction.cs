using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Language.Tokens
{
    public class MathFunction : Token, ISignedValue
    {
        public string Name
        {
            get { return base.Value; }
        }

        public bool IsNegative { get; set; }

        public override string ToString()
        {
            return (IsNegative) ? ("-" + Name) : Name;
        }

        private MathFunction(string name) : base(name)
        {
            this.IsNegative = false;
        }


        public static MathFunction Sin = new MathFunction("sin");//, 0);
        public static MathFunction Cos = new MathFunction("cos");//, 0);
        public static MathFunction Pow = new MathFunction("pow");//, 0, 1);
        
        private static MathFunction[] functions = typeof(MathFunction).GetFields()
                                                                      .Where(f => f.IsStatic &&
                                                                                  f.FieldType == typeof(MathFunction))
                                                                      .Select(f => f.GetValue(null) as MathFunction)
                                                                      .ToArray();
        public static IEnumerable<MathFunction> AllFunctions
        {
            get { return functions; }
        }

        public static bool Recognize(string token, out Token tokenObject)
        {
            tokenObject = null;
            Token tmp = functions.FirstOrDefault(func => func.Name == token);

            if (tmp == null)
                return false;

            tokenObject = (Token)tmp.Clone();
            return true;
        }
    }
}
