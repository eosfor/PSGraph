BeforeAll {
    Import-Module "./PSGraph.Tests/bin/Debug/net9.0/PSQuickGraph.psd1"
}

Describe 'New-Graph' {
    It 'Given no parameters, greates an empty graph object' {
        $graph = New-Graph
        $graph| Should -Not -BeNullOrEmpty
    }

    It 'Basic usage test: add edges to existing vertices' {
        $graph = New-Graph

        Add-Vertex -Graph $graph -Vertex 'A'
        Add-Vertex -Graph $graph -Vertex 'D'
        Add-Vertex -Graph $graph -Vertex 'E'
        Add-Vertex -Graph $graph -Vertex 'F'
        Add-Vertex -Graph $graph -Vertex 'G'
        Add-Vertex -Graph $graph -Vertex 'M'
        Add-Vertex -Graph $graph -Vertex 'B'
        Add-Vertex -Graph $graph -Vertex 'H'
        Add-Vertex -Graph $graph -Vertex 'I'

        Add-Edge -From A -To D -Graph $graph | Out-Null
        Add-Edge -From A -To E -Graph $graph | Out-Null
        Add-Edge -From D -to F -Graph $graph | Out-Null
        Add-Edge -From E -To F -Graph $graph | Out-Null
        Add-Edge -From G -To M -Graph $graph | Out-Null
        Add-Edge -From B -To E -Graph $graph | Out-Null
        Add-Edge -From B -To G -Graph $graph | Out-Null
        Add-Edge -From B -To H -Graph $graph | Out-Null
        Add-Edge -From H -To I -Graph $graph | Out-Null
        Add-Edge -From M -To B -Graph $graph | Out-Null
    }

    It 'Basic usage test: add edges creating new vertices' {
        $graph = New-Graph

        Add-Edge -From A -To D -Graph $graph | Out-Null
        Add-Edge -From A -To E -Graph $graph | Out-Null
        Add-Edge -From D -to F -Graph $graph | Out-Null
        Add-Edge -From E -To F -Graph $graph | Out-Null
        Add-Edge -From G -To M -Graph $graph | Out-Null
        Add-Edge -From B -To E -Graph $graph | Out-Null
        Add-Edge -From B -To G -Graph $graph | Out-Null
        Add-Edge -From B -To H -Graph $graph | Out-Null
        Add-Edge -From H -To I -Graph $graph | Out-Null
        Add-Edge -From M -To B -Graph $graph | Out-Null
    }
}