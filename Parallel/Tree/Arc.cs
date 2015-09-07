using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parallel.Tree
{
    public class Arc : IEquatable<Arc>
    {
        public Node Source { get; private set; }
        public Node Dest { get; private set; }

        public Arc(Node source, Node dest)
        {
            this.Dest = dest;
            this.Source = source;
        }

        public bool Equals(Arc other)
        {
            return (Source == other.Source) && (Dest == other.Dest);
        }

        public override int GetHashCode()
        {
            return Source.GetHashCode() & Dest.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("{0} <-> {1}", Source.Value, Dest.Value);
        }
    }
}
