# Import-PublicDatasets.ps1
# Demo: download and import several well-known public graph datasets
# using Import-Graph with CSV and JSON formats.
#
# Datasets used:
#   1. SNAP wiki-Vote      -- directed, ~7k nodes, ~100k edges (TSV, no header, comments)
#   2. D3.js Les Miserables -- undirected, 77 nodes, 254 links (JSON, numeric indices, "name"/"links")
#   3. Network Repository Karate Club -- undirected, 34 nodes, 78 edges (CSV, comma, no header)

param(
    [string]$OutputDir = (Join-Path ([System.IO.Path]::GetTempPath()) 'PSGraph-datasets')
)

Import-Module "./PSGraph.Tests/bin/Debug/net9.0/PSQuickGraph.psd1" -Force

if (-not (Test-Path $OutputDir)) {
    New-Item -ItemType Directory -Path $OutputDir | Out-Null
}

Write-Host "`n=== PSGraph Public Dataset Import Demo ===" -ForegroundColor Cyan

# ---------------------------------------------------------------
# 1. SNAP wiki-Vote (directed voting network)
#    https://snap.stanford.edu/data/wiki-Vote.html
#    Format: tab-separated, comment lines starting with #, no header
# ---------------------------------------------------------------
Write-Host "`n--- 1. SNAP wiki-Vote ---" -ForegroundColor Yellow

$wikiVoteUrl  = 'https://snap.stanford.edu/data/wiki-Vote.txt.gz'
$wikiVoteGz   = Join-Path $OutputDir 'wiki-Vote.txt.gz'
$wikiVoteTxt  = Join-Path $OutputDir 'wiki-Vote.txt'

if (-not (Test-Path $wikiVoteTxt)) {
    Write-Host "Downloading wiki-Vote dataset..."
    Invoke-WebRequest -Uri $wikiVoteUrl -OutFile $wikiVoteGz

    # Decompress .gz
    $inStream  = [System.IO.File]::OpenRead($wikiVoteGz)
    $gzStream  = [System.IO.Compression.GZipStream]::new($inStream, [System.IO.Compression.CompressionMode]::Decompress)
    $outStream = [System.IO.File]::Create($wikiVoteTxt)
    $gzStream.CopyTo($outStream)
    $outStream.Close(); $gzStream.Close(); $inStream.Close()
    Remove-Item $wikiVoteGz -ErrorAction SilentlyContinue

    Write-Host "Saved to $wikiVoteTxt"
} else {
    Write-Host "Using cached $wikiVoteTxt"
}

$wikiGraph = Import-Graph -Path $wikiVoteTxt -Format Csv -Delimiter "`t" -NoHeader
Write-Host "wiki-Vote: $($wikiGraph.VertexCount) vertices, $($wikiGraph.EdgeCount) edges"

# ---------------------------------------------------------------
# 2. D3.js Les Miserables (co-appearance network)
#    https://raw.githubusercontent.com/d3/d3-plugins/master/graph/data/miserables.json
#    Format: JSON with "nodes" (name, group) and "links" (source/target as numeric indices)
# ---------------------------------------------------------------
Write-Host "`n--- 2. D3.js Les Miserables ---" -ForegroundColor Yellow

$lesMisUrl  = 'https://raw.githubusercontent.com/d3/d3-plugins/master/graph/data/miserables.json'
$lesMisJson = Join-Path $OutputDir 'miserables.json'

if (-not (Test-Path $lesMisJson)) {
    Write-Host "Downloading Les Miserables dataset..."
    Invoke-WebRequest -Uri $lesMisUrl -OutFile $lesMisJson
    Write-Host "Saved to $lesMisJson"
} else {
    Write-Host "Using cached $lesMisJson"
}

$lesMisGraph = Import-Graph -Path $lesMisJson -Format Json
Write-Host "Les Miserables: $($lesMisGraph.VertexCount) vertices, $($lesMisGraph.EdgeCount) edges"

# Show a few characters and their groups
$lesMisGraph.Vertices |
    Select-Object Label, @{N='Group'; E={ $_.Metadata['group'] }} |
    Sort-Object Group, Label |
    Select-Object -First 10 |
    Format-Table -AutoSize

# ---------------------------------------------------------------
# 3. Zachary's Karate Club (social network)
#    https://networkrepository.com/soc-karate.php
#    Format: CSV, no header, space-delimited, 34 nodes, 78 edges
# ---------------------------------------------------------------
Write-Host "`n--- 3. Zachary's Karate Club ---" -ForegroundColor Yellow

$karateUrl = 'https://nrvis.com/download/data/soc/soc-karate.zip'
$karateZip = Join-Path $OutputDir 'soc-karate.zip'
$karateMtx = Join-Path $OutputDir 'soc-karate.mtx'

if (-not (Test-Path $karateMtx)) {
    Write-Host "Downloading Karate Club dataset..."
    Invoke-WebRequest -Uri $karateUrl -OutFile $karateZip

    # Extract the .mtx file from the zip
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $zip = [System.IO.Compression.ZipFile]::OpenRead($karateZip)
    $entry = $zip.Entries | Where-Object { $_.Name -like '*.mtx' } | Select-Object -First 1
    if ($entry) {
        [System.IO.Compression.ZipFileExtensions]::ExtractToFile($entry, $karateMtx, $true)
    }
    $zip.Dispose()
    Remove-Item $karateZip -ErrorAction SilentlyContinue

    Write-Host "Saved to $karateMtx"
} else {
    Write-Host "Using cached $karateMtx"
}

# The .mtx file uses space-separated edge list with % comments
$karateGraph = Import-Graph -Path $karateMtx -Format MatrixMarket
Write-Host "Karate Club: $($karateGraph.VertexCount) vertices, $($karateGraph.EdgeCount) edges"

# ---------------------------------------------------------------
# Summary
# ---------------------------------------------------------------
Write-Host "`n=== Summary ===" -ForegroundColor Cyan
@(
    [PSCustomObject]@{ Dataset = 'wiki-Vote';       Vertices = $wikiGraph.VertexCount;   Edges = $wikiGraph.EdgeCount;   Format = 'TSV (SNAP)' }
    [PSCustomObject]@{ Dataset = 'Les Miserables';  Vertices = $lesMisGraph.VertexCount; Edges = $lesMisGraph.EdgeCount; Format = 'JSON (D3)' }
    [PSCustomObject]@{ Dataset = 'Karate Club';     Vertices = $karateGraph.VertexCount; Edges = $karateGraph.EdgeCount; Format = 'MTX (space)' }
) | Format-Table -AutoSize

Write-Host "Files cached in: $OutputDir`n"
