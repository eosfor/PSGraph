using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using System.Threading.Tasks;

namespace PSGraph.Tests
{
	class PSHelpers
	{
		static public Object RunPSCmdlet(PSCmdlet cmdlet)
		{
            //PowerShell powershell = PowerShell.Create();
            //powershell.AddCommand("Import-Module").AddParameter("Assembly", System.Reflection.Assembly.GetExecutingAssembly());

            //Pipeline pipeLine = powershell.Runspace.CreatePipeline();

            //PowerShell ps = PowerShell.Create();

            //ps.Invoke();
            //ps.Commands.Clear();
            return null;
		}
	}
}
