using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using Point = System.Windows.Point;

namespace PolygonEditor.Models
{
    internal enum Constraint
    {
        None,
        Parallel,
        Perpendicular,
        All
    }

    internal class Edge
    {
        // offset
        public double A;
        public double B;
        public double OffsetB;

        public static int Width = 3;
        public int PolygonIndex { get; set; }
        public Vertex? Left { get; set; }
        public Vertex? Right { get; set; }
        public Vertex? Mark { get; set; }
        public BresLine? Graphic { get; set; }
        public Image? Icon { get; set; }
        public Constraint Constraint { get; set; } = Constraint.None;
        public static Edge? FindEdge(BresLine line, List<Polygon> polygons)
        {
            foreach (var polygon in polygons)
            {
                foreach (var edge in polygon.Edges)
                {
                    if (line == edge.Graphic) return edge;
                }
            }
            return null;
        }
        public static void DragEdge(Edge edge, Point newPosition, Point lastPosition)
        {
            if (edge.Left is null || edge.Right is null) throw new Exception("DragEdgeException: end vertices are null");

            (var left, var right) = (edge.Left, edge.Right);
            var deltaX = newPosition.X - lastPosition.X;
            var deltaY = newPosition.Y - lastPosition.Y;

            Vertex.DragVertex(left, new Point(left.X + deltaX, left.Y + deltaY), new Point(left.X, left.Y), edge.Constraint);
            Vertex.DragVertex(right, new Point(right.X + deltaX, right.Y + deltaY), new Point(right.X, right.Y), edge.Constraint);
        }
        
        // hepers
        public void AddMarkedVetex(Vertex vertex)
        {
            if(Mark is null)
            {
                Left.Right = vertex;
                Right.Left = vertex;

                vertex.Left = Left;
                vertex.Right = Right;
                return;
            }

            var v1 = new ArrowVector(Left.Center, vertex.Center);
            var v2 = new ArrowVector(Left.Center, Mark.Center);

            if(v1.Length > v2.Length)
            {
                Mark.Right = vertex;
                Right.Left = vertex;

                vertex.Left = Mark;
                vertex.Right = Right;
            }
            else
            {
                Mark.Left = vertex;
                Left.Right = vertex;

                vertex.Right = Mark;
                vertex.Left = Left;
            }
        }

        // constructin an offset
        public void FindOffset(double offset, int sgn)
        {
            if (Right is null || Left is null) throw new Exception("FindOffsetException");

            var a = (Right.Y - Left.Y) / (Right.X - Left.X);
            var b = Left.Y - Left.X * a;

            A = a;
            B = b;
            OffsetB = b + sgn * (offset * Math.Sqrt(1 + Math.Pow(a, 2)));
        }
        public static (double x, double y) OffsetIntersectionCoords(Edge e1, Edge e2, double offset)
        {
            // if one of the edges is perpendicular
            if(e1.Constraint == Constraint.Perpendicular)
            {
                var vector = new ArrowVector(e1.Left.Center, e1.Right.Center);
                var sgn = vector.RetrunRightTurn();
                var x1 = e1.Left.X + offset * sgn;
                var y1 = e2.A * x1 + e2.OffsetB;
                return (x1, y1);
            }
            else if(e2.Constraint == Constraint.Perpendicular)
            {
                var vector = new ArrowVector(e2.Left.Center, e2.Right.Center);
                var sgn = vector.RetrunRightTurn();
                var x1 = e2.Left.X + offset * sgn;
                var y1 = e1.A * x1 + e1.OffsetB;
                return (x1, y1);
            }

            // if we split an edge
            if (e1.A * e2.A > 0 && Math.Abs(Math.Abs(e1.A) - Math.Abs(e2.A)) < 0.000001)
            {
                var middle = (e1.Right == e2.Left) ? e1.Right : e1.Left;

                if(e1.A == 0)
                {
                    return (middle.X, e1.OffsetB);
                }
                
                var a = -(1 / e1.A);
                var b = middle.Y - a * middle.X;
                
                var x1 = (e1.OffsetB - b) / (a - e1.A);
                var y1 = x1 * e2.A + e2.OffsetB;
                return (x1, y1);
            }

            // regular case
            var x = (e1.OffsetB - e2.OffsetB) / (e2.A - e1.A);
            var y = x * e2.A + e2.OffsetB;
            return (x, y);
        }
        public static Point IntersectionCoords(Edge e1, Edge e2)
        {
            var x = (e1.B - e2.B) / (e2.A - e1.A);
            var y = x * e2.A + e2.B;
            return new Point(x,y);
        }

        // fixing an offset
        public static List<(Edge,Edge)> Intersecting(Edge edge, Polygon polygon)
        {
            var points = new List<(Edge, Edge)>();

            foreach (var e in polygon.Edges)
            {
                if (e == edge || !isIntersecting(edge, e)) continue;
                points.Add((edge,e));
            }
            return points;
        }
        private static bool isIntersecting(Edge edge1, Edge edge2)
        {
            var p1 = edge1.Left!.Center;
            var p2 = edge1.Right!.Center;
            var p3 = edge2.Left!.Center;
            var p4 = edge2.Right!.Center;


            var p34 = new Point(p4.X - p3.X, p4.Y - p3.Y);
            var p31 = new Point(p1.X - p3.X, p1.Y - p3.Y);
            var p32 = new Point(p2.X - p3.X, p2.Y - p3.Y);
            var p12 = new Point(p2.X - p1.X, p2.Y - p1.Y);
            var p13 = new Point(p3.X - p1.X, p3.Y - p1.Y);
            var p14 = new Point(p4.X - p1.X, p4.Y - p1.Y);

            double d1 = ArrowVector.VectorProduct(p34, p31);
            double d2 = ArrowVector.VectorProduct(p34, p32);
            double d3 = ArrowVector.VectorProduct(p12, p13);
            double d4 = ArrowVector.VectorProduct(p12, p14);
            double d12 = d1 * d2;
            double d34 = d3 * d4;

            if (d12 > 0 || d34 > 0) return false;
            if (d12 < 0 && d34 < 0) return true;

            if (p1.X == p3.X && p1.Y == p3.Y || p1.X == p4.X && p1.Y == p4.Y || p2.X == p3.X && p2.Y == p3.Y || p2.X == p4.X && p2.Y == p4.Y) return false;

            if (p1.X != p3.X)
            {
                return Math.Max(p1.X, p2.X) > Math.Min(p3.X, p4.X) && Math.Max(p3.X, p4.X) > Math.Min(p1.X, p2.X);
            }
            else
            {
                return Math.Max(p1.Y, p2.Y) > Math.Min(p3.Y, p4.Y) && Math.Max(p3.Y, p4.Y) > Math.Min(p1.Y, p2.Y);
            }
        }
    }
}
