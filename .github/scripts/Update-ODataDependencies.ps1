[CmdletBinding()]
param(
    [string] $PackagesFile = "Directory.Packages.props",
    [string] $NuGetBaseUrl = "https://api.nuget.org/v3-flatcontainer"
)

$ErrorActionPreference = "Stop"

function Get-LatestCommonVersion {
    param(
        [string[]] $PackageIds,
        [version] $CurrentVersion,
        [version] $MaximumVersion
    )

    $commonVersions = $null

    foreach ($packageId in $PackageIds) {
        $packageUrl = "$NuGetBaseUrl/$($packageId.ToLowerInvariant())/index.json"
        $versions = @(
            (Invoke-RestMethod $packageUrl).versions |
                Where-Object { $_ -match "^\d+(?:\.\d+){1,3}$" } |
                ForEach-Object { [version] $_ } |
                Where-Object { $_ -ge $CurrentVersion -and $_ -lt $MaximumVersion }
        )

        $commonVersions = if ($null -eq $commonVersions) {
            $versions
        }
        else {
            @($commonVersions | Where-Object { $versions -contains $_ })
        }
    }

    $latestVersion = $commonVersions | Sort-Object -Descending | Select-Object -First 1
    if ($null -eq $latestVersion) {
        throw "No stable version shared by $($PackageIds -join ', ') was found in [$CurrentVersion, $MaximumVersion)."
    }

    return $latestVersion
}

function Get-MaximumVersion {
    param([string] $DependencyRange)

    if ($DependencyRange -notmatch ",\s*(?<Maximum>[^\)]+)\)$") {
        throw "Invalid dependency range: $DependencyRange"
    }

    return [version] $Matches["Maximum"]
}

[xml] $packages = Get-Content $PackagesFile
$properties = $packages.Project.PropertyGroup
$originalContent = Get-Content $PackagesFile -Raw

$previousODataLibVersion = [version] $properties.ODataLibPackageVersion
$previousModelBuilderVersion = [version] $properties.ODataModelBuilderPackageVersion

$odataLibVersion = Get-LatestCommonVersion `
    -PackageIds @("Microsoft.OData.Core", "Microsoft.OData.Edm", "Microsoft.Spatial") -CurrentVersion $previousODataLibVersion -MaximumVersion (Get-MaximumVersion $properties.ODataLibPackageDependency)
$modelBuilderVersion = Get-LatestCommonVersion `
    -PackageIds @("Microsoft.OData.ModelBuilder") -CurrentVersion $previousModelBuilderVersion -MaximumVersion (Get-MaximumVersion $properties.ODataModelBuilderPackageDependency)

$updatedContent = [regex]::Replace(
    $originalContent, "(<ODataLibPackageVersion>)[^<]+", "`${1}$odataLibVersion", 1)
$updatedContent = [regex]::Replace(
    $updatedContent, "(<ODataModelBuilderPackageVersion>)[^<]+", "`${1}$modelBuilderVersion", 1)
$odataLibChanged = $odataLibVersion -ne $previousODataLibVersion
$modelBuilderChanged = $modelBuilderVersion -ne $previousModelBuilderVersion
$changed = $odataLibChanged -or $modelBuilderChanged

if ($changed) {
    Set-Content $PackagesFile $updatedContent -NoNewline
}

Write-Host "ODataLib: $previousODataLibVersion -> $odataLibVersion"
Write-Host "ModelBuilder: $previousModelBuilderVersion -> $modelBuilderVersion"

if ($env:GITHUB_OUTPUT) {
    "changed=$($changed.ToString().ToLowerInvariant())" >> $env:GITHUB_OUTPUT
    "odata-lib-changed=$($odataLibChanged.ToString().ToLowerInvariant())" >> $env:GITHUB_OUTPUT
    "previous-odata-lib-version=$previousODataLibVersion" >> $env:GITHUB_OUTPUT
    "odata-lib-version=$odataLibVersion" >> $env:GITHUB_OUTPUT
    "model-builder-changed=$($modelBuilderChanged.ToString().ToLowerInvariant())" >> $env:GITHUB_OUTPUT
    "previous-model-builder-version=$previousModelBuilderVersion" >> $env:GITHUB_OUTPUT
    "model-builder-version=$modelBuilderVersion" >> $env:GITHUB_OUTPUT
}
