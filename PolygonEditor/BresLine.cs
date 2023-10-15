using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PolygonEditor
{
    internal class BresLine : UIElement
    {
        public Point Start {  get; set; }
        public Point End {  get; set; }

        private Brush lineColor = Brushes.Red;
        public Brush LineColor
        {
            get { return lineColor; }
            set
            {
                lineColor = value;
                this.InvalidateVisual();
            }
        }
        public BresLine()
        {
            
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            //Pen pen = new Pen(lineColor, 2);
            //drawingContext.DrawLine(pen, Start, End);
            Pen pen = new Pen(lineColor, 2);
            int x1 = (int)Start.X;
            int y1 = (int)Start.Y;
            int x2 = (int)End.X;
            int y2 = (int)End.Y;

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
