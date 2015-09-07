using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Language
{
    public struct Location
    {
        public int Line { get; private set; }
        public int Position { get; private set; }

        public static Location None = new Location(-1, -1);

        public Location(int line, int position) : this()
        {
            this.Line = line;
            this.Position = position;
        }

        public Location NextLine()
        {
            return new Location { Line = this.Line + 1, Position = 0 };
        }

        public Location NextPosition()
        {
            return new Location { Line = this.Line, Position = this.Position + 1 };
        }

        public static bool operator <(Location a, Location b)
        {
            return  (a.Line < b.Line) || ((a.Line == b.Line) && (a.Position < b.Position));
        }

        public static bool operator >(Location a, Location b)
        {
            return (a.Line > b.Line) || ((a.Line == b.Line) && (a.Position > b.Position));
        }
    }
}
