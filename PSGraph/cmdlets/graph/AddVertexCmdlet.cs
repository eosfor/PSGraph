namespace PSGraph.Cmdlets
{
    [Cmdlet(VerbsCommon.Add, "Vertex")]
    public class AddVertexCmdlet : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public PSObject Vertex;

        [Parameter(Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public PsBidirectionalGraph Graph;

        protected override void ProcessRecord()
        {

            //var v = ((PSObject) Vertex).ImmediateBaseObject;

            PSVertex? newPSVertex = null;

            if (Vertex.ImmediateBaseObject is PSVertex)
            {
                newPSVertex = (PSVertex)Vertex.ImmediateBaseObject;
            }
            else
            {
                newPSVertex = new PSVertex(Vertex.ImmediateBaseObject.ToString(), Vertex.ImmediateBaseObject);
            }

            var result = Graph.AddVertex(newPSVertex);
            WriteVerbose(result.ToString());
        }
    }
}
