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
        public int Index {  get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public Vertex? Right { get; set; }
        public Vertex? Left { get; set; }
        public Ellipse? Graphic { get; set; }
    }
}
