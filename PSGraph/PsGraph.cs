﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using QuickGraph;


namespace PSGraph
{
	[Cmdlet(VerbsCommon.New, "Graph")]
	public class PsGraph : PSCmdlet
	{
		private PsGraphType _grapthType;

		[Parameter(Mandatory = true, HelpMessage= "BidirectionalMatrixGraph is not supported")]
		public PsGraphType Type
		{
			get { return _grapthType; }
		    set
		    {
		        if (!Enum.IsDefined(typeof (PsGraphType), value))
		            throw new System.ArgumentException();

		        if (value == PsGraphType.BidirectionalMatrixGraph)
		            throw new NotSupportedException();

                _grapthType = value;
		    }
		}


		protected override void ProcessRecord()
		{
			WriteVerbose("New PsGraph: " + _grapthType);

		    Object newGraph = null;

			switch (_grapthType)
			{
				case PsGraphType.AdjacencyGraph:
			        newGraph = new AdjacencyGraph<Object, STaggedEdge<Object, Object>>();
                    break;

				case PsGraphType.BidirectionalGraph:
                    newGraph = new BidirectionalGraph<Object, STaggedEdge<Object, Object>>();
                    break;

				case PsGraphType.BidirectionalMatrixGraph:
                    //newGraph = new BidirectionalMatrixGraph<Edge<int>();
                    break;

				case PsGraphType.UndirectedGraph:
			        newGraph = new UndirectedGraph<Object, STaggedEdge<Object, Object>>();
                    break;
			}

			WriteObject(newGraph);
		}
	}
}




