using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;


namespace PSGraph
{
	[Cmdlet(VerbsCommon.New, "Graph")]
	public class PsGraph : PSCmdlet
	{
		private string _grapthType;

		[Parameter(Mandatory = true, HelpMessage= "no one is supported")]
		public string Type
		{
			get { return _grapthType; }
			set { _grapthType = value; }
		}

		protected override void ProcessRecord()
		{
			WriteVerbose("New PsGraph: " + _grapthType);
			//WriteObject("New PsGraph: " + _grapthType);
		}
	}
}
