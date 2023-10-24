using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PolygonEditor.Models
{
    enum Quadrant
    {
        I,
        II,
        III,
        IV,
        AxisP,
        AxisM
    }
    internal class ArrowVector
    {
        public Point StartPoint { get; set; }
        public Point EndPoint { get; set; }
        public Point MiddlePoint => new((StartPoint.X + EndPoint.X) / 2, (StartPoint.Y + EndPoint.Y) / 2);
        public double Start => EndPoint.X - StartPoint.X;
        public double End => EndPoint.Y - StartPoint.Y;
        public double Length => Math.Sqrt(Math.Pow(Start, 2) + Math.Pow(End, 2));
        public ArrowVector(Point StartPoint, Point EndPoint)
        {
            this.StartPoint = StartPoint;
            this.EndPoint = EndPoint;
        }
        public ArrowVector(Vertex StartPoint, Vertex EndPoint)
        {
            this.StartPoint = StartPoint.Center;
            this.EndPoint = EndPoint.Center;
        }
        public int RetrunRightTurn()
        {
            var quadrant = checkVectorOrientation(this);

            if (quadrant == Quadrant.I || quadrant == Quadrant.IV || quadrant == Quadrant.AxisP)
            {
                return 1;
            }
            if (quadrant == Quadrant.II || quadrant == Quadrant.III || quadrant == Quadrant.AxisM)
            {
                return -1;
            }

            // safety
            return 1;
        }
        public static Quadrant checkVectorOrientation(ArrowVector vector)
        {
            var deltaX = -vector.StartPoint.X;
            var deltaY = -vector.StartPoint.Y;

            var newEndX = vector.EndPoint.X + deltaX;
            var newEndY = vector.EndPoint.Y + deltaY;

            if (newEndX > 0 && newEndY > 0) return Quadrant.I;
            if (newEndX < 0 && newEndY > 0) return Quadrant.II;
            if (newEndX < 0 && newEndY < 0) return Quadrant.III;
            if (newEndX > 0 && newEndY < 0) return Quadrant.IV;

            if (newEndY < 0 || newEndX > 0) return Quadrant.AxisP;
            return Quadrant.AxisM;
        }
        public static double VectorProduct(Point p1, Point p2)
        {
            return p1.X * p2.Y - p2.X * p1.Y;
        }
    }
}
