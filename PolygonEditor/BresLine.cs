using System;
using System.Collections.Generic;
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
                Pen line = new Pen(lineColor, Edge.Width);
                drawingContext.DrawLine(line,new Point(X1,Y1), new Point(X2,Y2));
            }
            else
            {
                Pen pen = new Pen(lineColor, 2);
                int x1 = (int)X1;
                int y1 = (int)Y1;
                int x2 = (int)X2;
                int y2 = (int)Y2;

                int dx = Math.Abs(x2 - x1);
                int dy = Math.Abs(y2 - y1);
                int sx = x1 < x2 ? 1 : -1;
                int sy = y1 < y2 ? 1 : -1;
                int err = dx - dy;

                while (true)
                {
                    drawingContext.DrawRectangle(pen.Brush, pen, new Rect(x1, y1, 1, 1));

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
        }
    }
}
