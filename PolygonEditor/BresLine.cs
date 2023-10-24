using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using PolygonEditor.Models;

namespace PolygonEditor
{
    internal class BresLine : UIElement
    {
        public int UpperLimitX = 3840;
        public int UpperLimitY = 2160;
        public double X1 { get; set; }
        public double Y1 { get; set; }
        public double X2 { get; set; }
        public double Y2 { get; set; }

        private bool isBresengam = false;
        public bool IsBresenham
        {
            get { return isBresengam; }
            set
            {
                isBresengam = value;
                this.Redraw();
            }
        }

        public bool isSymetricBresenham = false;
        public bool IsSymetricBresenham
        {
            get { return isSymetricBresenham;  }
            set
            {
                isSymetricBresenham = value;
                this.Redraw();
            }
        }

        private Brush lineColor = Brushes.Red;
        public Brush LineColor
        {
            get { return lineColor; }
            set
            {
                lineColor = value;
                this.Redraw();
            }
        }

        private int lineWidth = Edge.Width;
        public int LineWidth
        {
            get { return lineWidth; }
            set
            {
                lineWidth = value;
                this.Redraw();
            }

        }
        public BresLine()
        {
            
        }
        public void Redraw()
        {
            this.InvalidateVisual();
        }
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            if (!isBresengam)
            {
                Pen line = new Pen(lineColor, lineWidth);
                drawingContext.DrawLine(line,new Point(X1,Y1), new Point(X2,Y2));
            }
            else if (isBresengam)
            {
                Pen pen = new Pen(lineColor, 2);
                int x1 = X1 > UpperLimitX ? UpperLimitX : (int)X1;
                int y1 = Y1 > UpperLimitY ? UpperLimitY : (int)Y1;
                int x2 = X2 > UpperLimitX ? UpperLimitX : (int)X2;
                int y2 = Y2 > UpperLimitY ? UpperLimitY : (int)Y2;

                int dx = Math.Abs(x2 - x1);
                int dy = Math.Abs(y2 - y1);
                int sx = x1 < x2 ? 1 : -1;
                int sy = y1 < y2 ? 1 : -1;
                int err = dx - dy;

                while (true)
                {
                    drawingContext.DrawRectangle(pen.Brush, pen, new Rect(x1, y1, 1, lineWidth));

                    if (x1 == x2 && y1 == y2)
                        break;

                    int e2 = 2 * err;
                    if (e2 > -dy)
                    {
                        err -= dy;
                        x1 += sx;
                    }

                    if (e2 < dx)
                    {
                        err += dx;
                        y1 += sy;
                    }
                }
            }
            else if (isSymetricBresenham)
            {
                // symetric bresenham
            }
        }
    }
}
