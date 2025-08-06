BeforeAll {
    Import-Module "./PSGraph.Tests/bin/Debug/net9.0/PSQuickGraph.psd1"
}

Describe 'Graph analysis cmdlets' {
    Context 'Get-GraphConnectedComponent' {
        It 'groups vertices into components' {
            $graph = New-Graph
            Add-Vertex -Graph $graph -Vertex 'A'
            Add-Vertex -Graph $graph -Vertex 'B'
            Add-Vertex -Graph $graph -Vertex 'C'
            Add-Vertex -Graph $graph -Vertex 'D'

            Add-Edge -From 'A' -To 'B' -Graph $graph | Out-Null
            Add-Edge -From 'B' -To 'A' -Graph $graph | Out-Null
            Add-Edge -From 'C' -To 'D' -Graph $graph | Out-Null
            Add-Edge -From 'D' -To 'C' -Graph $graph | Out-Null

            $components = Get-GraphConnectedComponent -Graph $graph
            $components | Should -Not -BeNullOrEmpty
            $compA = ($components | Where-Object { $_.Vertex.Name -eq 'A' }).ComponentId
            $compB = ($components | Where-Object { $_.Vertex.Name -eq 'B' }).ComponentId
            $compC = ($components | Where-Object { $_.Vertex.Name -eq 'C' }).ComponentId
            $compA | Should -BeExactly $compB
            $compA | Should -Not -BeExactly $compC
        }
    }

    Context 'Find-GraphCycle' {
        It 'detects cycles in the graph' {
            $graph = New-Graph
            Add-Vertex -Graph $graph -Vertex 'A'
            Add-Vertex -Graph $graph -Vertex 'B'
            Add-Vertex -Graph $graph -Vertex 'C'
            Add-Edge -From 'A' -To 'B' -Graph $graph | Out-Null
            Add-Edge -From 'B' -To 'C' -Graph $graph | Out-Null
            Add-Edge -From 'C' -To 'A' -Graph $graph | Out-Null

            $cycles = Find-GraphCycle -Graph $graph
            $cycles | Should -Not -BeNullOrEmpty
            ($cycles[0].Vertices | ForEach-Object Name) | Should -Contain 'A'
            ($cycles[0].Vertices | ForEach-Object Name) | Should -Contain 'B'
            ($cycles[0].Vertices | ForEach-Object Name) | Should -Contain 'C'
        }
    }

    Context 'Get-GraphCentrality' {
        It 'computes betweenness centrality' {
            $graph = New-Graph
            Add-Vertex -Graph $graph -Vertex 'A'
            Add-Vertex -Graph $graph -Vertex 'B'
            Add-Vertex -Graph $graph -Vertex 'C'
            Add-Edge -From 'A' -To 'B' -Graph $graph | Out-Null
            Add-Edge -From 'B' -To 'C' -Graph $graph | Out-Null

            $centrality = Get-GraphCentrality -Graph $graph
            $centrality | Should -Not -BeNullOrEmpty
            $centA = ($centrality | Where-Object { $_.Vertex.Name -eq 'A' }).Centrality
            $centB = ($centrality | Where-Object { $_.Vertex.Name -eq 'B' }).Centrality
            $centC = ($centrality | Where-Object { $_.Vertex.Name -eq 'C' }).Centrality
            $centB | Should -BeGreaterThan $centA
            $centB | Should -BeGreaterThan $centC
        }
    }
}
