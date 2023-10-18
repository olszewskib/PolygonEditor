using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PolygonEditor
{
    enum Quadrant
    {
        I,
        II,
        III,
        IV,
        Axis
    }
    internal class ArrowVector
    {
        public Point StartPoint { get; set; }
        public Point EndPoint { get; set; }
        public Point MiddlePoint => new((StartPoint.X + EndPoint.X) / 2, (StartPoint.Y + EndPoint.Y) / 2);
        public double Start => EndPoint.X - StartPoint.X;
        public double End => EndPoint.Y - StartPoint.Y;

        public double Length => Math.Sqrt(Math.Pow(Start,2) + Math.Pow(End,2));


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
        public Point RightPoint(double d)
        {
            var len = Length;
            var v1 = ((End/len)*d,(-Start/len)*d);
            var v2 = ((-End/len)*d,(Start/len)*d);

            var v1End = new Point(MiddlePoint.X + v1.Item1, MiddlePoint.Y + v1.Item2);
            var v2End = new Point(MiddlePoint.X + v2.Item1, MiddlePoint.Y + v2.Item2);

            return RetrunRightTurn(v1End, v2End);
        }

        public Point RetrunRightTurn(Point p1, Point p2)
        {
            var quadrant = checkVectorOrientation(this);

            if (quadrant == Quadrant.I)
            {
                return (p1.X < p2.X) ? p1 : p2;
            }
            if (quadrant == Quadrant.II)
            {
                return (p1.X < p2.X) ? p1 : p2;
            }
            if (quadrant == Quadrant.III)
            {
                return (p1.X > p2.X) ? p1 : p2;
            }
            if (quadrant == Quadrant.IV)
            {
                return (p1.X > p2.X) ? p1 : p2;
            }

            return p1;

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
            return Quadrant.Axis;
        }

        public static double VectorProduct(ArrowVector v1, ArrowVector v2)
        {
            return v1.Start * v2.Start - v1.End * v2.End;
        }
    }
}
