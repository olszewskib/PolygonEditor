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

        public static (double x, double y) OffsetIntersectionCoords(Edge e1, Edge e2)
        {
            var x = (e1.OffsetB - e2.OffsetB) / (e2.A - e1.A);
            var y = x * e2.A + e2.OffsetB;
            return (x, y);
        }

        // fixing an offset
        public static (Point point, Edge edge)? Intersecting(Edge edge, Polygon polygon)
        {
            foreach (var e in polygon.Edges)
            {
                if (e == edge || !isIntersecting(e, edge)) continue;
                var x = (e.B - edge.B) / (edge.A - e.A);
                var y = x * edge.A + edge.B;

                var p1 = e.Left;
                var p2 = e.Right;

                (var xMin, var xMax) = p1.X < p2.X ? (p1.X, p2.X) : (p2.X, p1.X);
                (var yMin, var yMax) = p1.Y < p2.Y ? (p1.Y, p2.Y) : (p2.Y, p1.Y);

                if (x < xMax && x > xMin && y < yMax && y > yMin) return (new Point(x, y), e);

            }
            return null;
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
