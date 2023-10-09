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

        private List<Ellipse> _points = new();

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
        private void mainCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {

            // coordinates of a mouse click
            var X = Mouse.GetPosition(mainCanvas).X;
            var Y = Mouse.GetPosition(mainCanvas).Y;

            if(e.OriginalSource is Ellipse)
            {
                Ellipse draggedPoint = (Ellipse)e.OriginalSource;
                draggedPoint.CaptureMouse();
                isDragging = true;
                lastPosition = e.GetPosition(mainCanvas);
                return;
            }


            Ellipse point = new Ellipse
            {
                Width = _pointRadius,
                Height = _pointRadius,
                StrokeThickness = 3,
                Stroke = Brushes.Blue
            };

            point.MouseEnter += Point_MouseEnter;
            point.MouseLeave += Point_MouseLeave;
            point.MouseUp += Point_MouseUp;
            point.MouseMove += Point_MouseMove;

            _points.Add(point);

            Canvas.SetLeft(point, X-(point.Width/2));
            Canvas.SetTop(point, Y-(point.Height/2));

            mainCanvas.Children.Add(point);

        }
    }
}
