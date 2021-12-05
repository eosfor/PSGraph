using System;
using System.Collections.Generic;
using System.Management.Automation;
using QuikGraph;

namespace PSGraph
{
    [Cmdlet(VerbsCommon.New, "Graph")]
    public class NewPsGraphCmdlet : PSCmdlet
    {
        private PsGraphType _grapthType;

        [Parameter(Mandatory = true, HelpMessage = "BidirectionalMatrixGraph is not supported")]
        public PsGraphType Type
        {
            get { return _grapthType; }
            set
            {
                if (!Enum.IsDefined(typeof(PsGraphType), value))
                {
                    throw new ArgumentException();
                }

                if (value == PsGraphType.BidirectionalMatrixGraph)
                {
                    throw new NotSupportedException();
                }

                _grapthType = value;
            }
        }

        protected override void ProcessRecord()
        {
            WriteVerbose("New PsGraph: " + _grapthType);

            object newGraph = null;

            switch (_grapthType)
            {
                case PsGraphType.AdjacencyGraph:
                    newGraph = new AdjacencyGraph<object, STaggedEdge<object, object>>(false);
                    break;

                case PsGraphType.BidirectionalGraph:
                    newGraph = new BidirectionalGraph<object, STaggedEdge<object, object>>(false);
                    break;

                case PsGraphType.BidirectionalMatrixGraph:
                    //newGraph = new BidirectionalMatrixGraph<Edge<int>();
                    ThrowTerminatingError(new ErrorRecord(new NotImplementedException(), "99", ErrorCategory.InvalidOperation, null));
                    break;

                case PsGraphType.UndirectedGraph:
                    newGraph = new UndirectedGraph<object, STaggedEdge<object, object>>(false);
                    break;
            }

            WriteObject(newGraph);
        }
    }
}
