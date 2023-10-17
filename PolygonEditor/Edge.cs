using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace PolygonEditor
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
        public static int Width = 3;
        public int PolygonIndex { get; set; }
        public Vertex? Left { get; set; }
        public Vertex? Right { get; set; }
        public BresLine? Graphic { get; set; }
        public Image? Icon { get; set; }
        public Constraint Constraint { get; set; } = Constraint.None;
        public static Edge? FindEdge(BresLine line, List<Polygon> polygons)
        {
            foreach(var polygon in polygons)
            {
                foreach(var edge in polygon.Edges)
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

            Vertex.DragVertex(left, new System.Windows.Point(left.X + deltaX,left.Y + deltaY), new System.Windows.Point(left.X, left.Y), edge.Constraint);
            Vertex.DragVertex(right, new System.Windows.Point(right.X + deltaX,right.Y + deltaY), new System.Windows.Point(right.X, right.Y),edge.Constraint);
        }
    }
}
