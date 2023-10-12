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
    internal class Edge
    {
        public int PolygonIndex { get; set; }
        public Vertex? Left { get; set; }
        public Vertex? Right { get; set; }
        public Line? Graphic { get; set; }
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

            double deltaX = newPosition.X - lastPosition.X;
            double deltaY = newPosition.Y - lastPosition.Y;

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

        }
    }
}
