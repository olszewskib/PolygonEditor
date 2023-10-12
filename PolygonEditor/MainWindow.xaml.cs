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
        private bool isDragging = false;
        private Point lastPosition;

        // variables that make sense
        private bool isPolygonForming = false;
        private List<Polygon> polygons = new();



        public MainWindow()
        {
            InitializeComponent();
        }
        
        // General functions
        private (double,double) GetMousePosition()
        {
            var X = Mouse.GetPosition(mainCanvas).X;
            var Y = Mouse.GetPosition(mainCanvas).Y;
            return (X, Y);
        }

        // Graphic initialization
        private Ellipse initPointGraphic()
        {
            Ellipse point = new()
            {
                Width = Vertex.VertexRadius,
                Height = Vertex.VertexRadius,
                StrokeThickness = 4,
                Stroke = Brushes.Blue
            };

            point.MouseEnter += Point_MouseEnter;
            point.MouseLeave += Point_MouseLeave;
            point.MouseUp += Point_MouseUp;
            point.MouseMove += Point_MouseMove;
            point.MouseDown += Point_MouseDown;
            
            return point;
        }
        private Line initEdgeGraphic(Vertex v1, Vertex v2)
        {
            if (v1.Graphic is null || v2.Graphic is null) throw new Exception("initEdgeGraphicException");

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

            edge.MouseEnter += Edge_MouseEnter;
            edge.MouseLeave += Edge_MouseLeave;
            edge.MouseMove += Edge_MouseMove;
            edge.MouseDown += Edge_MouseDown;
            edge.MouseUp += Edge_MouseUp;

            return edge;
        }




        // Point events
        private void Point_MouseEnter(object sender, MouseEventArgs e)
        {
            isDragging = false;
            if (sender is Ellipse hoverPoint)
            {
                hoverPoint.Stroke = Brushes.Red;
            }
        }
        private void Point_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Ellipse hoverPoint)
            {
                hoverPoint.Stroke = Brushes.Blue;
            }
        }
        private void Point_MouseUp(object sender, MouseEventArgs e)
        {
            if (sender is Ellipse hoverPoint)
            {
                hoverPoint.ReleaseMouseCapture();
                isDragging = false;
            }
        }
        private void Point_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isDragging) return;

            if (sender is Ellipse draggedPoint)
            {
                var draggedVertex = Vertex.FindVertex(draggedPoint,polygons) ?? throw new Exception("Dragged point not found somehow");

                Point newPosition = e.GetPosition(mainCanvas);
                Vertex.DragVertex(draggedVertex, newPosition, lastPosition);

                lastPosition = newPosition;
            }

        }
        private void Point_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.OriginalSource is Ellipse ellipse)
            {
                var existingVertex = Vertex.FindVertex(ellipse, polygons) ?? throw new Exception("Clicked vertex somehow not found");
                
                if (isPolygonForming)
                {
                    var firstVertex = polygons[existingVertex.PolygonIndex].FirstVertex;
                    if(existingVertex == firstVertex)
                    {
                        var lastVertex = polygons[existingVertex.PolygonIndex].LastVertex;
                        var edge = new Edge
                        {
                            Graphic = initEdgeGraphic(lastVertex, firstVertex),
                            Left = lastVertex,
                            Right = firstVertex,
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

                if (existingVertex.Graphic is null) throw new Exception("MouseDownException: existingVertex.Graphic is null");
                existingVertex.Graphic.CaptureMouse();
                isDragging = true;
                lastPosition = e.GetPosition(mainCanvas);
                return;
            }

        }

        // Edges Events
        private void Edge_MouseEnter(object sender, MouseEventArgs e)
        {
            isDragging = false;
            if (sender is Line hoverLine)
            {
                hoverLine.Stroke = Brushes.Green;
            }
        }
        private void Edge_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Line hoverLine)
            {
                hoverLine.Stroke = Brushes.Black;
            }
        }
        private void Edge_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isDragging) return;

            if (sender is Line draggedLine)
            {
                var draggedEdge = Edge.FindEdge(draggedLine,polygons) ?? throw new Exception("Dragged edge not found somehow");

                Point newPosition = e.GetPosition(mainCanvas);

                Edge.DragEdge(draggedEdge,newPosition,lastPosition);

                lastPosition = newPosition;
            }

        }
        private void Edge_MouseDown(object sender, MouseEventArgs e)
        {
            if(e.OriginalSource is Line line)
            {
                var edge = Edge.FindEdge(line, polygons) ?? throw new Exception("Edge_MouseDownException: edge not foune");
                if (edge.Graphic is null) throw new Exception("Edge_MouseDownException: edge.Graphic is null somehow");
                edge.Graphic.CaptureMouse();
                isDragging = true;
                lastPosition = e.GetPosition(mainCanvas);
            }
        }
        private void Edge_MouseUp(object sender, MouseEventArgs e)
        {
            if (sender is Line hoverLine)
            {
                hoverLine.ReleaseMouseCapture();
                isDragging = false;
            }

        }
        
        // Canvas events
        private void mainCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // checking if point already exists

            if (e.OriginalSource is Line || e.OriginalSource is Ellipse) return;

            // coordinates of a mouse click
            (var X, var Y) = GetMousePosition();

            // vertex init
            var vertex = new Vertex
            {
                Graphic = initPointGraphic(),
                X = X - (Vertex.VertexRadius / 2),
                Y = Y - (Vertex.VertexRadius / 2),
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
                    Left = lastVertex,
                    Right = vertex,
                    PolygonIndex = vertex.PolygonIndex
                };

                DrawEdge(edge);
            }

            // drawing a point
            polygons[Polygon.Id].AddVertex(vertex);
        }
        private void DrawPoint(Vertex v)
        {
            Canvas.SetLeft(v.Graphic, v.X-(Vertex.VertexRadius /2));
            Canvas.SetTop(v.Graphic, v.Y-(Vertex.VertexRadius /2));

            mainCanvas.Children.Add(v.Graphic);
        }
        private void DrawEdge(Edge e)
        {
            polygons[Polygon.Id].AddEdge(e);
            mainCanvas.Children.Add(e.Graphic);
        }
    }
}
