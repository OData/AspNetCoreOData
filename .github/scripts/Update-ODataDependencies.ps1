[CmdletBinding()]
param(
    [string] $PackagesFile = "Directory.Packages.props",
    [string] $NuGetFlatContainerBaseUrl = "https://api.nuget.org/v3-flatcontainer"
)

$ErrorActionPreference = "Stop"

function Get-DependencySettings {
    param(
        [string] $PackagesContent,
        [string] $VersionPropertyName,
        [string] $DependencyPropertyName
    )

    $versionMatch = [regex]::Match(
        $PackagesContent,
        "<$VersionPropertyName>(?<Version>[^<]+)</$VersionPropertyName>")
    $dependencyMatch = [regex]::Match(
        $PackagesContent,
        "<$DependencyPropertyName>\[\$\($VersionPropertyName\),\s*(?<Maximum>[^\)]+)\)</$DependencyPropertyName>")

    if ($versionMatch.Success -and $dependencyMatch.Success) {
        return @{
            Version = [version] $versionMatch.Groups["Version"].Value
            Maximum = [version] $dependencyMatch.Groups["Maximum"].Value
        }
    }

    throw "Could not parse $VersionPropertyName and $DependencyPropertyName from $PackagesFile."
}

function Get-StablePackageVersions {
    param(
        [string] $PackageId,
        [version] $MaximumVersion
    )

    $packagePath = $PackageId.ToLowerInvariant()
    $response = Invoke-RestMethod "$NuGetFlatContainerBaseUrl/$packagePath/index.json"

    return @(
        $response.versions |
            Where-Object { $_ -match "^\d+(?:\.\d+){1,3}$" -and [version] $_ -lt $MaximumVersion } |
            ForEach-Object { [version] $_ } |
            Sort-Object -Descending
    )
}

function Get-LatestCommonVersion {
    param(
        [string[]] $PackageIds,
        [version] $MinimumVersion,
        [version] $MaximumVersion
    )

    $commonVersions = $null

    foreach ($packageId in $PackageIds) {
        $versions = @(Get-StablePackageVersions -PackageId $packageId -MaximumVersion $MaximumVersion)
        $versionSet = [System.Collections.Generic.HashSet[string]]::new(
            [string[]] ($versions | ForEach-Object { $_.ToString() }),
            [System.StringComparer]::OrdinalIgnoreCase)

        if ($null -eq $commonVersions) {
            $commonVersions = $versionSet
        }
        else {
            $commonVersions.IntersectWith($versionSet)
        }
    }

    $latestVersion = $commonVersions |
        ForEach-Object { [version] $_ } |
        Where-Object { $_ -ge $MinimumVersion } |
        Sort-Object -Descending |
        Select-Object -First 1

    if ($null -eq $latestVersion) {
        throw "No stable version shared by $($PackageIds -join ', ') was found in [$MinimumVersion, $MaximumVersion)."
    }

    return $latestVersion
}

function Set-VersionProperty {
    param(
        [string] $Content,
        [string] $PropertyName,
        [version] $Version
    )

    $pattern = "(<$PropertyName>)[^<]+(</$PropertyName>)"
    if (-not [regex]::IsMatch($Content, $pattern)) {
        throw "Could not find $PropertyName in $PackagesFile."
    }

    return [regex]::Replace($Content, $pattern, "`${1}$Version`${2}", 1)
}

$originalPackagesContent = Get-Content $PackagesFile -Raw
$packagesContent = $originalPackagesContent

$odataLibSettings = Get-DependencySettings `
    -PackagesContent $packagesContent -VersionPropertyName "ODataLibPackageVersion" -DependencyPropertyName "ODataLibPackageDependency"
Write-Host "ODataLib settings: $($odataLibSettings | Out-String)"

$modelBuilderSettings = Get-DependencySettings `
    -PackagesContent $packagesContent -VersionPropertyName "ODataModelBuilderPackageVersion" -DependencyPropertyName "ODataModelBuilderPackageDependency"
Write-Host "ModelBuilder settings: $($modelBuilderSettings | Out-String)"

$odataLibVersion = Get-LatestCommonVersion `
    -PackageIds @("Microsoft.OData.Core", "Microsoft.OData.Edm", "Microsoft.Spatial") -MinimumVersion $odataLibSettings.Version -MaximumVersion $odataLibSettings.Maximum
$modelBuilderVersion = Get-LatestCommonVersion `
    -PackageIds @("Microsoft.OData.ModelBuilder") -MinimumVersion $modelBuilderSettings.Version -MaximumVersion $modelBuilderSettings.Maximum

$updatedPackagesContent = Set-VersionProperty `
    -Content $packagesContent -PropertyName "ODataLibPackageVersion" -Version $odataLibVersion
$updatedPackagesContent = Set-VersionProperty `
    -Content $updatedPackagesContent -PropertyName "ODataModelBuilderPackageVersion" -Version $modelBuilderVersion

$changed = $updatedPackagesContent -ne $originalPackagesContent

if ($changed) {
    Set-Content $PackagesFile $updatedPackagesContent -NoNewline
}

Write-Host "Microsoft.OData.Core/Edm/Spatial: $odataLibVersion"
Write-Host "Microsoft.OData.ModelBuilder: $modelBuilderVersion"
Write-Host "Changed: $changed"

if ($env:GITHUB_OUTPUT) {
    "changed=$($changed.ToString().ToLowerInvariant())" >> $env:GITHUB_OUTPUT
    "previous-odata-lib-version=$($odataLibSettings.Version)" >> $env:GITHUB_OUTPUT
    "odata-lib-version=$odataLibVersion" >> $env:GITHUB_OUTPUT
    "previous-model-builder-version=$($modelBuilderSettings.Version)" >> $env:GITHUB_OUTPUT
    "model-builder-version=$modelBuilderVersion" >> $env:GITHUB_OUTPUT
}
