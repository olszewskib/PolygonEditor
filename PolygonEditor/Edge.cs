using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolygonEditor
{
    internal class Edge
    {
        public int Index { get; set; }
        public Vertex? v1 { get; set; }
        public Vertex? v2 { get; set; }
    }
}
