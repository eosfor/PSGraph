BeforeAll {
    Import-Module "./PSGraph.Tests/bin/Debug/net9.0/PSQuickGraph.psd1" -Force
}

Describe 'Test-GraphPath' {
    BeforeEach {
        $graph = New-Graph
        'A','B','C','D','E','F','G' | ForEach-Object { Add-Vertex -Graph $graph -Vertex $_ }
        # Main chain A->B->C->D
        Add-Edge -From 'A' -To 'B' -Graph $graph | Out-Null
        Add-Edge -From 'B' -To 'C' -Graph $graph | Out-Null
        Add-Edge -From 'C' -To 'D' -Graph $graph | Out-Null
        # Alternative branch A->E->F->D
        Add-Edge -From 'A' -To 'E' -Graph $graph | Out-Null
        Add-Edge -From 'E' -To 'F' -Graph $graph | Out-Null
        Add-Edge -From 'F' -To 'D' -Graph $graph | Out-Null
        # Dead-end from C to G only
        Add-Edge -From 'C' -To 'G' -Graph $graph | Out-Null
    }

    It 'Returns $true for direct reachability' {
        Test-GraphPath -Graph $graph -From 'A' -To 'B' | Should -BeTrue
    }

    It 'Returns $true for multi-hop reachability along a chain' {
        Test-GraphPath -Graph $graph -From 'A' -To 'D' | Should -BeTrue
    }

    It 'Returns $true when vertices are in the same strongly connected component (cycle)' {
        Add-Edge -From 'D' -To 'B' -Graph $graph | Out-Null  # creates cycle B->C->D->B
        Test-GraphPath -Graph $graph -From 'C' -To 'B' | Should -BeTrue
    }

    It 'Returns $false when no path exists' {
        # H is isolated
        Add-Vertex -Graph $graph -Vertex 'H'
        Test-GraphPath -Graph $graph -From 'H' -To 'A' | Should -BeFalse
    }

    It 'Returns $true for same vertex (trivial path)' {
        Test-GraphPath -Graph $graph -From 'A' -To 'A' | Should -BeTrue
    }

    It 'Returns $false for reverse direction when only one-way edges exist' {
        Test-GraphPath -Graph $graph -From 'D' -To 'A' | Should -BeFalse
    }
}
