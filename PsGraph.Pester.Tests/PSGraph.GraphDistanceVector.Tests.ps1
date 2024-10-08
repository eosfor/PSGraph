BeforeAll {
    Import-Module "/workspaces/PSGraph/PSGraph.Tests/bin/Debug/net8.0/PSQuickGraph.psd1"
}

Describe 'Get-GraphDistanceVector' {
    BeforeEach {
        # Initialize a new graph before each test
        $graph = New-Graph

        # Add vertices
        Add-Vertex -Graph $graph -Vertex 'A'
        Add-Vertex -Graph $graph -Vertex 'B'
        Add-Vertex -Graph $graph -Vertex 'C'
        Add-Vertex -Graph $graph -Vertex 'D'

        # Add edges
        Add-Edge -From 'A' -To 'B' -Graph $graph | Out-Null
        Add-Edge -From 'A' -To 'C' -Graph $graph | Out-Null
        Add-Edge -From 'B' -To 'D' -Graph $graph | Out-Null
        Add-Edge -From 'C' -To 'D' -Graph $graph | Out-Null
    }

    It 'Should return the correct distance vector for the graph' {
        # Run the Get-GraphDistanceVector cmdlet
        $distanceVector = Get-GraphDistanceVector -Graph $graph

        # Check the results are not empty
        $distanceVector | Should -Not -BeNullOrEmpty

        # Expected results for distance from root vertices
        $expectedResults = @(
            @{ Vertex = 'A'; Level = 0 },
            @{ Vertex = 'B'; Level = 1 },
            @{ Vertex = 'C'; Level = 1 },
            @{ Vertex = 'D'; Level = 2 }
        )

        # Validate each record in the distance vector
        foreach ($record in $distanceVector) {
            $vertex = $record.Vertex.Name
            $level = $record.Level

            $expected = $expectedResults | Where-Object { $_.Vertex -eq $vertex }

            $expected | Should -Not -BeNullOrEmpty
            $level | Should -BeExactly $expected.Level
        }
    }

    It 'Should return an empty result if the graph has no vertices' {
        $emptyGraph = New-Graph
        $result = Get-GraphDistanceVector -Graph $emptyGraph

        # The result should be empty
        $result | Should -BeNullOrEmpty
    }
}
