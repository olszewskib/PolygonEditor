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
        Perpendicular
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
            if(edge.Left.Left is null || edge.Right.Right is null) throw new Exception("DragEdgeException: neighbour vertices are null");
            if(edge.Left.LeftEdge is null || edge.Right.RightEdge is null) throw new Exception("DragEdgeException: neighbour edges are null");

            (var left, var right) = (edge.Left, edge.Right);

            var deltaX = newPosition.X - lastPosition.X;
            var deltaY = newPosition.Y - lastPosition.Y;

            // moving left vertex
            left.X += deltaX;
            left.Y += deltaY;

            // moving right vertex
            right.X += deltaX;
            right.Y += deltaY;

            ConnectVertex(edge, edge.Left, edge.Right);
            ConnectVertex(edge.Left.LeftEdge, edge.Left.Left, edge.Left);
            ConnectVertex(edge.Right.RightEdge, edge.Right, edge.Right.Right);
        }
        public static void ConnectVertex(Edge edge, Vertex v1, Vertex v2)
        {
            if (v1.Graphic is null || v2.Graphic is null) throw new Exception("RedrawEdgeException: vertex.Graphic is null");

            var center1 = v1.Center;
            var center2 = v2.Center;

            if (edge.Graphic == null) throw new Exception("RedrawEdgeException: edgeGraphic is null");

            edge.Graphic.X1 = center1.X;
            edge.Graphic.Y1 = center1.Y;
            edge.Graphic.X2 = center2.X;
            edge.Graphic.Y2 = center2.Y;

            edge.Graphic.Redraw();

            if(edge.Icon is not null && edge.Constraint != Constraint.None)
            {
                if(edge.Constraint == Constraint.Parallel)
                {
                    Canvas.SetLeft(edge.Icon, (center1.X + center2.X) / 2 - edge.Icon.Width / 2);
                    Canvas.SetTop(edge.Icon, center1.Y);
                }
                else if(edge.Constraint == Constraint.Perpendicular)
                {
                    Canvas.SetLeft(edge.Icon, center1.X);
                    Canvas.SetTop(edge.Icon, (center1.Y + center2.Y) / 2 - edge.Icon.Width / 2);
                }
            }
        }
    }
}
