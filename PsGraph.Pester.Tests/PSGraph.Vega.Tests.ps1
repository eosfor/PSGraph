BeforeAll {
    Import-Module "./PSGraph.Tests/bin/Debug/net9.0/PSQuickGraph.psd1"
}
Describe 'Vega Tests' {
    It 'Vega Force Directed' {
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

        Export-Graph -Graph $graph -Format Vega_ForceDirected -Path '/tmp/x.force.html'
    }

    It 'Vega Tree Layout' {
        $graph = New-Graph

        # Корень
        Add-Vertex -Graph $graph -Vertex 'A'

        # Первый уровень
        Add-Vertex -Graph $graph -Vertex 'B'
        Add-Vertex -Graph $graph -Vertex 'C'

        # Второй уровень
        Add-Vertex -Graph $graph -Vertex 'D'
        Add-Vertex -Graph $graph -Vertex 'E'
        Add-Vertex -Graph $graph -Vertex 'F'

        # Связи (дерево)
        Add-Edge -From A -To B -Graph $graph | Out-Null
        Add-Edge -From A -To C -Graph $graph | Out-Null
        Add-Edge -From B -To D -Graph $graph | Out-Null
        Add-Edge -From B -To E -Graph $graph | Out-Null
        Add-Edge -From C -To F -Graph $graph | Out-Null

        Export-Graph -Graph $graph -Format Vega_TreeLayout -Path /tmp/x.tree.html -UseVirtualTreeRoot
    }

    It 'Vega Adjacency Matrix' {
        $graph = New-Graph

        # Вершины — отделы компании или сервисы
        'Auth', 'Payments', 'Orders', 'Users', 'Notifications', 'Analytics' | ForEach-Object {
            Add-Vertex -Graph $graph -Vertex $_
        }

        # Связи — вызовы или зависимости
        Add-Edge -From Auth -To Users -Graph $graph | Out-Null
        Add-Edge -From Payments -To Orders -Graph $graph | Out-Null
        Add-Edge -From Payments -To Auth -Graph $graph | Out-Null
        Add-Edge -From Orders -To Users -Graph $graph | Out-Null
        Add-Edge -From Orders -To Notifications -Graph $graph | Out-Null
        Add-Edge -From Notifications -To Users -Graph $graph | Out-Null
        Add-Edge -From Analytics -To Orders -Graph $graph | Out-Null
        Add-Edge -From Analytics -To Payments -Graph $graph | Out-Null
        Add-Edge -From Analytics -To Users -Graph $graph | Out-Null

        Export-Graph -Graph $graph -Format Vega_AdjacencyMatrix -Path /tmp/x.adj.matrix.html
    }
}
