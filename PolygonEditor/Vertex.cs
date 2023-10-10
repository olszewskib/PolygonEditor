using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PolygonEditor
{
    internal class Vertex
    {
        public int PolygonIndex {  get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public Vertex? Right { get; set; }
        public Edge? RightEdge {  get; set; }
        public Vertex? Left { get; set; }
        public Edge? LeftEdge {  get; set; }
        public Ellipse? Graphic { get; set; }
    }
}
