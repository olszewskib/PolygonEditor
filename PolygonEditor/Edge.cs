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
        public int PolygonIndex { get; set; }
        public Vertex? Left { get; set; }
        public Vertex? Right { get; set; }
        public Line? Graphic { get; set; }
        public Image? Icon { get; set; }
        public Constraint Constraint { get; set; } = Constraint.None;
        public static Edge? FindEdge(Line line, List<Polygon> polygons)
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

            double deltaX = 0;
            double deltaY = 0;
            
            if (edge.Left.LeftEdge.Constraint == Constraint.None && edge.Right.RightEdge.Constraint == Constraint.None)
            {
                deltaX = newPosition.X - lastPosition.X;
                deltaY = newPosition.Y - lastPosition.Y;
            }
            if (edge.Left.LeftEdge.Constraint == Constraint.Parallel || edge.Right.RightEdge.Constraint == Constraint.Parallel)
            {
                deltaX = newPosition.X - lastPosition.X;
            }
            if (edge.Left.LeftEdge.Constraint == Constraint.Perpendicular || edge.Right.RightEdge.Constraint == Constraint.Perpendicular)
            {
                deltaY = newPosition.Y - lastPosition.Y;
            }


            edge.Left.X = Canvas.GetLeft(edge.Left.Graphic) + deltaX;
            edge.Left.Y = Canvas.GetTop(edge.Left.Graphic) + deltaY;

            edge.Right.X = Canvas.GetLeft(edge.Right.Graphic) + deltaX;
            edge.Right.Y = Canvas.GetTop(edge.Right.Graphic) + deltaY;

            Canvas.SetLeft(edge.Left.Graphic, Canvas.GetLeft(edge.Left.Graphic) + deltaX);
            Canvas.SetTop(edge.Left.Graphic, Canvas.GetTop(edge.Left.Graphic) + deltaY);

            Canvas.SetLeft(edge.Right.Graphic, Canvas.GetLeft(edge.Right.Graphic) + deltaX);
            Canvas.SetTop(edge.Right.Graphic, Canvas.GetTop(edge.Right.Graphic) + deltaY);

            RedrawEdge(edge, edge.Left, edge.Right);
            RedrawEdge(edge.Left.LeftEdge, edge.Left.Left, edge.Left);
            RedrawEdge(edge.Right.RightEdge, edge.Right, edge.Right.Right);

        }
        public static void RedrawEdge(Edge edge, Vertex v1, Vertex v2)
        {
            if (v1.Graphic is null || v2.Graphic is null) throw new Exception("RedrawEdgeException: vertex.Graphic is null");

            System.Windows.Point center1 = new(Canvas.GetLeft(v1.Graphic) + v1.Graphic.Width / 2, Canvas.GetTop(v1.Graphic) + v1.Graphic.Height / 2);
            System.Windows.Point center2 = new(Canvas.GetLeft(v2.Graphic) + v2.Graphic.Width / 2, Canvas.GetTop(v2.Graphic) + v2.Graphic.Height / 2);

            if (edge.Graphic == null) throw new Exception("RedrawEdgeException: edgeGraphic is null");

            edge.Graphic.X1 = center1.X;
            edge.Graphic.Y1 = center1.Y;
            edge.Graphic.X2 = center2.X;
            edge.Graphic.Y2 = center2.Y;

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

        public static (System.Windows.Point, System.Windows.Point) getEndpoints(Edge edge)
        {
            var left = edge.Left ?? throw new Exception("ConstraintException: left end is null");
            var right = edge.Right ?? throw new Exception("ConstraintException: left end is null");
            if (left.Graphic is null || right.Graphic is null) throw new Exception("SplitException: graphics are null");
            System.Windows.Point leftCenter = new(Canvas.GetLeft(left.Graphic) + left.Graphic.Width / 2, Canvas.GetTop(left.Graphic) + left.Graphic.Height / 2);
            System.Windows.Point rightCenter = new(Canvas.GetLeft(right.Graphic) + right.Graphic.Width / 2, Canvas.GetTop(right.Graphic) + right.Graphic.Height / 2);
            return (leftCenter, rightCenter);

        }
    }
}
