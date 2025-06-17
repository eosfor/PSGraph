//add-edge -from $nodeFrom -to $nodeTo -attributes $attr -graph $g

namespace PSGraph.Cmdlets
{
    [Cmdlet(VerbsCommon.Add, "Edge")]
    public class AddEdgeCmdLet : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public PSObject From;

        [Parameter(Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public PSObject To;

        [Parameter(Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public PsBidirectionalGraph Graph;

        [Parameter(Mandatory = false)]
        public object Tag;

        protected override void ProcessRecord()
        {
            ProcessRecordDefault();
        }

        void ProcessRecordDefault()
        {
            PSVertex? newFrom = null;
            PSVertex? newTo = null;

            if (From.ImmediateBaseObject is PSVertex)
            {
                newFrom = (PSVertex)From.ImmediateBaseObject;
            }
            else
            {
                newFrom = new PSVertex(From.ImmediateBaseObject.ToString(), From.ImmediateBaseObject);
            }

            if (To.ImmediateBaseObject is PSVertex)
            {
                newTo = (PSVertex)To.ImmediateBaseObject;
            }
            else
            {
                newTo = new PSVertex(To.ImmediateBaseObject.ToString(), To.ImmediateBaseObject);
            }


            var edge = new PSEdge(newFrom, newTo, new PSEdgeTag(Tag?.ToString()));
            var result = Graph.AddVerticesAndEdge(edge);

            WriteVerbose(result.ToString());
           
        }
    }
}
