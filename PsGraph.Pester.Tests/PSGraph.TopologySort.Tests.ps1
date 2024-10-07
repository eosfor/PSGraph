BeforeAll {
    Import-Module "/workspaces/PSGraph/PSGraph.Tests/bin/Debug/net8.0/PSQuickGraph.psd1"
}

Describe 'Get-GraphTopologicSort' {
    BeforeEach {
        # Initialize a new graph before each test
        $graph = New-Graph

        # Add vertices
        Add-Vertex -Graph $graph -Vertex 'A'
        Add-Vertex -Graph $graph -Vertex 'B'
        Add-Vertex -Graph $graph -Vertex 'C'
        Add-Vertex -Graph $graph -Vertex 'D'
        Add-Vertex -Graph $graph -Vertex 'E'

        # Add edges
        Add-Edge -From 'A' -To 'B' -Graph $graph | Out-Null
        Add-Edge -From 'A' -To 'C' -Graph $graph | Out-Null
        Add-Edge -From 'B' -To 'D' -Graph $graph | Out-Null
        Add-Edge -From 'C' -To 'D' -Graph $graph | Out-Null
        Add-Edge -From 'D' -To 'E' -Graph $graph | Out-Null
    }

    It 'Should return a topologically sorted list of vertices' {
        $sortedVertices = Get-GraphTopologicSort -Graph $graph

        $sortedVertices | Should -Not -BeNullOrEmpty
        
        $sortedVertices2 = $sortedVertices | ForEach-Object { $_ }


        # $sortedVertices2 | Should -BeExactly @('A', 'C', 'B', 'D', 'E')

        # Additional checks to ensure the topological order is valid
        # 'A' must come before 'B', 'C', 'D', 'E'
        $sortedVertices2.IndexOf('A') | Should -BeLessThan $sortedVertices2.IndexOf('B')
        $sortedVertices2.IndexOf('A') | Should -BeLessThan $sortedVertices2.IndexOf('C')
        $sortedVertices2.IndexOf('A') | Should -BeLessThan $sortedVertices2.IndexOf('D')
        $sortedVertices2.IndexOf('A') | Should -BeLessThan $sortedVertices2.IndexOf('E')ß

        # 'B' must come before 'D'
        $sortedVertices2.IndexOf('B') | Should -BeLessThan $sortedVertices2.IndexOf('D')

        # 'C' must come before 'D'
        $sortedVertices2.IndexOf('C') | Should -BeLessThan $sortedVertices2.IndexOf('D')

        # 'D' must come before 'E'
        $sortedVertices2.IndexOf('D') | Should -BeLessThan $sortedVertices2.IndexOf('E')
        
        
    }

    It 'Should return $null for an empty graph' {
        $emptyGraph = New-Graph
        $sortedVertices = Get-GraphTopologicSort -Graph $emptyGraph

        $sortedVertices | Should -BeNullOrEmpty
    }

    It 'Should throw an error for a cyclic graph' {
        # Add a cycle
        Add-Edge -From 'E' -To 'A' -Graph $graph | Out-Null

        { Get-GraphTopologicSort -Graph $graph } | Should -Throw -ErrorId 'QuikGraph.NonAcyclicGraphException,PSGraph.Cmdlets.GetGraphTopologicSort'
    }
}
