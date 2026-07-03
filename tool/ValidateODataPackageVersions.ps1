<#
.SYNOPSIS
Validates the package versions in the directory.packages.props file against the lower bound of the ODataLibPackageDependency version range in the builder.versions.settings.targets.

.PARAMETER builderVersionTargetsPath
Specifies the path to the builder.versions.settings.targets file.

.PARAMETER directoryPackagesPropsPath
Specifies the path to the directory.packages.props file.
#>

Param(
  [string]
  $builderVersionTargetsPath,
  [string]
  $directoryPackagesPropsPath
)

# Constants
$ODATA_LIB_PACKAGE_DEPENDENCY_KEY = "ODataLibPackageDependency"
$ODATA_MODEL_BUILDER_PACKAGE_DEPENDENCY_KEY = "ODataModelBuilderPackageDependency"
$BUILDER_VERSION_TARGETS_FILENAME = "builder.versions.settings.targets"
$DIRECTORY_PACKAGES_PROPS = "Directory.Packages.props"

# Path to your builder.versions.settings.targets file
if ($builderVersionTargetsPath -eq "") {
    $builderVersionTargetsPath = "$PSScriptRoot\$BUILDER_VERSION_TARGETS_FILENAME"
}

# Path to your directory.packages.props file
if ($directoryPackagesPropsPath -eq "") {
    $directoryPackagesPropsPath = Join-Path -Path (Split-Path -Path $PSScriptRoot -Parent) -ChildPath $DIRECTORY_PACKAGES_PROPS
}

# Function to extract the OData package dependency version range from the targets file
function Get-ODataPackageDependencyVersion {
    param (
        [xml]$targets,
        [string]$dependencyName
    )
    $dependencyVersionRange = [string]$targets.Project.PropertyGroup.$dependencyName
    $dependencyVersionRange = $dependencyVersionRange.Trim()
    $dependencyVersionRange = $dependencyVersionRange -replace "\s", ""

    # Extract the lower bound version from the version range
    $lowerBoundVersion = $dependencyVersionRange -replace "\[([^\]]+),.*", '$1'
    return $lowerBoundVersion
}

# Dictionary to store the OData package dependencies
$odataPackageDependenciesDict = [System.Collections.Generic.Dictionary[String,String]]::new()

# Load the targets file
[xml]$targets = Get-Content $builderVersionTargetsPath

# Extract the ODataLibPackageDependency version range
$lowerBoundVersion = Get-ODataPackageDependencyVersion -targets $targets -dependencyName $ODATA_LIB_PACKAGE_DEPENDENCY_KEY
$odataPackageDependenciesDict.Add($ODATA_LIB_PACKAGE_DEPENDENCY_KEY, $lowerBoundVersion)

# Extract the ODataModelBuilderPackageDependency version range
$lowerBoundVersion = Get-ODataPackageDependencyVersion -targets $targets -dependencyName $ODATA_MODEL_BUILDER_PACKAGE_DEPENDENCY_KEY
$odataPackageDependenciesDict.Add($ODATA_MODEL_BUILDER_PACKAGE_DEPENDENCY_KEY, $lowerBoundVersion)

# Extract the lower bound version from the version range
$lowerBoundVersion = $odataPackageDependenciesDict[$ODATA_LIB_PACKAGE_DEPENDENCY_KEY] 

# Load the directory.packages.props file
[xml]$packagesProps = Get-Content $directoryPackagesPropsPath

# Extract the PackageReference versions
$PackageReference = $packagesProps.Project.ItemGroup.PackageVersion

# Dictionary to store the PackageReferences
$csprojPackageReferencesDict = @{
    $ODATA_LIB_PACKAGE_DEPENDENCY_KEY = "Microsoft.OData.Core|Microsoft.OData.Client|Microsoft.OData.Edm|Microsoft.Spatial"
    $ODATA_MODEL_BUILDER_PACKAGE_DEPENDENCY_KEY = "Microsoft.OData.ModelBuilder"
}


foreach ($key in $odataPackageDependenciesDict.Keys) {
    $lowerBoundVersion = $odataPackageDependenciesDict[$key]
    $packageReferences = $PackageReference | Where-Object { $_.Include -match $csprojPackageReferencesDict[$key] }
    $packageVersions = $packageReferences.Version

    Write-Host "Validating the package versions of '$key' in '$DIRECTORY_PACKAGES_PROPS' against the lower bound of the '$key' version range in '$BUILDER_VERSION_TARGETS_FILENAME'."

    # Validate the versions
    foreach ($version in $packageVersions) {
        if ($version -ne $lowerBoundVersion) {
            $exception = New-Object System.Exception(
                "Error.VersionMismatch: '$key' version '$version' in '$directoryPackagesPropsPath' do not match the lower bound '$lowerBoundVersion' of '$key' in '$builderVersionTargetsPath'.")
            throw $exception
        }
    }

    Write-Host "Validation successful: Package versions match the lower bound of the $key version range."
}
