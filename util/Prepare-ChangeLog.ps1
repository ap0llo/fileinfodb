﻿<#
    Script intended to help generate a change log for a release.

    The script will prompt for the version range to generate the log
    for (this will usually be the latest and the previous version)    
    and then write the commit messages of all the commit that
    were made between the two versions to a text file.    
    This text file can then be used as basis for writing release notes.

    The script assumes that the versions are stored in the repository
    as tags in the format "v<VERSION>"
#>

$gitArgs = "--oneline --no-decorate --no-merges --reverse"

# Get the version range to generate the change log for
$currentVersionString = Read-Host -Prompt "Enter version to get changelog for (newer/latest version)"
$previousVersionString = Read-Host -Prompt "Enter previous version" 

# if entered values can be parsed as version,
# ensure current version is newer than previous version
$currentVersion = $currentVersionString -as [System.Version]
$previousVersion = $previousVersionString -as [System.Version]        
if(($currentVersion -ne $null) -and ($previousVersion -ne $null))
{
    if($currentVersion -le $previousVersion)
    {
        throw "Specified current version '$($currentVersionString)' is older than previous version '$($previousVersionString)'"
    }
}

# Run git fetch to ensure tags are up-to-date
Write-Host "Fetching git tags"
Invoke-Expression "git fetch --tags"

# Get git changes
Write-Host "Generating change log"
$gitOutput = Invoke-Expression "git log v$previousVersionString..v$currentVersionString $gitArgs" 
if($LASTEXITCODE -ne 0)
{
    throw "Failed to get changes between tags v$previousVersionString and v$currentVersionString"
}

# Save changes to file
$outFile = "Changelog_$currentVersionString.txt"
Out-File -InputObject $gitOutput -FilePath $outFile
Write-Host "git changes written to $outFile"


