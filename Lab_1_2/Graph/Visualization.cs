using Parallel.Tree;

using System.Collections.Generic;

namespace Lab_1.Graph
{
    public class Visualization
    {
        public static TreeGraph GetTreeGraph(Tree tree)
        {
            TreeGraph graph = new TreeGraph();
            Dictionary<Node, Vertex> map = new Dictionary<Node, Vertex>();

            foreach (var node in tree.Nodes)
            {
                map[node] = new Vertex { ID = node.GetHashCode(), Value = node.Value };
                graph.AddVertex(map[node]);
            }

            foreach (var arc in tree.Arcs)
                graph.AddEdge(new Edge(map[arc.Source], map[arc.Dest]));

            return graph;
        }
    }
}
