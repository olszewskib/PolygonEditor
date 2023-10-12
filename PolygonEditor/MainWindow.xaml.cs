using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PolygonEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int _pointRadius = 7;

        private bool isDragging = false;
        private Point lastPosition;

        private List<Polygon> polygons = new();



        public MainWindow()
        {
            InitializeComponent();
        }
        private Ellipse initPointGraphic()
        {
            Ellipse point = new()
            {
                Width = _pointRadius,
                Height = _pointRadius,
                StrokeThickness = 4,
                Stroke = Brushes.Blue
            };

            point.MouseEnter += Point_MouseEnter;
            point.MouseLeave += Point_MouseLeave;
            point.MouseUp += Point_MouseUp;
            point.MouseMove += Point_MouseMove;
            
            return point;
        }
        private (double,double) GetMousePosition()
        {
            var X = Mouse.GetPosition(mainCanvas).X;
            var Y = Mouse.GetPosition(mainCanvas).Y;
            return (X, Y);
        }
        private void DrawPoint(Vertex v)
        {

            Canvas.SetLeft(v.Graphic, v.X-(_pointRadius/2));
            Canvas.SetTop(v.Graphic, v.Y-(_pointRadius/2));

            mainCanvas.Children.Add(v.Graphic);
        }
        private Vertex? FindPoint(Ellipse point)
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
        private void DragVertex(Vertex vertex, Point newPosition)
        {
            double deltaX = newPosition.X - lastPosition.X;
            double deltaY = newPosition.Y - lastPosition.Y;

            vertex.X = Canvas.GetLeft(vertex.Graphic) + deltaX;
            vertex.Y = Canvas.GetTop(vertex.Graphic) + deltaY;

            Canvas.SetLeft(vertex.Graphic, Canvas.GetLeft(vertex.Graphic) + deltaX);
            Canvas.SetTop(vertex.Graphic, Canvas.GetTop(vertex.Graphic) + deltaY);

            if (vertex.Left is null || vertex.Right is null) throw new Exception("VertexDraggingException: null vertices");
            if (vertex.LeftEdge is null || vertex.RightEdge is null) throw new Exception("VertexDraggingException: null edges");

            RedrawEdge(vertex.LeftEdge, vertex.Left, vertex);
            RedrawEdge(vertex.RightEdge, vertex, vertex.Right);
        }

        // Clean Code up from here

        private void Point_MouseEnter(object sender, MouseEventArgs e)
        {
            isDragging = false;
            Ellipse? hoverPoint = sender as Ellipse;
            if(hoverPoint != null)
            {
                hoverPoint.Stroke = Brushes.Red;
            }
        }
        private void Point_MouseLeave(object sender, MouseEventArgs e)
        {
            Ellipse? hoverPoint = sender as Ellipse;
            if(hoverPoint != null)
            {
                hoverPoint.Stroke = Brushes.Blue;
            }
        }
        private void Point_MouseUp(object sender, MouseEventArgs e)
        {
            Ellipse? hoverPoint = sender as Ellipse;
            if(hoverPoint != null)
            {
                hoverPoint.ReleaseMouseCapture();
                isDragging= false;
            }
        }
        private void Point_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isDragging) return;

            Ellipse? draggedPoint = sender as Ellipse;
            if (draggedPoint != null)
            {
                var draggedVertex = FindPoint(draggedPoint) ?? throw new Exception("Dragged point not found somehow");

                Point newPosition = e.GetPosition(mainCanvas);
                DragVertex(draggedVertex, newPosition);

                lastPosition = newPosition;
            }

        }


        // variables that make sense
        private bool isPolygonForming = false;
        private void mainCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // checking if point already exists
            if (e.OriginalSource is Ellipse ellipse)
            {
                var existingVertex = FindPoint(ellipse) ?? throw new Exception("Clicked vertex somehow not found");
                
                if (isPolygonForming)
                {
                    var firstVertex = polygons[existingVertex.PolygonIndex].FirstVertex;
                    if(existingVertex == firstVertex)
                    {
                        var lastVertex = polygons[existingVertex.PolygonIndex].LastVertex;
                        var edge = new Edge
                        {
                            Graphic = initEdgeGraphic(lastVertex, firstVertex),
                            v1 = lastVertex,
                            v2 = firstVertex,
                            PolygonIndex = lastVertex.PolygonIndex
                        };

                        firstVertex.LeftEdge = edge;
                        firstVertex.Left = lastVertex;
                        lastVertex.Right = firstVertex;
                        DrawEdge(edge);
                        isPolygonForming = false;
                    }
                    return;

                }

                existingVertex.Graphic.CaptureMouse();
                isDragging = true;
                lastPosition = e.GetPosition(mainCanvas);
                return;
            }

            // coordinates of a mouse click
            (var X, var Y) = GetMousePosition();

            // vertex init
            var vertex = new Vertex
            {
                Graphic = initPointGraphic(),
                X = X - (_pointRadius / 2),
                Y = Y - (_pointRadius / 2),
            };

            // init for forming a polygon 
            if(!isPolygonForming)
            {
                polygons.Add(new Polygon(vertex,vertex));
                isPolygonForming= true;
            }
            vertex.PolygonIndex = Polygon.Id;

            DrawPoint(vertex);

            // drawing an edge
            if (polygons[Polygon.Id].Vertices.Count != 0)
            {
                var lastVertex = polygons[Polygon.Id].LastVertex;
                var edge = new Edge
                {
                    Graphic = initEdgeGraphic(lastVertex,vertex),
                    v1 = lastVertex,
                    v2 = vertex,
                    PolygonIndex = vertex.PolygonIndex
                };

                DrawEdge(edge);
            }

            // drawing a point
            polygons[Polygon.Id].AddVertex(vertex);
        }

        private Line initEdgeGraphic(Vertex v1, Vertex v2)
        {
            Point center1 = new(Canvas.GetLeft(v1.Graphic) + v1.Graphic.Width / 2, Canvas.GetTop(v1.Graphic) + v1.Graphic.Height / 2);
            Point center2 = new(Canvas.GetLeft(v2.Graphic) + v2.Graphic.Width / 2, Canvas.GetTop(v2.Graphic) + v2.Graphic.Height / 2);

            Line edge = new()
            {
                X1 = center1.X,
                Y1 = center1.Y,
                X2 = center2.X,
                Y2 = center2.Y,
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };

            return edge;
        }

        private void RedrawEdge(Edge edge, Vertex v1, Vertex v2)
        {
            Point center1 = new(Canvas.GetLeft(v1.Graphic) + v1.Graphic.Width / 2, Canvas.GetTop(v1.Graphic) + v1.Graphic.Height / 2);
            Point center2 = new(Canvas.GetLeft(v2.Graphic) + v2.Graphic.Width / 2, Canvas.GetTop(v2.Graphic) + v2.Graphic.Height / 2);

            if (edge.Graphic == null) throw new Exception("Redrawing Edge");

            edge.Graphic.X1 = center1.X;
            edge.Graphic.Y1 = center1.Y;
            edge.Graphic.X2 = center2.X;
            edge.Graphic.Y2 = center2.Y;

        }

        private void DrawEdge(Edge e)
        {
            polygons[Polygon.Id].AddEdge(e);
            mainCanvas.Children.Add(e.Graphic);
        }
    }
}
