using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Language
{
    public static class ErrorLogger
    {
        private static List<Error> errors = new List<Error>();

        public static IEnumerable<Error> Errors
        {
            get { return errors; }
        }

        public static void ClearLog()
        {
            errors.Clear();
        }

        public static void LogError(ErrorType errorType, Location location, string format, params object[] args)
        {
            errors.Add(new Error(errorType, string.Format(format, args), location));
        }
    }
}
