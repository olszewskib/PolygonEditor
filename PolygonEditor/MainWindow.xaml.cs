using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using PolygonEditor.Models;

using Polygon = PolygonEditor.Models.Polygon;

namespace PolygonEditor
{
    public partial class MainWindow : Window
    {
        private List<Polygon> polygons = new();
        private bool isPolygonForming = false;
        private int selectedPolygonId = 0;
        public int SelectedPolygonId
        {
            get => selectedPolygonId;
            set
            {
                selectedPolygonId = value;
                offsetCheckBox.Content = FillLabel(value);
                offsetCheckBox.IsChecked = polygons[value].OffsetPolygon is not null;
            }
        }

        // Variables for menuEvents
        private Polygon? polygonToMove;
        private Vertex? menuVertex;
        private Edge? menuEdge;

        // Variables for mouseMoveEvents
        private bool isDragging = false;
        private BresLine? ray = null;
        private Point lastPosition;

        // Constants
        private readonly int iconSize = 20;
        double offset = 40;

        public MainWindow()
        {
            InitializeComponent();
            polygonOneInit();
            offsetCheckBox.Content = FillLabel(selectedPolygonId);
        }

        // General functions
        private string FillLabel(int id) => $"Offset: Polygon {id}";
        private (double,double) GetMousePosition()
        {
            var X = Mouse.GetPosition(mainCanvas).X;
            var Y = Mouse.GetPosition(mainCanvas).Y;
            return (X, Y);
        }

        // Pre Defined Polygon
        private void polygonOneInit()
        {
            var polygon = initPolygon(334, 666);
            addPointToPolygon(454,573, polygon);
            addPointToPolygon(321,486, polygon);
            addPointToPolygon(540,396, polygon);
            addPointToPolygon(521,727, polygon);
            closePolygon(polygon);
        }

        // Constructing a polygons
        private Polygon initPolygon(double x, double y)
        {
            var vertex = new Vertex { Graphic = initPointGraphic() };
            polygons.Add(new Polygon(vertex,vertex));
            vertex.PolygonIndex = Polygon.Id;
            DrawPoint(vertex, x, y);
            var polygon = polygons[Polygon.Id];
            polygon.AddVertex(vertex);
            return polygon;

        }
        private void addPointToPolygon(double x, double y, Polygon polygon)
        {
            var vertex = new Vertex 
            { 
                Graphic = initPointGraphic(),
                PolygonIndex = Polygon.Id
            };
            DrawPoint(vertex,x,y);

            var lastVertex = polygon.LastVertex;
            var edge = new Edge
            {
                Graphic = initEdgeGraphic(lastVertex,vertex),
                Left = lastVertex,
                Right = vertex,
                PolygonIndex = vertex.PolygonIndex
            };
            DrawEdge(edge);

            polygon.AddVertex(vertex);
        }
        private void closePolygon(Polygon polygon)
        {
            var firstVertex = polygon.FirstVertex;
            var lastVertex = polygon.LastVertex;
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
            if (ray is not null)
            {
                mainCanvas.Children.Remove(ray);
                ray = null;
            }
            DrawEdge(edge);

            // make it into a left turn
            polygon.MakePolygonIntoALeftTurn();
        }

        // Graphic initialization
        private Ellipse initPointGraphic(bool noEvents = false)
        {
            Ellipse point = new()
            {
                Width = Vertex.Radius,
                Height = Vertex.Radius,
                StrokeThickness = Vertex.StrokeThickness,
                Stroke = Brushes.Blue
            };

            if (noEvents) return point;
            point.MouseEnter += Point_MouseEnter;
            point.MouseLeave += Point_MouseLeave;
            point.MouseUp += Point_MouseUp;
            point.MouseMove += Point_MouseMove;
            point.MouseDown += Point_MouseDown;

            return point;
        }
        private BresLine initEdgeGraphic(Vertex v1, Vertex v2)
        {
            if (v1.Graphic is null || v2.Graphic is null) throw new Exception("initEdgeGraphicException");

            Point center1 = v1.Center;
            Point center2 = v2.Center;

            var isChecked = isBresenhamCheckBox.IsChecked ?? throw new Exception("CheckboxException: NotFound somehow");

            BresLine edge = new()
            {
                X1 = center1.X,
                Y1 = center1.Y,
                X2 = center2.X,
                Y2 = center2.Y,
                LineColor = Brushes.Black,
                IsBresenham = isChecked,
            };

            edge.MouseEnter += Edge_MouseEnter;
            edge.MouseLeave += Edge_MouseLeave;
            edge.MouseMove += Edge_MouseMove;
            edge.MouseDown += Edge_MouseDown;
            edge.MouseUp += Edge_MouseUp;

            return edge;
        }

        // Context menu initialization
        private bool isParallelValid()
        {
            if (menuEdge is null) throw new Exception("isParallelValidException");
            if (menuEdge.Right is null || menuEdge.Right.RightEdge is null) throw new Exception("isParallelValidException");
            if (menuEdge.Left is null || menuEdge.Left.LeftEdge is null) throw new Exception("isParallelValidException");
            if (menuEdge.Right.RightEdge.Constraint == Constraint.Parallel) return false;
            if (menuEdge.Left.LeftEdge.Constraint == Constraint.Parallel) return false;
            if (menuEdge.Constraint == Constraint.Perpendicular) return false;
            return true;
        }
        private bool isPerpendicularValid()
        {
            if (menuEdge is null) throw new Exception("isPerpendicularValidException");
            if (menuEdge.Right is null || menuEdge.Right.RightEdge is null) throw new Exception("isPerpendicularValidException");
            if (menuEdge.Left is null || menuEdge.Left.LeftEdge is null) throw new Exception("isPerpendicularValidException");
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
            if (isPolygonForming) return;
            if (!isDragging) return;

            if (sender is Ellipse draggedPoint)
            {
                var draggedVertex = Vertex.FindVertex(draggedPoint,polygons) ?? throw new Exception("Dragged point not found somehow");

                Point newPosition = e.GetPosition(mainCanvas);
                Vertex.DragVertex(draggedVertex, newPosition, lastPosition);

                // offset polygon 
                var polygon = polygons[draggedVertex.PolygonIndex];
                if (polygon.OffsetPolygon is not null)
                {
                    RemoveOffset(polygon);
                    AddOffset(polygon);
                }

                lastPosition = newPosition;
            }
        }
        private void Point_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is Ellipse ellipse)
            {
                var existingVertex = Vertex.FindVertex(ellipse, polygons) ?? throw new Exception("Clicked vertex somehow not found");

                // closing a polygon
                if (isPolygonForming)
                {
                    var polygon = polygons[existingVertex.PolygonIndex];
                    if (polygon.Vertices.Count < 3) return;

                    var firstVertex = polygon.FirstVertex;
                    if(existingVertex == firstVertex)
                    {
                        closePolygon(polygon);
                        isPolygonForming = false;
                    }
                    return;
                }

                // constext menu for a vertex, accessible if polygon is not forming
                if (e.ChangedButton == MouseButton.Right)
                {
                    var contextMenu = FindResource("VertexMenu") as ContextMenu;
                    if(contextMenu is not null)
                    {
                        menuVertex = existingVertex;
                        contextMenu.PlacementTarget = ellipse;
                        contextMenu.IsOpen = true;
                    }
                    return;
                }

                // moving an existing vertex, accessible if polygon is not forming
                if (existingVertex.Graphic is null) throw new Exception("MouseDownException: existingVertex.Graphic is null");
                existingVertex.Graphic.CaptureMouse();
                isDragging = true;
                lastPosition = e.GetPosition(mainCanvas);
            }
        }

        // Edges Events
        private void Edge_MouseEnter(object sender, MouseEventArgs e)
        {
            if (isPolygonForming) return;
            isDragging = false;
            if (sender is BresLine hoverLine)
            {
                hoverLine.LineColor = Brushes.Green;
            }
        }
        private void Edge_MouseLeave(object sender, MouseEventArgs e)
        {
            if (isPolygonForming) return;
            if (sender is BresLine hoverLine)
            {
                hoverLine.LineColor = Brushes.Black;
            }
        }
        private void Edge_MouseMove(object sender, MouseEventArgs e)
        {
            if (isPolygonForming) return;
            if (!isDragging) return;

            if (sender is BresLine draggedLine)
            {
                var draggedEdge = Edge.FindEdge(draggedLine,polygons) ?? throw new Exception("Dragged edge not found somehow");

                Point newPosition = e.GetPosition(mainCanvas);

                Edge.DragEdge(draggedEdge,newPosition,lastPosition);

                lastPosition = newPosition;
            }
        }
        private void Edge_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // needed when user clicks on a ray when forming a polygon
            if (isPolygonForming)
            {
                mainCanvas_MouseDown(sender, e);
                return;
            }

            if(e.OriginalSource is BresLine line)
            {
                var edge = Edge.FindEdge(line, polygons) ?? throw new Exception("Edge_MouseDownException: edge not foune");

                // menu option for edge, accessible if polygon is not forming
                if (e.ChangedButton == MouseButton.Right && !isPolygonForming)
                {
                    menuEdge = edge;
                    var contextMenu = initContextMenu();
                    if(contextMenu is not null)
                    {
                        contextMenu.PlacementTarget = line;
                        contextMenu.IsOpen = true;
                    }
                    return;
                }

                // moving an exisitng edge, accessible if polygon is not forming
                if (edge.Graphic is null) throw new Exception("Edge_MouseDownException: edge.Graphic is null somehow");
                edge.Graphic.CaptureMouse();
                isDragging = true;
                lastPosition = e.GetPosition(mainCanvas);
            }
        }
        private void Edge_MouseUp(object sender, MouseEventArgs e)
        {
            if (isPolygonForming) return;
            if (sender is BresLine hoverLine)
            {
                hoverLine.ReleaseMouseCapture();
                isDragging = false;
            }
        }
        
        // Canvas events
        private void mainCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // coordinates of a mouse click
            (var x, var y) = GetMousePosition();

            // moving a polygon
            if (e.ChangedButton == MouseButton.Right)
            {
                if (e.OriginalSource is Canvas canvas)
                {
                    polygonToMove = Polygon.FindPolygon(new Point(x, y), polygons);
                    if (polygonToMove is null) return;

                    SelectedPolygonId = polygonToMove.PolygonId;

                    canvas.CaptureMouse();
                    isDragging = true;
                    lastPosition = e.GetPosition(mainCanvas);
                    return;
                }
            }

            // Check if the click is on the exisitng object
            if (e.OriginalSource is Ellipse || (e.OriginalSource is BresLine && e.Source != ray)) return;

            // init for polygon 
            if(!isPolygonForming)
            {
                initPolygon(x,y);
                isPolygonForming= true;
                return;
            }

            // adding point and edge to polygon
            var polygon = polygons[Polygon.Id];
            addPointToPolygon(x, y, polygon);

        }
        private void mainCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (isPolygonForming)
            {
                Point newPosition = e.GetPosition(mainCanvas);

                var lastVertex = polygons[Polygon.Id].LastVertex;
                var isChecked = isBresenhamCheckBox.IsChecked ?? throw new Exception("CheckboxException: NotFound somehow");

                if (ray is null)
                {
                    BresLine edge = new()
                    {
                        X1 = lastVertex.X + (Vertex.Radius /2),
                        Y1 = lastVertex.Y + (Vertex.Radius /2),
                        X2 = newPosition.X + (Vertex.Radius / 2),
                        Y2 = newPosition.Y + (Vertex.Radius / 2),
                        LineColor = Brushes.Black,
                        IsBresenham = isChecked,
                    };
                    edge.MouseDown += Edge_MouseDown;
                    mainCanvas.Children.Add(edge);
                    ray = edge;
                    return;
                }

                ray.X1 = lastVertex.X + (Vertex.Radius / 2);
                ray.Y1 = lastVertex.Y + (Vertex.Radius / 2);
                ray.X2 = newPosition.X + (Vertex.Radius / 2);
                ray.Y2 = newPosition.Y + (Vertex.Radius / 2);
                ray.Redraw();

                lastPosition = newPosition;
            }

            if (!isDragging) return;

            if (sender is Canvas canvas)
            {
                if (polygonToMove is null) return;

                Point newPosition = e.GetPosition(mainCanvas);

                // moving polygon
                double deltaX = newPosition.X - lastPosition.X;
                double deltaY = newPosition.Y - lastPosition.Y;
                foreach(var vertex in polygonToMove.Vertices)
                {
                    var X = vertex.X;
                    var Y = vertex.Y;
                    Vertex.DragVertex(vertex, new Point(X + deltaX, Y + deltaY), new System.Windows.Point(X, Y), Constraint.All);
                }

                lastPosition = newPosition;
            }
        }
        private void mainCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if(sender is Canvas canvas)
            {
                canvas.ReleaseMouseCapture();
                isDragging = false;
            } 
        }
        
        // Drawing Functions
        private void DrawPoint(Vertex v, double x, double y)
        {
            v.X = x - (Vertex.Radius / 2);
            v.Y = y - (Vertex.Radius / 2);

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
            var leftCenter = left.Center;
            var rightCenter = right.Center;

            // vertex init
            var x = (leftCenter.X + rightCenter.X) / 2;
            var y = (leftCenter.Y + rightCenter.Y) / 2;
            
            var vertex = new Vertex
            {
                Graphic = initPointGraphic(),
                PolygonIndex = menuEdge.PolygonIndex,
                Left = left,
                Right = right,
            };
            DrawPoint(vertex,x,y);
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
            var leftCenter = left.Center;
            var rightCenter = right.Center;

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
            var leftCenter = left.Center;
            var rightCenter = right.Center;

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
        
        // offset
        private void AddOffset(Polygon _polygon)
        {

            // drawing a regular offset with no regards to its corectness
            var polygon = _polygon;
            polygon.SetOffestForEdges(offset);

            bool init = true;
            foreach(var edge in polygon.Edges)
            {
                if (edge.Right is null || edge.Right.RightEdge is null) throw new Exception();
                (var x, var y) = Edge.OffsetIntersectionCoords(edge.Right.RightEdge, edge);

                var vertex = new Vertex { Graphic = initPointGraphic(true) };
                DrawPoint(vertex,x,y);

                if (init)
                {
                    polygon.OffsetPolygon = new Polygon(vertex, vertex);
                    Polygon.Id--;
                    init = false;
                    polygon.OffsetPolygon!.AddVertex(vertex);
                    continue;
                }

                var lastVertex = polygon.OffsetPolygon!.LastVertex;
                var offsetEdge = new Edge
                {
                    Graphic = initEdgeGraphic(lastVertex, vertex),
                    Left = lastVertex,
                    Right = vertex,
                    PolygonIndex = polygon.PolygonId,
                    A = edge.A,
                    B = edge.OffsetB
                };
                polygon.OffsetPolygon.AddEdge(offsetEdge);
                mainCanvas.Children.Add(offsetEdge.Graphic);

                polygon.OffsetPolygon!.AddVertex(vertex);
            }
            polygon.OffsetPolygon!.LastVertex.Right = polygon.OffsetPolygon.FirstVertex;

            var LastVertex = polygon.OffsetPolygon!.LastVertex;
            var FirstVertex = polygon.OffsetPolygon!.FirstVertex;
            var E = polygon.FirstVertex.RightEdge;
            var OffsetEdge = new Edge
            {
                Graphic = initEdgeGraphic(LastVertex, FirstVertex),
                Left = LastVertex,
                Right = FirstVertex,
                PolygonIndex = polygon.PolygonId,
                A = E.A,
                B = E.OffsetB
            };
            polygon.OffsetPolygon.Edges.Insert(0,OffsetEdge);
            mainCanvas.Children.Add(OffsetEdge.Graphic);
            LastVertex.RightEdge = OffsetEdge;
            FirstVertex.LeftEdge = OffsetEdge;


            // finding intersections
            var flag = false;
            foreach(var offsetEdge in polygon.OffsetPolygon.Edges)
            {
                var intersections = Edge.Intersecting(offsetEdge, polygon.OffsetPolygon);
                if (intersections.Count == 0) continue;

                flag = true;
                foreach(var intersection in intersections)
                {
                    var e1 = intersection.Item1;
                    var e2 = intersection.Item2;
                    var point = Edge.IntersectionCoords(e1, e2);

                    var vertex = new Vertex { Graphic = initPointGraphic(true) };
                    vertex.isMarked = true;
                    vertex.Graphic.Stroke = Brushes.Red;

                    DrawPoint(vertex, point.X, point.Y);
                    polygon.OffsetPolygon.Vertices.Add(vertex);
                    e1.AddMarkedVetex(vertex);
                    e1.Mark = vertex;
                }
            }

            // there are no itersections, so retrun
            if (!flag) return;

            // splitting edges with intersection vertices
            var safety = polygon.OffsetPolygon.Vertices.Count;
            var start = polygon.OffsetPolygon.LastVertex.Left;
            start = start.Right;

            while(safety-- > 0)
            {
                if (!start.Right.isMarked && !start.isMarked)
                {
                    start = start.Right;
                    continue;
                } 

                if (!start.isMarked && !start.Right.Right.isMarked)
                {
                    splitEdgeWithVertex(polygon.OffsetPolygon, start.RightEdge, start.Right);
                }
                else if(!start.isMarked && start.Right.Right.isMarked)
                {
                    splitEdgeWithVertex(polygon.OffsetPolygon, start.RightEdge, start.Right);
                    splitEdgeWithVertex(polygon.OffsetPolygon, start.Right.RightEdge, start.Right.Right);
                }

                start = start.Right;
            }

            // removing invalid elements
            var loop = 1;
            safety = polygon.OffsetPolygon.Vertices.Count;
            start = polygon.OffsetPolygon.LastVertex.Left;
            start = start.Right;

            while(safety-- > 0)
            {
                if (start.isMarked)
                {
                    loop++;
                    if(loop % 2 == 0)
                    {
                        mainCanvas.Children.Remove(start.RightEdge.Graphic);
                        if (!start.Right.isMarked) mainCanvas.Children.Remove(start.Right.Graphic);
                    }
                    start = start.Right;
                    continue;
                }

                if(loop % 2 == 1)
                {
                    start = start.Right;
                    continue;
                }

                mainCanvas.Children.Remove(start.RightEdge.Graphic);
                if (!start.Right.isMarked) mainCanvas.Children.Remove(start.Right.Graphic);
                start = start.Right;

            }
            return;
        }
        private void RemoveOffset(Polygon _polygon)
        {
            var polygon = _polygon.OffsetPolygon;
            if (polygon is not null)
            {
                foreach(var edge in polygon.Edges)
                {
                    mainCanvas.Children.Remove(edge.Graphic);
                    if (edge.Mark is not null) mainCanvas.Children.Remove(edge.Mark.Graphic);
                }
                foreach(var point in polygon.Vertices)
                {
                    mainCanvas.Children.Remove(point.Graphic);
                }
                _polygon.OffsetPolygon = null;
            }
        }
        private void offsetSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (polygons.Count == 0) return;
            offset = e.NewValue;
            var polygon = polygons[selectedPolygonId];
            RemoveOffset(polygon);
            AddOffset(polygon);
        }
        private void offsetCheckBox_Clicked(object sender, RoutedEventArgs e)
        {
            if (offsetCheckBox.IsChecked is bool check)
            {
                var polygon = polygons[selectedPolygonId];
                if(check)
                {
                    offsetSlider.IsEnabled = true;
                    AddOffset(polygon);
                }
                else
                {
                    offsetSlider.IsEnabled = false;
                    RemoveOffset(polygon);
                }
            }
        }
        private void splitEdgeWithVertex(Polygon polygon, Edge edge, Vertex vertex)
        {
            // Erase old edge
            mainCanvas.Children.Remove(edge.Graphic);
            mainCanvas.Children.Remove(edge.Icon);
            var index = polygon.Edges.IndexOf(edge);
            polygon.Edges.Remove(edge);

            var left = edge.Left ?? throw new Exception("SplitException: left end is null");
            var right = edge.Right ?? throw new Exception("SplitException: right end is null");
            var polygonIndex = edge.PolygonIndex;

            var leftEdge = new Edge
            {
                Graphic = initEdgeGraphic(left, vertex),
                Left = left,
                Right = vertex,
                PolygonIndex = polygonIndex,
            };
            polygon.Edges.Insert(index, leftEdge);
            mainCanvas.Children.Add(leftEdge.Graphic);

            var rightEdge = new Edge
            {
                Graphic = initEdgeGraphic(vertex,right),
                Left = vertex,
                Right = right,
                PolygonIndex = polygonIndex,
            };
            polygon.Edges.Insert(index + 1, rightEdge);
            mainCanvas.Children.Add(rightEdge.Graphic);

            left.Right = vertex;
            right.Left = vertex;

            left.RightEdge = leftEdge;
            right.LeftEdge = rightEdge;

            vertex.LeftEdge = leftEdge;
            vertex.RightEdge = rightEdge;

        }
    }
    
}