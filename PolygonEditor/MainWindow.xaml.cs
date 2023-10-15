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
        private int iconSize = 20;

        // variables that make sense
        private bool isPolygonForming = false;
        private List<Polygon> polygons = new();

        // Variables for menu events
        private Vertex? menuVertex;
        private Edge? menuEdge;

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

        private bool isParallelValid()
        {
            if (menuEdge is null) throw new Exception("isParallelValidException");
            if (menuEdge.Right.RightEdge.Constraint == Constraint.Parallel) return false;
            if (menuEdge.Left.LeftEdge.Constraint == Constraint.Parallel) return false;
            if (menuEdge.Constraint == Constraint.Perpendicular) return false;
            return true;
        }
        private bool isPerpendicularValid()
        {
            if (menuEdge is null) throw new Exception("isPerpendicularValidException");
            if (menuEdge.Right.RightEdge.Constraint == Constraint.Perpendicular) return false;
            if (menuEdge.Left.LeftEdge.Constraint == Constraint.Perpendicular) return false;
            if (menuEdge.Constraint == Constraint.Parallel) return false;
            return true;
        }
        private ContextMenu? initContextMenu()
        {
            if (menuEdge is null) return null;

            var contextMenu = new ContextMenu();
            var split = new MenuItem { Header = "Split Edge" };
            split.Click += MenuItem_Click_SplitEdge;

            var parallel = new MenuItem();
            parallel.Header = menuEdge.Constraint == Constraint.Parallel ? "Remove parallel constraint" : "Add parallel constraint";
            parallel.Click += MenuItem_Click_ParallelConstraint;
            parallel.IsEnabled = isParallelValid();

            var perpendicular = new MenuItem();
            perpendicular.Header = menuEdge.Constraint == Constraint.Perpendicular ? "Remove perpendicular constraint" : "Add perpendicular constraint";
            perpendicular.Click += MenuItem_Click_PerpendicularConstraint;
            perpendicular.IsEnabled = isPerpendicularValid();

            contextMenu.Items.Add(split);
            contextMenu.Items.Add(parallel);
            contextMenu.Items.Add(perpendicular);

            return contextMenu;
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
        private void Point_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is Ellipse ellipse)
            {
                var existingVertex = Vertex.FindVertex(ellipse, polygons) ?? throw new Exception("Clicked vertex somehow not found");

                // menu options for a vertex
                if (e.ChangedButton == MouseButton.Right && !isPolygonForming)
                {
                    //Todo add a warning that a polygon is forming if clicked
                    var contextMenu = FindResource("VertexMenu") as ContextMenu;
                    if(contextMenu is not null)
                    {
                        menuVertex = existingVertex;
                        contextMenu.PlacementTarget = ellipse;
                        contextMenu.IsOpen = true;
                    }
                    return;
                }
                
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
        private void Edge_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.OriginalSource is Line line)
            {
                var edge = Edge.FindEdge(line, polygons) ?? throw new Exception("Edge_MouseDownException: edge not foune");

                // menu option for edge
                if (e.ChangedButton == MouseButton.Right && !isPolygonForming)
                {
                    //Todo add a warning that a polygon is forming if clicked
                    menuEdge = edge;
                    var contextMenu = initContextMenu();
                    if(contextMenu is not null)
                    {
                        contextMenu.PlacementTarget = line;
                        contextMenu.IsOpen = true;
                    }
                    return;
                }

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
            // Later this will give options to clear canvas ect
            if (e.ChangedButton == MouseButton.Right) return;

            // Check if the click is on the exisitng object
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
            polygons[e.PolygonIndex].AddEdge(e);
            mainCanvas.Children.Add(e.Graphic);
        }

        // MenuItems events
        private void MenuItem_Click_RemoveVertex(object sender, RoutedEventArgs e)
        {
            //Todo if user wants to remove a vertex from a triangel dont let him or delete the whole polygon
            
            if (menuVertex is null) return;
            if (menuVertex.LeftEdge is null || menuVertex.RightEdge is null) throw new Exception("VertexRemovalException: null edges");

            mainCanvas.Children.Remove(menuVertex.RightEdge.Graphic);
            mainCanvas.Children.Remove(menuVertex.LeftEdge.Graphic);
            mainCanvas.Children.Remove(menuVertex.Graphic);
            if (menuVertex.LeftEdge.Icon is Image leftIcon) mainCanvas.Children.Remove(leftIcon);
            if (menuVertex.RightEdge.Icon is Image rightIcon) mainCanvas.Children.Remove(rightIcon);

            Vertex.RemoveVertex(menuVertex, polygons);
            // add missing edge!

            if (menuVertex.Left is null || menuVertex.Right is null) throw new Exception("VertexRemovalException: null neighbours");
            var edge = new Edge
            {
                Graphic = initEdgeGraphic(menuVertex.Left, menuVertex.Right),
                Left = menuVertex.Left,
                Right = menuVertex.Right,
                PolygonIndex = menuVertex.PolygonIndex
            };
            DrawEdge(edge);

            menuVertex.Right.LeftEdge = edge;
            menuVertex.Left.RightEdge = edge;

        }

        private void MenuItem_Click_SplitEdge(object sender, RoutedEventArgs e)
        {
            if (menuEdge is null) return;

            // Erase old edge
            mainCanvas.Children.Remove(menuEdge.Graphic);
            mainCanvas.Children.Remove(menuEdge.Icon);

            var left = menuEdge.Left ?? throw new Exception("SplitException: left end is null");
            var right = menuEdge.Right ?? throw new Exception("SplitException: right end is null");
            (var leftCenter, var rightCenter) = Edge.getEndpoints(menuEdge);

            // vertex init
            var vertex = new Vertex
            {
                Graphic = initPointGraphic(),
                X = (leftCenter.X + rightCenter.X)/2,
                Y = (leftCenter.Y + rightCenter.Y) / 2,
                PolygonIndex = menuEdge.PolygonIndex,
                Left = left,
                Right = right,
            };
            DrawPoint(vertex);
            polygons[vertex.PolygonIndex].Vertices.Add(vertex);

            var leftEdge = new Edge
            {
                Graphic = initEdgeGraphic(menuEdge.Left, vertex),
                Left = menuEdge.Left,
                Right = vertex,
                PolygonIndex = menuEdge.PolygonIndex,
            };
            DrawEdge(leftEdge);

            var rightEdge = new Edge
            {
                Graphic = initEdgeGraphic(vertex,menuEdge.Right),
                Left = vertex,
                Right = menuEdge.Right,
                PolygonIndex = menuEdge.PolygonIndex
            };
            DrawEdge(rightEdge);

            left.Right = vertex;
            right.Left = vertex;

            left.RightEdge = leftEdge;
            right.LeftEdge = rightEdge;

            vertex.LeftEdge = leftEdge;
            vertex.RightEdge = rightEdge;
        }

        private void MenuItem_Click_ParallelConstraint(object sender, RoutedEventArgs e)
        {
            if (menuEdge is null) return;

            if(menuEdge.Constraint == Constraint.Parallel)
            {
                menuEdge.Constraint = Constraint.None;
                if(menuEdge.Icon is not null)
                {
                    mainCanvas.Children.Remove(menuEdge.Icon);
                    menuEdge.Icon = null;
                }
                return;
            }

            var left = menuEdge.Left ?? throw new Exception("ConstraintException: left end is null");
            var right = menuEdge.Right ?? throw new Exception("ConstraintException: left end is null");
            (var leftCenter, var rightCenter) = Edge.getEndpoints(menuEdge);

            var vertexToMove = leftCenter.Y <= rightCenter.Y ? (left,leftCenter) : (right,rightCenter);
            var delta = Math.Abs(leftCenter.Y - rightCenter.Y);
            var newPosition = new System.Windows.Point(vertexToMove.Item2.X, vertexToMove.Item2.Y + delta);

            Vertex.DragVertex(vertexToMove.Item1, newPosition, vertexToMove.Item2);

            menuEdge.Constraint = Constraint.Parallel;

            var icon = new Image()
            {
                Source = new BitmapImage(new Uri("Resources/horizontal.png", UriKind.Relative)),
                Width = iconSize,
                Height = iconSize,
            };

            Canvas.SetLeft(icon, ((leftCenter.X + rightCenter.X) / 2) - iconSize/2);
            Canvas.SetTop(icon, newPosition.Y);
            mainCanvas.Children.Add(icon);
            menuEdge.Icon = icon;

            //if(e.OriginalSource is MenuItem item)
            //{
            //    item.IsChecked = true;
            //}
        }
        private void MenuItem_Click_PerpendicularConstraint(object sender, RoutedEventArgs e)
        {
            if (menuEdge is null) return;

            if(menuEdge.Constraint == Constraint.Perpendicular)
            {
                menuEdge.Constraint = Constraint.None;
                if(menuEdge.Icon is not null)
                {
                    mainCanvas.Children.Remove(menuEdge.Icon);
                    menuEdge.Icon = null;
                }
                return;
            }

            var left = menuEdge.Left ?? throw new Exception("ConstraintException: left end is null");
            var right = menuEdge.Right ?? throw new Exception("ConstraintException: left end is null");
            (var leftCenter, var rightCenter) = Edge.getEndpoints(menuEdge);

            var vertexToMove = leftCenter.X <= rightCenter.X ? (left,leftCenter) : (right,rightCenter);
            var delta = Math.Abs(leftCenter.X - rightCenter.X);
            var newPosition = new System.Windows.Point(vertexToMove.Item2.X + delta, vertexToMove.Item2.Y);

            Vertex.DragVertex(vertexToMove.Item1, newPosition, vertexToMove.Item2);

            menuEdge.Constraint = Constraint.Perpendicular;

            var icon = new Image()
            {
                Source = new BitmapImage(new Uri("Resources/vertical.png", UriKind.Relative)),
                Width = iconSize,
                Height = iconSize
            };

            Canvas.SetLeft(icon, newPosition.X);
            Canvas.SetTop(icon, ((leftCenter.Y + rightCenter.Y) / 2) - iconSize/2);
            mainCanvas.Children.Add(icon);
            menuEdge.Icon = icon;
        }
    }
}