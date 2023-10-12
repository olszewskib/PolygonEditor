using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolygonEditor
{
    internal class Polygon
    {
        public static int Id = -1;
        public Vertex FirstVertex {  get; set; }
        public Vertex LastVertex { get; set; }

        public List<Vertex> Vertices = new();

        public List<Edge> Edges = new();
        public Polygon(Vertex first, Vertex last)
        {
            Id++;
            FirstVertex = first;
            LastVertex = last;
        }
        public void AddVertex(Vertex v)
        {
            // adding neighbours
            LastVertex.Right = v;
            v.Left = LastVertex;

            // adding missing edge
            v.LeftEdge = LastVertex.RightEdge;

            // adding new vertex
            LastVertex = v;
            Vertices.Add(v);
        }

        public void AddEdge(Edge e)
        {
            // last vertex jest zmieniany pozniej
            LastVertex.RightEdge = e;

            // adding new edge
            Edges.Add(e);
        }
    }
}
