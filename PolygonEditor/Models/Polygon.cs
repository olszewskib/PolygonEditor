using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolygonEditor.Models
{
    internal class Polygon
    {
        public static int Id = -1;

        public Polygon? OffsetPolygon;
        public Vertex FirstVertex { get; set; }
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
            // last vertex is changed after
            // needed for vertex removal
            if (e.Left == LastVertex) LastVertex.RightEdge = e;

            // adding new edge
            Edges.Add(e);
        }
        public static Polygon? FindPolygon(System.Windows.Point point, List<Polygon> polygons)
        {
            foreach (var polygon in polygons)
            {
                if (RayCasting.isVertexInPolygon(point, polygon)) return polygon;
            }

            return null;
        }
        public void SetOffestForEdges(double offset)
        {
            var start = FirstVertex;
            var end = start.Right ?? throw new Exception("OffsetException");
            var vector = new ArrowVector(start, end);
            var sgn = vector.RetrunRightTurn();

            var edge = start.RightEdge ?? throw new Exception("OffestExcetpion");
            edge.FindOffset(offset, sgn);

            start = start.Right;

            while (start != FirstVertex)
            {
                end = start.Right ?? throw new Exception("OffsetException");
                vector = new ArrowVector(start, end);
                sgn = vector.RetrunRightTurn();

                edge = start.RightEdge ?? throw new Exception("OffestExcetpion");
                edge.FindOffset(offset, sgn);

                start = start.Right;
            }

        }
        public void MakePolygonIntoALeftTurn()
        {
            var mostLeftVertex = FirstVertex;

            foreach(var vertex in Vertices)
            {
                if(vertex.X < mostLeftVertex.X) mostLeftVertex = vertex;
            }

            // now we should have the furthest vertex to the left of all polygon
            var lowerNeighbour = mostLeftVertex.Right.Y >= mostLeftVertex.Left.Y ? mostLeftVertex.RightEdge : mostLeftVertex.LeftEdge;

            if (lowerNeighbour.Left == mostLeftVertex) return;

            foreach(var vertex in Vertices)
            {
                (vertex.Left, vertex.Right) = (vertex.Right, vertex.Left);
                (vertex.LeftEdge, vertex.RightEdge) = (vertex.RightEdge, vertex.LeftEdge);
            }

            foreach(var edge in Edges)
            {
                (edge.Left, edge.Right) = (edge.Right, edge.Left);
            }
            
        }
    }
}
