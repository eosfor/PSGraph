using System;
using System.Linq;
using System.Management.Automation;
using QuickGraph;
using QuickGraph.Graphviz;
using System.Reflection;
using System.Runtime.Remoting.Channels;
using QuickGraph.Graphviz.Dot;

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

            Type[] graphGenericArgs = graph.GetType().GetGenericArguments();
            Type vertexType = graphGenericArgs[0];
            Type edgeType = graphGenericArgs[1];
            Type graphvizAlgType = typeof(GraphvizAlgorithm<,>).MakeGenericType(vertexType, edgeType);

            dynamic graphviz = Activator.CreateInstance(graphvizAlgType, graph);

            Type eventHandlerType = typeof(FormatVertexEventHandler<>).MakeGenericType(vertexType);
            var methodInfo = typeof(ExportGraphCmdLet).GetMethod(nameof(FormatVertexEventHandler)).MakeGenericMethod(vertexType);
            dynamic formatVertexEventHandler = Delegate.CreateDelegate(eventHandlerType, methodInfo);

            Type formatEdgeEventHandlerType = typeof(FormatEdgeAction<,>).MakeGenericType(vertexType, edgeType);
            var formatEdgeMethodInfo = typeof(ExportGraphCmdLet).GetMethod(nameof(FormatEdgeAction)).MakeGenericMethod(vertexType, edgeType);
            dynamic formatEdgeEventHandler = Delegate.CreateDelegate(formatEdgeEventHandlerType, formatEdgeMethodInfo);


            graphviz.FormatVertex += formatVertexEventHandler;
            graphviz.FormatEdge += formatEdgeEventHandler;

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


        public static void FormatEdgeAction<TVertex, TEdge>(object sender, FormatEdgeEventArgs<TVertex, TEdge> e) where TEdge : IEdge<TVertex>
        {
            dynamic x = e.Edge;
            try
            {
                //dynamic color = x.strokeGraphvizColor;
                e.EdgeFormatter.StrokeGraphvizColor = x.strokeGraphvizColor;
            }
            catch { }
        }
    }
}
