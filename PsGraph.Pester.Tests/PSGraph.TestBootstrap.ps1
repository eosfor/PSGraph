function Import-PSGraphTestModule {
    [CmdletBinding()]
    param(
        [string] $ModuleName = $(if ([string]::IsNullOrWhiteSpace($env:PSGRAPH_TEST_MODULE_NAME)) { 'PSQuickGraph' } else { $env:PSGRAPH_TEST_MODULE_NAME }),

        [string] $RequiredVersion = $env:PSGRAPH_TEST_MODULE_VERSION,

        [string] $ModuleManifestPath = $env:PSGRAPH_TEST_MODULE_MANIFEST
    )

    $ErrorActionPreference = 'Stop'

    if (-not [string]::IsNullOrWhiteSpace($ModuleManifestPath)) {
        if (-not (Test-Path -LiteralPath $ModuleManifestPath)) {
            throw "PSGraph test module manifest '$ModuleManifestPath' was not found."
        }

        Import-Module -Name (Resolve-Path -LiteralPath $ModuleManifestPath).Path -Force -ErrorAction Stop
        return
    }

    if (-not [string]::IsNullOrWhiteSpace($RequiredVersion)) {
        $installedParameters = @{
            Name = $ModuleName
            RequiredVersion = $RequiredVersion
            ErrorAction = 'Stop'
        }

        if ($RequiredVersion.Contains('-')) {
            $installedParameters.AllowPrerelease = $true
        }

        $installedModule = Get-InstalledModule @installedParameters | Select-Object -First 1
        if ($null -eq $installedModule) {
            throw "PSGraph test module '$ModuleName' $RequiredVersion was not found."
        }

        $manifestPath = Join-Path $installedModule.InstalledLocation "$ModuleName.psd1"
        if (-not (Test-Path -LiteralPath $manifestPath)) {
            $manifestPath = Get-ChildItem -LiteralPath $installedModule.InstalledLocation -Filter '*.psd1' |
                Select-Object -First 1 -ExpandProperty FullName
        }

        if (-not (Test-Path -LiteralPath $manifestPath)) {
            throw "PSGraph test module manifest was not found under '$($installedModule.InstalledLocation)'."
        }

        Import-Module -Name $manifestPath -Force -ErrorAction Stop
        return
    }

    $defaultManifestPath = Join-Path $PSScriptRoot '../PSGraph.Tests/bin/Debug/net9.0/PSQuickGraph.psd1'
    if (Test-Path -LiteralPath $defaultManifestPath) {
        Import-Module -Name (Resolve-Path -LiteralPath $defaultManifestPath).Path -Force -ErrorAction Stop
        return
    }

    Import-Module -Name $ModuleName -Force -ErrorAction Stop
}
