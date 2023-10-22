using System;
using PolygonEditor.Models;

namespace PolygonEditor
{
    internal class RayCasting
    {
        private static bool isIntersecting(System.Windows.Point point, Edge edge)
        {
            if (edge.Right is null || edge.Left is null) throw new Exception("IsIntersectingException");
            (var yMin, var yMax) = edge.Left.Y < edge.Right.Y ? (edge.Left.Y, edge.Right.Y) : (edge.Right.Y, edge.Left.Y);
            (var xMin, var xMax, var p1, var p2) = edge.Left.X < edge.Right.X ? (edge.Left.X, edge.Right.X, edge.Left, edge.Right) : (edge.Right.X, edge.Left.X, edge.Right, edge.Left);

            if (point.Y > yMax || point.Y < yMin) return false;
            if (point.X > xMax) return false;
            if (point.X < xMin) return true;

            var x = ((point.Y - p1.Y)/(p2.Y - p1.Y))*(p2.X - p1.X) + p1.X;

            return point.X < x;
        }

        public static bool isVertexInPolygon(System.Windows.Point point, Polygon polygon)
        {
            var intersections = 0;
            foreach(var edge in polygon.Edges)
            {
                if (isIntersecting(point, edge)) intersections++;
            }

            return intersections % 2 == 1;
        }
    }
}
