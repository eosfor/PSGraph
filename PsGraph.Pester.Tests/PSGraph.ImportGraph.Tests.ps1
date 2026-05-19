BeforeAll {
    . "$PSScriptRoot/PSGraph.TestBootstrap.ps1"
    Import-PSGraphTestModule
}

Describe 'Import-Graph CSV' {
    BeforeAll {
        $script:tempDir = Join-Path ([System.IO.Path]::GetTempPath()) ([System.Guid]::NewGuid().ToString())
        New-Item -ItemType Directory -Path $script:tempDir | Out-Null
    }

    AfterAll {
        Remove-Item -Recurse -Force $script:tempDir -ErrorAction SilentlyContinue
    }

    Context 'Basic CSV edge list' {
        It 'Imports vertices and edges from a simple CSV' {
            $csv = "From,To`nA,B`nB,C`nC,A"
            $path = Join-Path $script:tempDir 'basic.csv'
            Set-Content -Path $path -Value $csv -NoNewline

            $g = Import-Graph -Path $path -Format Csv

            $g.VertexCount | Should -Be 3
            $g.EdgeCount | Should -Be 3
        }

        It 'Imports Label and Weight columns' {
            $csv = "From,To,Label,Weight`nA,B,link,5`nB,C,route,3"
            $path = Join-Path $script:tempDir 'labelweight.csv'
            Set-Content -Path $path -Value $csv -NoNewline

            $g = Import-Graph -Path $path -Format Csv
            $edge = $g.Edges | Where-Object { $_.Source.Label -eq 'A' -and $_.Target.Label -eq 'B' }

            $edge.Label | Should -Be 'link'
            $edge.Weight | Should -Be 5
        }

        It 'Stores extra columns in RenderProperties' {
            $csv = "From,To,Color,Priority`nX,Y,red,1"
            $path = Join-Path $script:tempDir 'extra.csv'
            Set-Content -Path $path -Value $csv -NoNewline

            $g = Import-Graph -Path $path -Format Csv
            $edge = @($g.Edges)[0]

            $edge.RenderProperties['Color'] | Should -Be 'red'
            $edge.RenderProperties['Priority'] | Should -Be '1'
        }

        It 'Supports custom column names via -FromColumn/-ToColumn' {
            $csv = "Source,Target`nAlpha,Beta"
            $path = Join-Path $script:tempDir 'custom.csv'
            Set-Content -Path $path -Value $csv -NoNewline

            $g = Import-Graph -Path $path -Format Csv -FromColumn Source -ToColumn Target

            $g.VertexCount | Should -Be 2
            $g.Vertices.Label | Should -Contain 'Alpha'
            $g.Vertices.Label | Should -Contain 'Beta'
        }
    }

    Context 'Tab-delimited files' {
        It 'Imports TSV with -Delimiter tab' {
            $tsv = "From`tTo`nA`tB`nB`tC"
            $path = Join-Path $script:tempDir 'tab.tsv'
            Set-Content -Path $path -Value $tsv -NoNewline

            $g = Import-Graph -Path $path -Format Csv -Delimiter "`t"

            $g.VertexCount | Should -Be 3
            $g.EdgeCount | Should -Be 2
        }
    }

    Context 'Comment lines' {
        It 'Skips lines starting with #' {
            $csv = "# This is a comment`n# Another comment`nFrom,To`nA,B`nB,C"
            $path = Join-Path $script:tempDir 'comments.csv'
            Set-Content -Path $path -Value $csv -NoNewline

            $g = Import-Graph -Path $path -Format Csv

            $g.VertexCount | Should -Be 3
            $g.EdgeCount | Should -Be 2
        }
    }

    Context 'NoHeader mode' {
        It 'Uses first two columns positionally when -NoHeader is set' {
            $csv = "A,B`nB,C`nC,A"
            $path = Join-Path $script:tempDir 'noheader.csv'
            Set-Content -Path $path -Value $csv -NoNewline

            $g = Import-Graph -Path $path -Format Csv -NoHeader

            $g.VertexCount | Should -Be 3
            $g.EdgeCount | Should -Be 3
        }

        It 'Imports SNAP-style TSV (tab, no header, comments)' {
            $snap = "# Directed graph`n# Nodes: 3 Edges: 2`n# FromNodeId`tToNodeId`n30`t1412`n30`t3352"
            $path = Join-Path $script:tempDir 'snap.txt'
            Set-Content -Path $path -Value $snap -NoNewline

            $g = Import-Graph -Path $path -Format Csv -Delimiter "`t" -NoHeader

            $g.VertexCount | Should -Be 3
            $g.EdgeCount | Should -Be 2
            $g.Vertices.Label | Should -Contain '30'
            $g.Vertices.Label | Should -Contain '1412'
            $g.Vertices.Label | Should -Contain '3352'
        }
    }

    Context 'Vertex deduplication' {
        It 'Deduplicates vertices that appear in multiple edges' {
            $csv = "From,To`nA,B`nA,C`nB,C`nC,A"
            $path = Join-Path $script:tempDir 'dedup.csv'
            Set-Content -Path $path -Value $csv -NoNewline

            $g = Import-Graph -Path $path -Format Csv

            $g.VertexCount | Should -Be 3
            $g.EdgeCount | Should -Be 4
        }
    }

    Context 'Error handling' {
        It 'Throws on file not found' {
            { Import-Graph -Path "$($script:tempDir)/missing.csv" -Format Csv } |
                Should -Throw '*not found*'
        }

        It 'Throws when required column is missing' {
            $csv = "Name,Value`nA,1"
            $path = Join-Path $script:tempDir 'badcols.csv'
            Set-Content -Path $path -Value $csv -NoNewline

            { Import-Graph -Path $path -Format Csv } |
                Should -Throw '*From*'
        }
    }
}

Describe 'Import-Graph JSON' {
    BeforeAll {
        $script:tempDir2 = Join-Path ([System.IO.Path]::GetTempPath()) ([System.Guid]::NewGuid().ToString())
        New-Item -ItemType Directory -Path $script:tempDir2 | Out-Null
    }

    AfterAll {
        Remove-Item -Recurse -Force $script:tempDir2 -ErrorAction SilentlyContinue
    }

    Context 'Basic JSON with nodes and edges' {
        It 'Imports graph from standard JSON format' {
            $json = @'
{
  "nodes": [{"id": "A"}, {"id": "B"}, {"id": "C"}],
  "edges": [
    {"source": "A", "target": "B"},
    {"source": "B", "target": "C"}
  ]
}
'@
            $path = Join-Path $script:tempDir2 'basic.json'
            Set-Content -Path $path -Value $json -NoNewline

            $g = Import-Graph -Path $path -Format Json

            $g.VertexCount | Should -Be 3
            $g.EdgeCount | Should -Be 2
        }

        It 'Creates vertices automatically from edges-only JSON' {
            $json = '{"edges": [{"source": "X", "target": "Y"}, {"source": "Y", "target": "Z"}]}'
            $path = Join-Path $script:tempDir2 'edgesonly.json'
            Set-Content -Path $path -Value $json -NoNewline

            $g = Import-Graph -Path $path -Format Json

            $g.VertexCount | Should -Be 3
            $g.EdgeCount | Should -Be 2
        }

        It 'Reads node metadata' {
            $json = '{"nodes": [{"id": "A", "color": "red", "size": 10}], "edges": []}'
            $path = Join-Path $script:tempDir2 'meta.json'
            Set-Content -Path $path -Value $json -NoNewline

            $g = Import-Graph -Path $path -Format Json
            $v = $g.Vertices | Where-Object { $_.Label -eq 'A' }

            $v.Metadata['color'] | Should -Be 'red'
            $v.Metadata['size'] | Should -Be 10
        }

        It 'Reads edge label and weight' {
            $json = '{"edges": [{"source": "A", "target": "B", "label": "link", "weight": 5}]}'
            $path = Join-Path $script:tempDir2 'edgemeta.json'
            Set-Content -Path $path -Value $json -NoNewline

            $g = Import-Graph -Path $path -Format Json
            $edge = @($g.Edges)[0]

            $edge.Label | Should -Be 'link'
            $edge.Weight | Should -Be 5
        }
    }

    Context 'D3.js compatibility: links alias' {
        It 'Accepts "links" instead of "edges"' {
            $json = @'
{
  "nodes": [{"id": "A"}, {"id": "B"}],
  "links": [{"source": "A", "target": "B"}]
}
'@
            $path = Join-Path $script:tempDir2 'links.json'
            Set-Content -Path $path -Value $json -NoNewline

            $g = Import-Graph -Path $path -Format Json

            $g.VertexCount | Should -Be 2
            $g.EdgeCount | Should -Be 1
        }
    }

    Context 'D3.js compatibility: name property' {
        It 'Uses "name" as node label when "id" is absent' {
            $json = @'
{
  "nodes": [{"name": "Alice"}, {"name": "Bob"}],
  "edges": [{"source": "Alice", "target": "Bob"}]
}
'@
            $path = Join-Path $script:tempDir2 'names.json'
            Set-Content -Path $path -Value $json -NoNewline

            $g = Import-Graph -Path $path -Format Json

            $g.Vertices.Label | Should -Contain 'Alice'
            $g.Vertices.Label | Should -Contain 'Bob'
        }
    }

    Context 'D3.js compatibility: numeric index references' {
        It 'Resolves numeric source/target as node array indices' {
            $json = @'
{
  "nodes": [{"id": "X"}, {"id": "Y"}, {"id": "Z"}],
  "links": [
    {"source": 0, "target": 1},
    {"source": 1, "target": 2}
  ]
}
'@
            $path = Join-Path $script:tempDir2 'numeric.json'
            Set-Content -Path $path -Value $json -NoNewline

            $g = Import-Graph -Path $path -Format Json

            $g.VertexCount | Should -Be 3
            $g.EdgeCount | Should -Be 2

            $edges = @($g.Edges)
            ($edges | Where-Object { $_.Source.Label -eq 'X' -and $_.Target.Label -eq 'Y' }) | Should -Not -BeNullOrEmpty
            ($edges | Where-Object { $_.Source.Label -eq 'Y' -and $_.Target.Label -eq 'Z' }) | Should -Not -BeNullOrEmpty
        }
    }

    Context 'D3.js Les Miserables style' {
        It 'Imports D3 Les Miserables format with name, links, numeric indices, and group metadata' {
            $json = @'
{
  "nodes": [
    {"name": "Myriel", "group": 1},
    {"name": "Napoleon", "group": 1},
    {"name": "Valjean", "group": 2}
  ],
  "links": [
    {"source": 0, "target": 1, "value": 1},
    {"source": 0, "target": 2, "value": 8}
  ]
}
'@
            $path = Join-Path $script:tempDir2 'lesmis.json'
            Set-Content -Path $path -Value $json -NoNewline

            $g = Import-Graph -Path $path -Format Json

            $g.VertexCount | Should -Be 3
            $g.EdgeCount | Should -Be 2

            $myriel = $g.Vertices | Where-Object { $_.Label -eq 'Myriel' }
            $myriel.Metadata['group'] | Should -Be 1

            $valjeanEdge = $g.Edges | Where-Object {
                $_.Source.Label -eq 'Myriel' -and $_.Target.Label -eq 'Valjean'
            }
            $valjeanEdge | Should -Not -BeNullOrEmpty
            $valjeanEdge.RenderProperties['value'] | Should -Be 8
        }
    }

    Context 'Error handling' {
        It 'Throws on file not found' {
            { Import-Graph -Path "$($script:tempDir2)/missing.json" -Format Json } |
                Should -Throw '*not found*'
        }

        It 'Throws on invalid JSON' {
            $path = Join-Path $script:tempDir2 'invalid.json'
            Set-Content -Path $path -Value '{ not valid }}}' -NoNewline

            { Import-Graph -Path $path -Format Json } |
                Should -Throw
        }
    }
}
