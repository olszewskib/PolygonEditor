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
        private void DrawPoint(Ellipse point,double X, double Y)
        {
            Canvas.SetLeft(point, X-(_pointRadius/2));
            Canvas.SetTop(point, Y-(_pointRadius/2));

            mainCanvas.Children.Add(point);
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
                Point newPosition = e.GetPosition(mainCanvas);

                double deltaX = newPosition.X - lastPosition.X;
                double deltaY = newPosition.Y - lastPosition.Y;

                Canvas.SetLeft(draggedPoint, Canvas.GetLeft(draggedPoint) + deltaX);
                Canvas.SetTop(draggedPoint, Canvas.GetTop(draggedPoint) + deltaY);

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
                    if(existingVertex == polygons[existingVertex.PolygonIndex].FirstVertex)
                    {
                        DrawEdge(polygons[existingVertex.PolygonIndex].FirstVertex);
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

            // adding new point
            var point = initPointGraphic();

            // vertex init
            var v = new Vertex
            {
                Graphic = point,
                X = X - (_pointRadius / 2),
                Y = Y - (_pointRadius / 2),
            };

            // init for forming a polygon 
            if(!isPolygonForming)
            {
                polygons.Add(new Polygon(v,v));
                isPolygonForming= true;
            }
            v.PolygonIndex = Polygon.Id;

            DrawPoint(point, X, Y);

            if (polygons[v.PolygonIndex].Vertices.Count != 0)
            {
                DrawEdge(v);
            }

            
            polygons[v.PolygonIndex].AddVertex(v);

        }


        private void DrawEdge(Vertex v)
        {
            var lastVertex = polygons[v.PolygonIndex].LastVertex;

            // trzeba przenienieść do AddVertex w Polygon tutaj to troche nie ma sensu
            lastVertex.Right = v;
            v.Left = lastVertex;

            Point center1 = new(Canvas.GetLeft(lastVertex.Graphic) + lastVertex.Graphic.Width / 2, Canvas.GetTop(lastVertex.Graphic) + lastVertex.Graphic.Height / 2);
            Point center2 = new(Canvas.GetLeft(v.Graphic) + v.Graphic.Width / 2, Canvas.GetTop(v.Graphic) + v.Graphic.Height / 2);

            Line line = new()
            {
                X1 = center1.X,
                Y1 = center1.Y,
                X2 = center2.X,
                Y2 = center2.Y,
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };

            mainCanvas.Children.Add(line);

        }
    }
}
