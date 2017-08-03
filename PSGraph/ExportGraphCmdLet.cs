using System;
using System.Management.Automation;
using QuickGraph;
using QuickGraph.Graphviz;
using System.Reflection;
using System.Xml;
using QuickGraph.Serialization;
using System.IO;
using System.Text;

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

        [Parameter]
        public object EdgeFormatter { get; set; }

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

            if (_exportType == ExportTypes.GraphML) { ExportGraphML(graph); return; };

            Type[] graphGenericArgs = graph.GetType().GetGenericArguments();
            Type vertexType = graphGenericArgs[0];
            Type edgeType = graphGenericArgs[1];
            Type graphvizAlgType = typeof(GraphvizAlgorithm<,>).MakeGenericType(vertexType, edgeType);

            dynamic graphviz = Activator.CreateInstance(graphvizAlgType, graph);

            Type eventHandlerType = typeof(FormatVertexEventHandler<>).MakeGenericType(vertexType);
            var methodInfo = typeof(ExportGraphCmdLet).GetMethod(nameof(FormatVertexEventHandler)).MakeGenericMethod(vertexType);
            dynamic formatVertexEventHandler = Delegate.CreateDelegate(eventHandlerType, methodInfo);
            graphviz.FormatVertex += formatVertexEventHandler;

            if (EdgeFormatter != null)
            {
                Type formatEdgeEventHandlerType = typeof(FormatEdgeAction<,>).MakeGenericType(vertexType, edgeType);
                var formatEdgeMethodInfo = typeof(ExportGraphCmdLet).GetMethod(nameof(FormatEdgeAction))
                    .MakeGenericMethod(vertexType, edgeType);
                dynamic formatEdgeEventHandler =
                    Delegate.CreateDelegate(formatEdgeEventHandlerType, this, formatEdgeMethodInfo);
                graphviz.FormatEdge += formatEdgeEventHandler;
            }

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

        public void FormatEdgeAction<TVertex, TEdge>(object sender, FormatEdgeEventArgs<TVertex, TEdge> e) where TEdge : IEdge<TVertex>
        {
            dynamic formatter = EdgeFormatter;
            if (formatter is PSObject)
            {
                formatter = ((PSObject)formatter).ImmediateBaseObject;
            }

            formatter.Invoke(e.Edge, e.EdgeFormatter);
        }

        public void ExportGraphML(dynamic graph)
        {
            //graph.(xwriter);
            Type[] graphGenericArgs = graph.GetType().GetGenericArguments();
            Type vertexType = graphGenericArgs[0];
            Type edgeType = graphGenericArgs[1];

            Type eventHandlerType = typeof(FormatVertexEventHandler<>).MakeGenericType(vertexType);
            var methodInfo = typeof(ExportGraphCmdLet).GetMethod(nameof(FormatVertexEventHandler)).MakeGenericMethod(vertexType);
            dynamic formatVertexEventHandler = Delegate.CreateDelegate(eventHandlerType, methodInfo);


            if (!string.IsNullOrEmpty(Path))
            {
                using (XmlWriter xwriter = XmlWriter.Create(Path))
                {
                    QuickGraph.Serialization.GraphMLExtensions.SerializeToGraphML<object, STaggedEdge<object, object>, BidirectionalGraph<object, STaggedEdge<object, object>>>(graph, xwriter);
                }
            }
            else
            {
                StringWriter str = new StringWriter();
                using (XmlWriter xwriter = XmlWriter.Create(str))
                {
                    QuickGraph.Serialization.GraphMLExtensions.SerializeToGraphML<object, STaggedEdge<object, object>, BidirectionalGraph<object, STaggedEdge<object, object>>>(graph, xwriter);

                    WriteObject(str.ToString());
                }

            }


        }
    }
}
