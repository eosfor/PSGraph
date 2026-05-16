BeforeAll {
    Import-Module "./PSGraph.Tests/bin/Debug/net9.0/PSQuickGraph.psd1"
}

Describe 'Get-GraphTopologicalSort' {
    It 'Should return source vertices before dependent vertices' {
        $graph = New-Graph
        Add-Edge -From A -To B -Graph $graph | Out-Null
        Add-Edge -From A -To C -Graph $graph | Out-Null
        Add-Edge -From B -To D -Graph $graph | Out-Null
        Add-Edge -From C -To D -Graph $graph | Out-Null

        $order = @(Get-GraphTopologicalSort -Graph $graph)
        $names = @($order | ForEach-Object Name)

        $names.Count | Should -Be 4
        $names.IndexOf('A') | Should -BeLessThan $names.IndexOf('B')
        $names.IndexOf('A') | Should -BeLessThan $names.IndexOf('C')
        $names.IndexOf('B') | Should -BeLessThan $names.IndexOf('D')
        $names.IndexOf('C') | Should -BeLessThan $names.IndexOf('D')
    }

    It 'Should return targets before sources when reversed' {
        $graph = New-Graph
        Add-Edge -From A -To B -Graph $graph | Out-Null
        Add-Edge -From B -To C -Graph $graph | Out-Null

        $names = @(Get-GraphTopologicalSort -Graph $graph -Reverse | ForEach-Object Name)

        $names | Should -Be @('C', 'B', 'A')
    }

    It 'Should throw for cyclic graphs' {
        $graph = New-Graph
        Add-Edge -From A -To B -Graph $graph | Out-Null
        Add-Edge -From B -To A -Graph $graph | Out-Null

        { Get-GraphTopologicalSort -Graph $graph } | Should -Throw
    }
}
