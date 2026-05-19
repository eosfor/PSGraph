BeforeAll {
    . "$PSScriptRoot/PSGraph.TestBootstrap.ps1"
    Import-PSGraphTestModule

    function Skip-IfGraphTopologicalSortUnavailable {
        $allowMissingTopologicalSort = $env:PSGRAPH_ALLOW_MISSING_TOPOLOGICAL_SORT -eq '1'
        if ($allowMissingTopologicalSort -and -not (Get-Command -Name Get-GraphTopologicalSort -ErrorAction SilentlyContinue)) {
            Set-ItResult -Skipped -Because 'Get-GraphTopologicalSort is not exported by this installed PSQuickGraph version.'
            return $true
        }

        return $false
    }
}

Describe 'Get-GraphTopologicalSort' {
    It 'Should return source vertices before dependent vertices' {
        if (Skip-IfGraphTopologicalSortUnavailable) { return }

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
        if (Skip-IfGraphTopologicalSortUnavailable) { return }

        $graph = New-Graph
        Add-Edge -From A -To B -Graph $graph | Out-Null
        Add-Edge -From B -To C -Graph $graph | Out-Null

        $names = @(Get-GraphTopologicalSort -Graph $graph -Reverse | ForEach-Object Name)

        $names | Should -Be @('C', 'B', 'A')
    }

    It 'Should throw for cyclic graphs' {
        if (Skip-IfGraphTopologicalSortUnavailable) { return }

        $graph = New-Graph
        Add-Edge -From A -To B -Graph $graph | Out-Null
        Add-Edge -From B -To A -Graph $graph | Out-Null

        { Get-GraphTopologicalSort -Graph $graph } | Should -Throw
    }
}
