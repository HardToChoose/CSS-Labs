using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parallel.Tree
{
    public class Node
    {
        public string Value { get; set; }

        public Node Parent { get; set; }
        public IList<Node> Children { get; private set; }

        public Node(Node parent, string value)
        {
            this.Value = value;
            this.Parent = parent;
            this.Children = new List<Node>();
        }

        public IEnumerable<Arc> OutArcs
        {
            get
            {
                List<Arc> result = new List<Arc>();
                foreach (var child in Children)
                {
                    result.Add(new Arc(this, child));
                    result.AddRange(child.OutArcs);
                }
                return result;
            }
        }

        public int[] Path
        {
            get
            {
                List<int> path = new List<int>();
                for (Node current = this; current.Parent != null; current = current.Parent)
                {
                    path.Insert(0, current.Parent.Children.IndexOf(current));
                }
                return path.ToArray();
            }
        }

        public override int GetHashCode()
        {
            int hash = Value.GetHashCode() ^ Children.GetHashCode();
            return (Parent == null) ? hash : (hash ^ Parent.GetHashCode());
        }
    }
}
