﻿using System;
using System.Linq;
using System.Management.Automation;
using QuickGraph;
using QuickGraph.Graphviz;
using System.Reflection;
using System.Runtime.Remoting.Channels;

namespace PSGraph
{
    [Cmdlet(VerbsData.Export, "Graph")]
    public class ExportGraphCmdLet : PSCmdlet
    {
        private ExportTypes _exportType;

        [Parameter(Mandatory = true)]
        public object Graph { get; set; }

        [Parameter(Mandatory = true)]
        public ExportTypes Format
        {
            get { return _exportType; }
            set
            {
                if (_exportType != value)
                {
                    if (!Enum.IsDefined(typeof(ExportTypes), value))
                        throw new ArgumentException();
                    _exportType = value;
                }
            }
        }

        [Parameter]
        public string Path { get; set; }


        protected override void ProcessRecord()
        {
            object graph = Graph;
            if (graph is PSObject)
            {
                graph = ((PSObject)graph).ImmediateBaseObject;
            }
            if (graph == null)
            {
                throw new ArgumentException("'Graph' mustn't be equal to null");
            }

            WriteVerbose("Add-Edge: Graph type is: " + Graph.GetType().ToString());

            Type vertexType = graph.GetType().GetGenericArguments()[0];
            Type edgeType = typeof(STaggedEdge<,>).MakeGenericType(vertexType, typeof(object));
            Type graphvizAlgType = typeof(GraphvizAlgorithm<,>).MakeGenericType(vertexType, edgeType);

            dynamic graphviz = Activator.CreateInstance(graphvizAlgType, graph);

            Type eventHandlerType = typeof(FormatVertexEventHandler<>).MakeGenericType(vertexType);
            var methodInfo = typeof(ExportGraphCmdLet).GetMethod(nameof(FormatVertexEventHandler)).MakeGenericMethod(vertexType);
            dynamic formatVertexEventHandler = Delegate.CreateDelegate(eventHandlerType, methodInfo);

            graphviz.FormatVertex += formatVertexEventHandler;

            string result = graphviz.Generate();

            string path = Path;
            if (path != null)
            {
                System.IO.File.WriteAllText(path, result);
            }
            else
            {
                WriteObject(result);
            }
        }

        public static void FormatVertexEventHandler<TVertex>(object sender, FormatVertexEventArgs<TVertex> e)
        {
            if (e.Vertex is PSGraphVertex)
            {
                foreach (PropertyInfo p in e.Vertex.GetType().GetProperties())
                {
                    var destProperty = e.VertexFormatter.GetType().GetProperty(p.Name);
                    if (destProperty != null)
                    {
                        destProperty.SetValue(e.VertexFormatter, p.GetValue(e.Vertex));
                    }
                }
            }
        }
    }
}
