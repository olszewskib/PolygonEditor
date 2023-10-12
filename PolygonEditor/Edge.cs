using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace PolygonEditor
{
    internal class Edge
    {
        public int PolygonIndex { get; set; }
        public Vertex? v1 { get; set; }
        public Vertex? v2 { get; set; }
        public Line? Graphic { get; set; }
    }
}
