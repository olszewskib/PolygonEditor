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

        private int _vertexCount = 0;
        private bool _isPolygonForming = false;
        private int _polygonIndex => polygons.Count - 1;
        private List<Polygon> polygons = new();
        private Vertex? _lastVertex;



        public MainWindow()
        {
            InitializeComponent();
        }

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
        private void mainCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            
            if(_vertexCount == 0)
            {
                polygons.Add(new Polygon());
                _isPolygonForming= true;
            }
            _vertexCount++;
            
            // coordinates of a mouse click
            var X = Mouse.GetPosition(mainCanvas).X;
            var Y = Mouse.GetPosition(mainCanvas).Y;

            if (e.OriginalSource is Ellipse)
            {
                Ellipse existingPoint = (Ellipse)e.OriginalSource;
                if(_isPolygonForming)
                {
                    if(existingPoint == polygons[_polygonIndex].FirstVertex.Graphic)
                    {
                        DrawEdge(polygons[_polygonIndex].FirstVertex);
                        _isPolygonForming = false;
                        _vertexCount = 0;
                    }
                    return;

                }

                existingPoint.CaptureMouse();
                isDragging = true;
                lastPosition = e.GetPosition(mainCanvas);
                return;
            }

            var point = initPointGraphic();

            var v = new Vertex
            {
                Graphic = point,
                X = X - (_pointRadius / 2),
                Y = Y - (_pointRadius / 2)
            };

            Canvas.SetLeft(point, X-(_pointRadius/2));
            Canvas.SetTop(point, Y-(_pointRadius/2));

            mainCanvas.Children.Add(point);

            if (polygons[_polygonIndex].Vertices.Count !=0)
            {
                DrawEdge(v);
            }

            
            try
            {
                polygons[_polygonIndex].AddVertex(v);
            }
            catch
            {
                throw new Exception("Invalid Polygon index exception");
            }



        }

        private void DrawEdge(Vertex v)
        {
            var lastVertex = polygons[_polygonIndex].LastVertex;

            Point center1 = new Point(Canvas.GetLeft(lastVertex.Graphic) + lastVertex.Graphic.Width / 2, Canvas.GetTop(lastVertex.Graphic) + lastVertex.Graphic.Height / 2);
            Point center2 = new Point(Canvas.GetLeft(v.Graphic) + v.Graphic.Width / 2, Canvas.GetTop(v.Graphic) + v.Graphic.Height / 2);

            Line line = new Line
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
