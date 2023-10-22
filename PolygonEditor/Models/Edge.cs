using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

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
        public static void DragEdge(Edge edge, System.Windows.Point newPosition, System.Windows.Point lastPosition)
        {
            if (edge.Left is null || edge.Right is null) throw new Exception("DragEdgeException: end vertices are null");

            (var left, var right) = (edge.Left, edge.Right);
            var deltaX = newPosition.X - lastPosition.X;
            var deltaY = newPosition.Y - lastPosition.Y;

            Vertex.DragVertex(left, new System.Windows.Point(left.X + deltaX, left.Y + deltaY), new System.Windows.Point(left.X, left.Y), edge.Constraint);
            Vertex.DragVertex(right, new System.Windows.Point(right.X + deltaX, right.Y + deltaY), new System.Windows.Point(right.X, right.Y), edge.Constraint);
        }

        public void FindOffset(double offset, int sgn)
        {
            if (Right is null || Left is null) throw new Exception("FindOffsetException");
            var a = (Right.Y - Left.Y) / (Right.X - Left.X);
            var b = Left.Y - Left.X * a;

            A = a;
            B = b;
            OffsetB = b + sgn * (offset * Math.Sqrt(1 + Math.Pow(a, 2)));
        }

        public static (System.Windows.Point point, Edge edge)? Intersecting(Edge edge, Polygon polygon)
        {
            foreach (var e in polygon.Edges)
            {

                if (!isIntersecting(e, edge)) continue;
                var x = (e.B - edge.B) / (edge.A - e.A);
                var y = x * edge.A + edge.B;

                var p1 = e.Left;
                var p2 = e.Right;

                (var xMin, var xMax) = p1.X < p2.X ? (p1.X, p2.X) : (p2.X, p1.X);
                (var yMin, var yMax) = p1.Y < p2.Y ? (p1.Y, p2.Y) : (p2.Y, p1.Y);

                if (x < xMax && x > xMin && y < yMax && y > yMin) return (new System.Windows.Point(x, y), e);

            }
            return null;
        }

        public static bool isIntersecting(Edge edge1, Edge edge2)
        {
            var p1 = edge1.Left.Center;
            var p2 = edge1.Right.Center;
            var p3 = edge2.Left.Center;
            var p4 = edge2.Right.Center;


            var p34 = new System.Windows.Point(p4.X - p3.X, p4.Y - p3.Y);
            var p31 = new System.Windows.Point(p1.X - p3.X, p1.Y - p3.Y);
            var p32 = new System.Windows.Point(p2.X - p3.X, p2.Y - p3.Y);
            var p12 = new System.Windows.Point(p2.X - p1.X, p2.Y - p1.Y);
            var p13 = new System.Windows.Point(p3.X - p1.X, p3.Y - p1.Y);
            var p14 = new System.Windows.Point(p4.X - p1.X, p4.Y - p1.Y);

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
