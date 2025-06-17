namespace PSGraph.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "GraphDistanceVector")]
    public class GetGraphDistanceVector : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public PsBidirectionalGraph Graph;

        protected override void EndProcessing()
        {

            var dfs = new DepthFirstSearchAlgorithm<PSVertex, PSEdge>(Graph);
            var distanceRecorder = new VertexDistanceRecorderObserver<PSVertex, PSEdge>(v => 1);

            var rootVertices = Graph.Vertices.Where(v => Graph.InDegree(v) == 0);

            using (distanceRecorder.Attach(dfs))
            {
                foreach (var vertex in rootVertices)
                {
                    dfs.SetRootVertex(vertex);
                    dfs.Compute();
                }

            }

            var res = new List<PSDistanceVectorRecord>();
            foreach (var record in distanceRecorder.Distances)
            {
                var rr = new PSDistanceVectorRecord() { Vertex = record.Key, Level = record.Value };
                res.Add(rr);
            }

            WriteObject(res, enumerateCollection: true);
        }
    }
}