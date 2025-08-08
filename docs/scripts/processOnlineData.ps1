
$data = (Invoke-WebRequest -Uri 'https://raw.githubusercontent.com/vega/vega-datasets/refs/heads/main/data/flare.json').Content | ConvertFrom-Json
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