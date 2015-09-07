using GraphX;
using GraphX.Logic;
using QuickGraph;

namespace Lab_1.Graph
{
    public class Vertex : VertexBase
    {
        public string Value { get; set; }

        public override string ToString()
        {
            return Value;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }

    public class Edge : EdgeBase<Vertex>
    {
        public Edge(Vertex source, Vertex target) : base(source, target, 1) {}

        public Edge() : base(null, null, 1) {}
    }

    public class TreeGraph : BidirectionalGraph<Vertex, Edge> { }

    public class TreeGraphArea : GraphArea<Vertex, Edge, BidirectionalGraph<Vertex, Edge>> { }

    public class GraphLogicCore : GXLogicCore<Vertex, Edge, BidirectionalGraph<Vertex, Edge>> { }
}
