[CmdletBinding()]
param(
    [string] $ModuleName = 'PSQuickGraph',

    [string] $RequiredVersion,

    [string] $ModuleManifestPath,

    [string] $OutputDirectory = (Join-Path $PWD 'artifacts/gallery-installed-e2e')
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

function Assert-Condition {
    param(
        [bool] $Condition,
        [string] $Message
    )

    if (-not $Condition) {
        throw $Message
    }
}

function Add-CheckResult {
    param(
        [System.Collections.Generic.List[object]] $Checks,
        [string] $Name,
        [string] $Status = 'Passed'
    )

    $Checks.Add([pscustomobject]@{
        Name = $Name
        Status = $Status
    }) | Out-Null

    Write-Host "${Status}: $Name"
}

New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null

$checks = [System.Collections.Generic.List[object]]::new()

$installedModule = $null

if (-not [string]::IsNullOrWhiteSpace($ModuleManifestPath)) {
    Assert-Condition (Test-Path -LiteralPath $ModuleManifestPath) "Module manifest '$ModuleManifestPath' was not found."
    $manifestPath = (Resolve-Path -LiteralPath $ModuleManifestPath).Path
}
else {
    $installedParameters = @{
        Name = $ModuleName
        ErrorAction = 'Stop'
    }

    if (-not [string]::IsNullOrWhiteSpace($RequiredVersion)) {
        $installedParameters.RequiredVersion = $RequiredVersion
        if ($RequiredVersion.Contains('-')) {
            $installedParameters.AllowPrerelease = $true
        }
    }

    $installedModule = Get-InstalledModule @installedParameters | Select-Object -First 1
    Assert-Condition ($null -ne $installedModule) "Installed module '$ModuleName' was not found."

    $manifestPath = Join-Path $installedModule.InstalledLocation "$ModuleName.psd1"
    if (-not (Test-Path -LiteralPath $manifestPath)) {
        $manifestPath = Get-ChildItem -LiteralPath $installedModule.InstalledLocation -Filter '*.psd1' |
            Select-Object -First 1 -ExpandProperty FullName
    }

    Assert-Condition (Test-Path -LiteralPath $manifestPath) "Module manifest was not found under '$($installedModule.InstalledLocation)'."
}

Import-Module -Name $manifestPath -Force -ErrorAction Stop
$module = Get-Module -Name $ModuleName -ErrorAction Stop
Add-CheckResult -Checks $checks -Name 'Import installed module'

$requiredCommands = @(
    'New-Graph',
    'Add-Vertex',
    'Add-Edge',
    'Get-GraphPath',
    'Test-GraphPath',
    'Import-Graph',
    'Export-Graph',
    'New-DSM',
    'Export-DSM',
    'Start-DSMClustering'
)

foreach ($commandName in $requiredCommands) {
    $command = Get-Command -Name $commandName -ErrorAction Stop
    Assert-Condition ($null -ne $command) "Command '$commandName' was not exported by '$ModuleName'."
}

Add-CheckResult -Checks $checks -Name 'Required commands exported'

$graph = New-Graph
'A', 'B', 'C', 'D' | ForEach-Object {
    Add-Vertex -Graph $graph -Vertex $_ | Out-Null
}

Add-Edge -From 'A' -To 'B' -Graph $graph | Out-Null
Add-Edge -From 'B' -To 'C' -Graph $graph | Out-Null
Add-Edge -From 'A' -To 'D' -Graph $graph | Out-Null
Add-Edge -From 'D' -To 'C' -Graph $graph | Out-Null

Assert-Condition ($graph.VertexCount -eq 4) "Expected 4 vertices, got $($graph.VertexCount)."
Assert-Condition ($graph.EdgeCount -eq 4) "Expected 4 edges, got $($graph.EdgeCount)."
Add-CheckResult -Checks $checks -Name 'Build graph with vertices and edges'

$path = @(Get-GraphPath -Graph $graph -From 'A' -To 'C')
Assert-Condition ($path.Count -gt 0) 'Expected Get-GraphPath to return at least one edge.'
Assert-Condition ([bool](Test-GraphPath -Graph $graph -From 'A' -To 'C')) 'Expected Test-GraphPath A -> C to be true.'
Assert-Condition (-not [bool](Test-GraphPath -Graph $graph -From 'C' -To 'A')) 'Expected Test-GraphPath C -> A to be false.'
Add-CheckResult -Checks $checks -Name 'Query graph paths'

if (Get-Command -Name 'Get-GraphTopologicalSort' -ErrorAction SilentlyContinue) {
    $topologicalOrder = @(Get-GraphTopologicalSort -Graph $graph | ForEach-Object Name)
    Assert-Condition ($topologicalOrder.Count -eq 4) "Expected 4 vertices in topological order, got $($topologicalOrder.Count)."
    Assert-Condition ($topologicalOrder.IndexOf('A') -lt $topologicalOrder.IndexOf('B')) 'Expected A before B in topological order.'
    Assert-Condition ($topologicalOrder.IndexOf('A') -lt $topologicalOrder.IndexOf('D')) 'Expected A before D in topological order.'
    Assert-Condition ($topologicalOrder.IndexOf('B') -lt $topologicalOrder.IndexOf('C')) 'Expected B before C in topological order.'
    Assert-Condition ($topologicalOrder.IndexOf('D') -lt $topologicalOrder.IndexOf('C')) 'Expected D before C in topological order.'
    Add-CheckResult -Checks $checks -Name 'Topological sort'
}
else {
    Add-CheckResult -Checks $checks -Name 'Topological sort' -Status 'Skipped'
}

$dot = [string]::Join([Environment]::NewLine, @(Export-Graph -Graph $graph -Format Graphviz))
Assert-Condition (-not [string]::IsNullOrWhiteSpace($dot)) 'Expected Graphviz export to return text.'
Assert-Condition ($dot.Contains('A')) 'Expected Graphviz export to contain vertex A.'
Assert-Condition ($dot.Contains('C')) 'Expected Graphviz export to contain vertex C.'

$graphMlPath = Join-Path $OutputDirectory 'smoke.graphml'
Export-Graph -Graph $graph -Format GraphML -Path $graphMlPath
Assert-Condition (Test-Path -LiteralPath $graphMlPath) "Expected GraphML export file at '$graphMlPath'."
Add-CheckResult -Checks $checks -Name 'Export graph formats'

$csvPath = Join-Path $OutputDirectory 'smoke.csv'
Set-Content -LiteralPath $csvPath -Value "From,To`nX,Y`nY,Z" -NoNewline
$importedGraph = Import-Graph -Path $csvPath -Format Csv
Assert-Condition ($importedGraph.VertexCount -eq 3) "Expected imported graph to have 3 vertices, got $($importedGraph.VertexCount)."
Assert-Condition ($importedGraph.EdgeCount -eq 2) "Expected imported graph to have 2 edges, got $($importedGraph.EdgeCount)."
Add-CheckResult -Checks $checks -Name 'Import CSV graph'

$dsm = New-DSM -Graph $graph
Assert-Condition ($null -ne $dsm) 'Expected New-DSM to return a DSM object.'

$dsmText = Export-DSM -Dsm $dsm -Format TEXT
Assert-Condition (-not [string]::IsNullOrWhiteSpace($dsmText)) 'Expected Export-DSM TEXT to return matrix text.'
Assert-Condition ($dsmText.Contains('1')) 'Expected DSM text export to contain at least one edge weight.'

$clusterResult = Start-DSMClustering -Dsm $dsm -ClusteringAlgorithm GraphBased -Detailed
Assert-Condition ($null -ne $clusterResult) 'Expected Start-DSMClustering to return a result.'
Assert-Condition ($null -ne $clusterResult.Dsm) 'Expected clustering result to include a DSM.'
Add-CheckResult -Checks $checks -Name 'DSM commands'

$result = [pscustomobject]@{
    ModuleName = $ModuleName
    RequestedVersion = $RequiredVersion
    ImportedVersion = $module.Version.ToString()
    InstalledVersion = if ($null -ne $installedModule) { $installedModule.Version.ToString() } else { $null }
    ModuleManifestPath = $manifestPath
    PowerShellVersion = $PSVersionTable.PSVersion.ToString()
    Platform = [System.Runtime.InteropServices.RuntimeInformation]::OSDescription
    Checks = $checks
}

$resultPath = Join-Path $OutputDirectory 'gallery-installed-smoke-results.json'
$result | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $resultPath
Write-Host "Wrote result summary to $resultPath"
