BeforeAll {
    Import-Module "./PSGraph.Tests/bin/Debug/net9.0/PSQuickGraph.psd1"
}

Describe 'Get-GraphPath' {
    BeforeEach {
        # Initialize a new graph before each test
        $graph = New-Graph

        # Add vertices
        Add-Vertex -Graph $graph -Vertex 'A'
        Add-Vertex -Graph $graph -Vertex 'B'
        Add-Vertex -Graph $graph -Vertex 'C'
        Add-Vertex -Graph $graph -Vertex 'D'
        Add-Vertex -Graph $graph -Vertex 'E'
        Add-Vertex -Graph $graph -Vertex 'F'

        # Add edges
        Add-Edge -From 'A' -To 'B' -Graph $graph | Out-Null
        Add-Edge -From 'B' -To 'C' -Graph $graph | Out-Null
        Add-Edge -From 'C' -To 'D' -Graph $graph | Out-Null
        Add-Edge -From 'A' -To 'E' -Graph $graph | Out-Null
        Add-Edge -From 'E' -To 'F' -Graph $graph | Out-Null
        Add-Edge -From 'F' -To 'D' -Graph $graph | Out-Null
    }

    It 'Should find a direct path between two connected vertices' {
        $path = Get-GraphPath -Graph $graph -From 'A' -To 'B'

        $path | Should -Not -BeNullOrEmpty
        @($path.Source.Name, $path.Target.Name) | Should -BeExactly @('A', 'B')
    }

    It 'Should find a path between two vertices with intermediate nodes' {
        $path = Get-GraphPath -Graph $graph -From 'A' -To 'D'

        $path | Should -Not -BeNullOrEmpty
        # Possible paths: A -> B -> C -> D or A -> E -> F -> D
        # However Dijkstra returns the first one it finds
        $formattedPath = $path | % { $_.source.label,$_.target.label } | Select-Object -Unique
        $formattedPath | Should -BeExactly @('A', 'B', 'C', 'D')
    }

    It 'Should return $null or empty when no path exists between two vertices' {
        # Add a disconnected vertex 'G'
        Add-Vertex -Graph $graph -Vertex 'G'

        $path = Get-GraphPath -Graph $graph -From 'A' -To 'G'

        ($path -eq $null -or $path.Count -eq 0) | Should -BeTrue
    }

    It 'Should handle paths in graphs with cycles without entering infinite loops' {
        # Add a cycle: D -> B
        Add-Edge -From 'D' -To 'B' -Graph $graph | Out-Null

        $path = Get-GraphPath -Graph $graph -From 'A' -To 'D'

        $path | Should -Not -BeNullOrEmpty
        # Despite the cycle, the shortest path should still be of length 4
        $formattedPath = $path | % { $_.source.label,$_.target.label } | Select-Object -Unique
        $formattedPath | Should -BeExactly @('A', 'B', 'C', 'D')
    }
}
