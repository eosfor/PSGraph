$ErrorActionPreference = 'Stop'

$g = New-Graph

$a = [pscustomobject]@{ type = 'value'; Name = 'a'; data = 2.0; grad = 6.0 }
$b = [pscustomobject]@{ type = 'value'; Name = 'b'; data = -3.0; grad = -4.0 }
$mul = [pscustomobject]@{ type = 'operation'; Name = '*' }
$e = [pscustomobject]@{ type = 'value'; Name = 'e'; data = -6.0; grad = -2.0 }

Add-Edge -Graph $g -From $a -To $mul | Out-Null
Add-Edge -Graph $g -From $b -To $mul | Out-Null
Add-Edge -Graph $g -From $mul -To $e | Out-Null

$dot = Export-Graph -Graph $g -Format Graphviz `
    -GraphScript {
        @{
            rankdir = 'LR'
            label = 'Micrograd'
        }
    } `
    -VertexScript {
        $item = $_

        switch ($item.type) {
            'value' {
                @{
                    label = "{ $($item.Name) | data $('{0:F4}' -f $item.data) | grad $('{0:F4}' -f $item.grad) }"
                    shape = 'record'
                }
            }
            'operation' {
                @{
                    label = $item.Name
                    shape = 'ellipse'
                }
            }
        }
    }

$dot

# If PSGraphView is available:
# $svg = Export-GraphvizView -InputObject $dot -Renderer Dot -As Svg
# Display $svg 'image/svg+xml'
