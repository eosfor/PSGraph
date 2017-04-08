dir C:\Projects\Repos\psgraph\PSGraph\bin\Debug\q*.dll  | % {Add-Type -Path $_.fullname}

$ref =  "C:\Projects\Repos\psgraph\PSGraph\bin\Debug\QuickGraph.dll", 
        "C:\Projects\Repos\psgraph\PSGraph\bin\Debug\QuickGraph.Graphviz.dll"

$class = @"
using QuickGraph;
using System;
using System.Collections.Generic;
using QuickGraph.Graphviz.Dot;

namespace quickGraphTests
{
        public class baseVertexComparer  : EqualityComparer<GraphvizVertex>
        {
            public override bool Equals(GraphvizVertex x, GraphvizVertex y)
            {
                return (x.Label == y.Label);
            }

            public override int GetHashCode(GraphvizVertex x)
            {
                return x.Label.Length;
            }
        }
}

"@
Add-Type -TypeDefinition $class -Language CSharp -ReferencedAssemblies $ref



class VNET : QuickGraph.Graphviz.Dot.GraphvizVertex {
    [string]$ipRange;
}



$baseEQComparer = [quickGraphTests.baseVertexComparer]::new()

$graph1 = [QuickGraph.AdjacencyGraph[QuickGraph.Graphviz.Dot.GraphvizVertex,QuickGraph.STaggedEdge[QuickGraph.Graphviz.Dot.GraphvizVertex,int]]]::new($false,  -1, -1, [System.Collections.Generic.IEqualityComparer[QuickGraph.Graphviz.Dot.GraphvizVertex]]$baseEQComparer)


$v1 = [VNET]@{Label = "vnet1"; iprange = "10.10.5.0/24"}
$v2 = [VNET]@{Label = "vnet1"; iprange = "10.10.6.0/24"}
$v3 = [VNET]@{Label = "vnet2"; iprange = "10.10.7.0/24"}


$v1.Equals($v2)
$v1.Equals($v3)

$graph1.AddVertex($v1)
$graph1.AddVertex($v2)

$graph1.ContainsVertex($v2)