BeforeAll {
    Import-Module "./PSGraph.Tests/bin/Debug/net9.0/PSQuickGraph.psd1"
}

Describe 'Get-InEdge' {
    BeforeEach {
        # Initialize a new graph before each test
        $graph = New-Graph

        # Add vertices
        Add-Vertex -Graph $graph -Vertex 'A'
        Add-Vertex -Graph $graph -Vertex 'B'
        Add-Vertex -Graph $graph -Vertex 'C'

        # Add edges
        Add-Edge -From 'A' -To 'B' -Graph $graph | Out-Null
        Add-Edge -From 'A' -To 'C' -Graph $graph | Out-Null
        Add-Edge -From 'B' -To 'C' -Graph $graph | Out-Null
    }

    It 'Should return the incoming edges of a vertex' {
        $inEdges = Get-InEdge -Graph $graph -Vertex 'C'
        $inEdges | Should -Not -BeNullOrEmpty
        
        $inEdges | Should -Not -BeNullOrEmpty
        $expectedEdges = @('A->C', 'B->C')
        
        $inEdges | ForEach-Object {
            "$($_.Source.Name)->$($_.Target.Name)" | Should -BeIn $expectedEdges
        }
        
    }

    It 'Should return $null when no incoming edges exist for a vertex' {
        $inEdges = Get-InEdge -Graph $graph -Vertex 'A'

        $inEdges | Should -BeNullOrEmpty
    }
}

Describe 'Get-OutEdge' {
    BeforeEach {
        # Initialize a new graph before each test
        $graph = New-Graph

        # Add vertices
        Add-Vertex -Graph $graph -Vertex 'A'
        Add-Vertex -Graph $graph -Vertex 'B'
        Add-Vertex -Graph $graph -Vertex 'C'

        # Add edges
        Add-Edge -From 'A' -To 'B' -Graph $graph | Out-Null
        Add-Edge -From 'A' -To 'C' -Graph $graph | Out-Null
        Add-Edge -From 'B' -To 'C' -Graph $graph | Out-Null
    }

    It 'Should return the outgoing edges of a vertex' {
        $outEdges = Get-OutEdge -Graph $graph -Vertex 'A'

        $outEdges | Should -Not -BeNullOrEmpty

        $expectedEdges = @('A->B', 'A->C')
        
        $outEdges | ForEach-Object {
            "$($_.Source.Name)->$($_.Target.Name)" | Should -BeIn $expectedEdges
        }
    }

    It 'Should return $null when no outgoing edges exist for a vertex' {
        $outEdges = Get-OutEdge -Graph $graph -Vertex 'C'

        $outEdges | Should -BeNullOrEmpty
    }
}

