BeforeAll {
    Import-Module "/Users/andrei/repo/PSGraph/PSGraph/bin/Debug/net9.0/publish/PSGraph.dll" -Verbose
    Import-Module "/Users/andrei/repo/PSBicepGraph/src/PSBicepGraph/bin/Debug/net9.0/publish/PSBicepGraph.dll" -Verbose
}

Describe 'Start-DSMClustering AlgorithmConfig coercion' {
    BeforeEach {
        $g = New-Graph
        'A', 'B', 'C', 'D' | ForEach-Object { Add-Vertex -Graph $g -Vertex $_ | Out-Null }
        Add-Edge -From A -To B -Graph $g | Out-Null
        Add-Edge -From B -To C -Graph $g | Out-Null
        Add-Edge -From C -To A -Graph $g | Out-Null  # cycle
        Add-Edge -From C -To D -Graph $g | Out-Null
        $script:dsm = New-DSM -Graph $g
    }

    It 'Accepts hashtable for simulated annealing config' {
        $cfg = @{ Times = 1; StableLimit = 1; MaxRepeat = 10 }
        $res = Start-DSMClustering -Dsm $script:dsm -ClusteringAlgorithm Classic -AlgorithmConfig $cfg -Detailed
        $res | Should -Not -BeNullOrEmpty
        $res.Algorithm.GetType().Name | Should -Be 'DsmSimulatedAnnealingAlgorithm'
        # cost history should have at least initial cost
        $res.CostHistory.Count | Should -BeGreaterThan 0
    }

    It 'Accepts pscustomobject for simulated annealing config' {
        $cfg = [pscustomobject]@{ Times = 1; StableLimit = 1; MaxRepeat = 5 }
        # Wait-Debugger
        $res = Start-DSMClustering -Dsm $script:dsm -ClusteringAlgorithm Classic -AlgorithmConfig $cfg -Detailed
        $res.Passes | Should -BeGreaterOrEqual 1
        $res.Algorithm.GetType().Name | Should -Be 'DsmSimulatedAnnealingAlgorithm'
    }

    It 'Falls back to defaults with incompatible config shape (graph-based)' {
        $cfg = @{ Foo = 123; Bar = 456 }
        $res = Start-DSMClustering -Dsm $script:dsm -ClusteringAlgorithm GraphBased -AlgorithmConfig $cfg -Detailed
        $res.Algorithm.GetType().Name | Should -Be 'DsmGraphPartitioningAlgorithm'
        $res.CostHistory.Count | Should -Be 1 # single-pass
    }
}
