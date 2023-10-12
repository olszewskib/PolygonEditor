using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PolygonEditor
{
    internal class Vertex
    {
        public static int VertexRadius = 7;
        public int PolygonIndex {  get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public Vertex? Right { get; set; }
        public Edge? RightEdge {  get; set; }
        public Vertex? Left { get; set; }
        public Edge? LeftEdge {  get; set; }
        public Ellipse? Graphic { get; set; }
        public static Vertex? FindVertex(Ellipse point, List<Polygon> polygons)
        {
            foreach(var polygon in polygons)
            {
                foreach(var vertex in polygon.Vertices)
                {
                    if (point == vertex.Graphic) return vertex;
                }
            }
            return null;

        }
        public static void DragVertex(Vertex vertex, System.Windows.Point newPosition, System.Windows.Point lastPosition)
        {
            double deltaX = newPosition.X - lastPosition.X;
            double deltaY = newPosition.Y - lastPosition.Y;

            vertex.X = Canvas.GetLeft(vertex.Graphic) + deltaX;
            vertex.Y = Canvas.GetTop(vertex.Graphic) + deltaY;

            Canvas.SetLeft(vertex.Graphic, Canvas.GetLeft(vertex.Graphic) + deltaX);
            Canvas.SetTop(vertex.Graphic, Canvas.GetTop(vertex.Graphic) + deltaY);

            if (vertex.Left is null || vertex.Right is null) throw new Exception("VertexDraggingException: null vertices");
            if (vertex.LeftEdge is null || vertex.RightEdge is null) throw new Exception("VertexDraggingException: null edges");

            Edge.RedrawEdge(vertex.LeftEdge, vertex.Left, vertex);
            Edge.RedrawEdge(vertex.RightEdge, vertex, vertex.Right);
        }
    }
}
