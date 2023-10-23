using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PolygonEditor.Models
{
    internal class Vertex
    {
        public static int Radius = 10;
        public static int StrokeThickness = 7;
        public int PolygonIndex { get; set; }
        public double X
        {
            get => Canvas.GetLeft(Graphic);
            set => Canvas.SetLeft(Graphic, value);
        }
        public double Y
        {
            get => Canvas.GetTop(Graphic);
            set => Canvas.SetTop(Graphic, value);
        }
        public System.Windows.Point Center => new(X + Radius / 2, Y + Radius / 2);
        public Vertex? Right { get; set; }
        public Edge? RightEdge { get; set; }
        public Vertex? Left { get; set; }
        public Edge? LeftEdge { get; set; }

        public bool isMarked = false;
        public Ellipse? Graphic { get; set; }
        public static Vertex? FindVertex(Ellipse point, List<Polygon> polygons)
        {
            foreach (var polygon in polygons)
            {
                foreach (var vertex in polygon.Vertices)
                {
                    if (point == vertex.Graphic) return vertex;
                }
            }
            return null;
        }
        public static void DragVertex(Vertex vertex, System.Windows.Point newPosition, System.Windows.Point lastPosition, Constraint skip = Constraint.None)
        {
            var deltaX = newPosition.X - lastPosition.X;
            var deltaY = newPosition.Y - lastPosition.Y;

            if (vertex.LeftEdge is null || vertex.RightEdge is null) throw new Exception("DragVertexException: neighbour edges are null");

            vertex.X += deltaX;
            vertex.Y += deltaY;

            if (vertex.Left is null || vertex.Right is null || vertex.Left.Left is null || vertex.Right.Right is null) throw new Exception("VertexDraggingException: null vertices or not a polygon");
            if (vertex.LeftEdge is null || vertex.RightEdge is null || vertex.Left.LeftEdge is null || vertex.Right.RightEdge is null) throw new Exception("VertexDraggingException: null edges");

            if (skip != Constraint.All)
            {
                if (skip != Constraint.Parallel)
                {
                    // Parallel Constraints
                    if (vertex.LeftEdge.Constraint == Constraint.Parallel)
                    {
                        vertex.Left.Y += deltaY;
                        ConnectVertex(vertex.Left.LeftEdge, vertex.Left.Left, vertex.Left);
                    }
                    if (vertex.RightEdge.Constraint == Constraint.Parallel)
                    {
                        vertex.Right.Y += deltaY;
                        ConnectVertex(vertex.Right.RightEdge, vertex.Right, vertex.Right.Right);
                    }

                }
                if (skip != Constraint.Perpendicular)
                {
                    // Perpendicular Constraints
                    if (vertex.LeftEdge.Constraint == Constraint.Perpendicular)
                    {
                        vertex.Left.X += deltaX;
                        ConnectVertex(vertex.Left.LeftEdge, vertex.Left.Left, vertex.Left);
                    }
                    if (vertex.RightEdge.Constraint == Constraint.Perpendicular)
                    {
                        vertex.Right.X += deltaX;
                        ConnectVertex(vertex.Right.RightEdge, vertex.Right, vertex.Right.Right);
                    }
                }
            }

            ConnectVertex(vertex.LeftEdge, vertex.Left, vertex);
            ConnectVertex(vertex.RightEdge, vertex, vertex.Right);
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

            if (edge.Icon is not null && edge.Constraint != Constraint.None)
            {
                if (edge.Constraint == Constraint.Parallel)
                {
                    Canvas.SetLeft(edge.Icon, (center1.X + center2.X) / 2 - edge.Icon.Width / 2);
                    Canvas.SetTop(edge.Icon, center1.Y);
                }
                else if (edge.Constraint == Constraint.Perpendicular)
                {
                    Canvas.SetLeft(edge.Icon, center1.X);
                    Canvas.SetTop(edge.Icon, (center1.Y + center2.Y) / 2 - edge.Icon.Width / 2);
                }
            }
        }
        public static void RemoveVertex(Vertex vertex, List<Polygon> polygons)
        {
            var right = vertex.Right ?? throw new Exception("RemoveVertexException: no right neighbour");
            var left = vertex.Left ?? throw new Exception("RemoveVertexException: no left neighbour");
            var polygon = polygons[vertex.PolygonIndex];

            // removing vertex
            right.Left = left;
            left.Right = right;
            polygon.Vertices.Remove(vertex);

            // removing edges
            if (vertex.LeftEdge is null || vertex.RightEdge is null) throw new Exception("RemoveVertexException: null edges");
            polygon.Edges.Remove(vertex.LeftEdge);
            polygon.Edges.Remove(vertex.RightEdge);
        }
    }
}
