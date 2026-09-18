<#
.SYNOPSIS
Validates that OData package versions and dependency ranges use the version properties in Directory.Packages.props.

.PARAMETER directoryPackagesPropsPath
Specifies the path to the Directory.Packages.props file.
#>

Param(
    [string] $directoryPackagesPropsPath
)

$ErrorActionPreference = "Stop"

$DIRECTORY_PACKAGES_PROPS = "Directory.Packages.props"

if ([string]::IsNullOrWhiteSpace($directoryPackagesPropsPath)) {
    $directoryPackagesPropsPath = Join-Path `
        -Path (Split-Path -Path $PSScriptRoot -Parent) -ChildPath $DIRECTORY_PACKAGES_PROPS
}

[xml] $packagesProps = Get-Content $directoryPackagesPropsPath
$packageVersions = @($packagesProps.Project.ItemGroup.PackageVersion)

$dependencyGroups = @(
    @{
        VersionProperty = "ODataLibPackageVersion"
        DependencyProperty = "ODataLibPackageDependency"
        PackageIds = @(
            "Microsoft.OData.Core",
            "Microsoft.OData.Edm",
            "Microsoft.Spatial"
        )
    },
    @{
        VersionProperty = "ODataModelBuilderPackageVersion"
        DependencyProperty = "ODataModelBuilderPackageDependency"
        PackageIds = @("Microsoft.OData.ModelBuilder")
    }
)

foreach ($dependencyGroup in $dependencyGroups) {
    $versionProperty = $dependencyGroup.VersionProperty
    $dependencyProperty = $dependencyGroup.DependencyProperty
    $configuredVersion = [string] $packagesProps.Project.PropertyGroup.$versionProperty
    $dependencyRange = [string] $packagesProps.Project.PropertyGroup.$dependencyProperty
    $versionPropertyExpression = "`$($versionProperty)"

    if ([string]::IsNullOrWhiteSpace($configuredVersion)) {
        throw "Could not find $versionProperty in $directoryPackagesPropsPath."
    }

    try {
        [void] [version] $configuredVersion
    }
    catch {
        throw "$versionProperty value '$configuredVersion' is not a valid stable numeric version."
    }

    if ($dependencyRange -notmatch "^\[\$\($([regex]::Escape($versionProperty))\),\s*[^\)]+\)$") {
        throw "$dependencyProperty value '$dependencyRange' must use $versionPropertyExpression as its lower bound."
    }

    foreach ($packageId in $dependencyGroup.PackageIds) {
        $packageVersion = $packageVersions |
            Where-Object { $_.Include -eq $packageId } |
            Select-Object -First 1

        if ($null -eq $packageVersion) {
            throw "Could not find PackageVersion for $packageId in $directoryPackagesPropsPath."
        }

        if ($packageVersion.Version -ne $versionPropertyExpression) {
            throw "PackageVersion for $packageId must use $versionPropertyExpression instead of '$($packageVersion.Version)'."
        }
    }

    Write-Host "$dependencyProperty and its package versions use $versionPropertyExpression ($configuredVersion)."
}
