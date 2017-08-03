﻿using QuickGraph.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace PSGraph
{
    public static class PocSerializeHelper
    {
        public static PocGraph LoadGraph(string filename)
        {
            using (XmlReader reader = XmlReader.Create(filename))
            {
                var serializer = new GraphMLDeserializer<PocVertex, PocEdge, PocGraph>();

                var pocGraph = new PocGraph();
                serializer.Deserialize(reader, pocGraph, id => new PocVertex(id), (source, target, id) => new PocEdge(id, source, target));
                return pocGraph;
            }
        }

        public static void SaveGraph(PocGraph graph, string filename)
        {
            using (XmlWriter writer = XmlWriter.Create(filename))
            {
                var serializer = new GraphMLSerializer<PocVertex, PocEdge, PocGraph>();
                serializer.Serialize(writer, graph, v => v.ID, e => e.ID);
            }
        }
    }
}
