using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolygonEditor
{
    internal class Polygon
    {
        public Vertex? FirstVertex {  get; set; }
        public Vertex? LastVertex { get; set; }

        public List<Vertex> Vertices = new();

        public void AddVertex(Vertex v)
        {
            if(Vertices.Count == 0) FirstVertex = v;

            LastVertex = v;
            Vertices.Add(v);
        }
    }
}
