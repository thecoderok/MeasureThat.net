# Get the script directory and project paths
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectPath = Join-Path $scriptDir "src\BenchmarkLab\BenchmarkLab.csproj"

# Load the .csproj file
[xml]$csproj = Get-Content $projectPath

# Function to increment version
function Increment-Version($version) {
    $versionParts = $version -split '\.'
    if ($versionParts.Length -lt 3) {
        $versionParts += 0
    }
    $versionParts[2] = [int]$versionParts[2] + 1
    return $versionParts -join '.'
}

# Initialize flags to check if properties are found
$versionFound = $false
$assemblyVersionFound = $false
$fileVersionFound = $false

# Iterate through each PropertyGroup to find and update Version, AssemblyVersion, and FileVersion
foreach ($propertyGroup in $csproj.Project.PropertyGroup) {
    if ($propertyGroup.Version) {
        $propertyGroup.Version = Increment-Version $propertyGroup.Version
        $versionFound = $true
    }
    if ($propertyGroup.AssemblyVersion) {
        $propertyGroup.AssemblyVersion = Increment-Version $propertyGroup.AssemblyVersion
        $assemblyVersionFound = $true
    }
    if ($propertyGroup.FileVersion) {
        $propertyGroup.FileVersion = Increment-Version $propertyGroup.FileVersion
        $fileVersionFound = $true
    }
}

# If any of the properties were not found, throw an error
if (-not $versionFound) {
    throw "The property 'Version' cannot be found in any PropertyGroup."
}
if (-not $assemblyVersionFound) {
    throw "The property 'AssemblyVersion' cannot be found in any PropertyGroup."
}
if (-not $fileVersionFound) {
    throw "The property 'FileVersion' cannot be found in any PropertyGroup."
}

# Save the updated .csproj file
$csproj.Save($projectPath)

Write-Output "Project file path: $projectPath"
Write-Output "Version updated to $($propertyGroup.Version)"
Write-Output "AssemblyVersion updated to $($propertyGroup.AssemblyVersion)"
Write-Output "FileVersion updated to $($propertyGroup.FileVersion)"
