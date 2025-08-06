# PSGraph

![PowerShell Gallery](https://img.shields.io/powershellgallery/v/PSQuickGraph?label=PSGallery) ![CI](https://github.com/<your-org>/PSGraph/actions/workflows/ci.yml/badge.svg) ![Licence](https://img.shields.io/github/license/eosfor/PSGraph)

> **PSGraph is a PowerShell-first wrapper around the ⚡ QuickGraph / QuikGraph** ecosystem.  
> It lets you **build, query and visualise graphs directly from the pipeline** without
> dropping down to C# or external tools. Think *LINQ for graphs* – but in PowerShell.

---

## Why another graph module?

QuickGraph/QuikGraph gives .NET a battle-tested set of graph data-structures and algorithms (DFS, BFS, Dijkstra, min-cut, spanning-tree, etc.) :contentReference[oaicite:0]{index=0}.  
PSGraph layers **PowerShell ergonomics, discoverability and visualisation** on top so that:

* You can **pipe any object collection** into a graph, specify what constitutes vertices
  and edges, and explore the relationships immediately.
* Results stay as native objects – ideal for further CLI
  processing, reporting or automation.

The original goal was to **analyse dependencies** in IaC workloads, but the module has proven handy for **network flows, security events, configuration drift** 

---

## Features at a glance

|                                   | What you get |
|-----------------------------------|--------------|
| **Idiomatic cmdlets**             | `New-PSGraph`, `Add-PSVertex`, `Add-PSEdge`, `Get-GraphPath`, … |
| **Ready-made algorithms**         | All algorithms exposed by QuikGraph are one cmdlet away. |
| **Batteries-included visualisation** | Render to GraphViz (`*.dot`, `png`, `svg`) or to JSON for Vega/D3 dashboards. |
| **Pipeline-friendly**             | Import/Export from CSV, JSON, XML, SQL, REST or live objects. |
| **Test-driven**                   | Over 100 Pester tests ensure every cmdlet does what it says. |
| **Cross-platform**                | Runs anywhere PowerShell 7+ does (Windows, Linux, macOS). |

---

## Quick install

```powershell
Install-Module -Name PSQuickGraph -Scope CurrentUser
```

**An example of Cartesian layouts for a node-link diagram of hierarchical data.**

```pwsh
$data = (Invoke-WebRequest -Uri https://raw.githubusercontent.com/vega/vega-datasets/refs/heads/main/data/flare.json).Content | ConvertFrom-Json
$index = [object[]]::new($data.count + 1)
$data | % { $index[$_.id] = $_ }

$g = new-graph

$data | Group-Object -Property parent | % {
    $currentGroup = $_
    if ([string]::IsNullOrEmpty($currentGroup.name)) {
        $currentGroup.Group | % {
            Add-Vertex -Vertex $_.name -Graph $g
        }
    }
    else {
        $parentLabel = $index[$currentGroup.name].name
        $currentGroup.Group | % {
            Add-Edge -From $parentLabel -To $_.name -Graph $g
        }
    }
}

$tempDir = [System.IO.Path]::GetTempPath() 
$outFile = Join-Path $tempDir 'x.tree.html'
Export-Graph -Graph $g -Format Vega_TreeLayout -Path $outFile
```

![tree](docs/img/visualization-3.svg)


**Same graph but woth Force Directed layout**

```pwsh
$tempDir = [System.IO.Path]::GetTempPath() 
$outFile = Join-Path $tempDir 'x.force.html'
Export-Graph -Graph $g -Format Vega_ForceDirected -Path $outFile
```

![tree](docs/img/visualization-4.svg)