param(
    [string[]] $PackagesToRemove
)

function IsUpgradeRequired($oldPackageVersion, $packageVersion) {
    #handles 13.1.7340-preview or 13.1.7340-beta versions
    $oldPackageVersion = $oldPackageVersion.split('-')[0];
    $packageVersion = $packageVersion.split('-')[0];

    return [System.Version]$packageVersion -gt [System.Version]$oldPackageVersion
}

function Remove-Packages {
    param(
        [string] $ProjectName,
        [string[]] $PackagesToRemove
    )

    "`nRemoving deprecated packages for '$ProjectName'"
    foreach ($packageName in $PackagesToRemove) {
        $deprecatedPackageMatches = Invoke-Expression "Get-Package `"$($packageName)`" -ProjectName `"$ProjectName`"" 
        $deprecatedPackage = $deprecatedPackageMatches | Where-Object { $_.Id -eq $packageName } | Select-Object -First 1
        if ($null -ne $deprecatedPackage) {
            "`nUninstalling package: '$($deprecatedPackage.Id)' from `"$ProjectName`""
            Invoke-Expression "Uninstall-Package `"$($deprecatedPackage.Id)`" -ProjectName `"$ProjectName`" -RemoveDependencies -Force" 
        }
    }
}

# Known major dependency transitions by Sitefinity version boundary.
# When upgrading across these boundaries, transitive dependencies change major
# versions. Because Update-Package processes packages one at a time in
# packages.config projects, the old major-version packages conflict with the
# new ones before the full set is updated. Removing them first avoids resolver
# failures.
$KnownDependencyTransitions = @{
    "15.4.8626" = @(
        @{ Prefix = "AWSSDK.";       OldMajorVersion = 3 }
        @{ Prefix = "ServiceStack."; OldMajorVersion = 8 }
    )
}

function Remove-ConflictingTransitiveDependencies {
    param(
        [string] $ProjectName,
        [array]  $Packages
    )

    # Determine the highest target version from the upgrade config
    $highestTargetVersion = $null
    foreach ($package in $Packages) {
        $ver = $package.version.split('-')[0]
        $parsed = [System.Version]$ver
        if ($null -eq $highestTargetVersion -or $parsed -gt $highestTargetVersion) {
            $highestTargetVersion = $parsed
        }
    }

    if ($null -eq $highestTargetVersion) {
        return
    }

    # Collect all transitions that apply (target version >= boundary)
    $applicableTransitions = @()
    foreach ($boundary in $KnownDependencyTransitions.Keys) {
        $boundaryVersion = [System.Version]$boundary
        if ($highestTargetVersion -ge $boundaryVersion) {
            $applicableTransitions += $KnownDependencyTransitions[$boundary]
        }
    }

    if ($applicableTransitions.Count -eq 0) {
        return
    }

    "`nChecking for conflicting transitive dependencies in '$ProjectName'..."
    $projectPackages = Get-Package -ProjectName $ProjectName

    foreach ($transition in $applicableTransitions) {
        $prefix = $transition.Prefix
        $oldMajor = $transition.OldMajorVersion

        $conflictingPackages = $projectPackages | Where-Object {
            $_.Id.StartsWith($prefix) -and $_.Version.Major -eq $oldMajor
        }

        foreach ($pkg in $conflictingPackages) {
            "`nRemoving conflicting transitive dependency: '$($pkg.Id)' version '$($pkg.Version)' from '$ProjectName' (major version $oldMajor will be superseded)"
            try {
                Invoke-Expression "Uninstall-Package `"$($pkg.Id)`" -ProjectName `"$ProjectName`" -Force"
            }
            catch {
                "`nWarning: Could not remove '$($pkg.Id)': $($_.Exception.Message). Upgrade will attempt to proceed."
            }
        }
    }
}

$basePath = $PSScriptRoot
$logFileName = $basePath + '\result.log'
$upgradeTraceLog = $basePath + '\upgrade.log'
$progressLogFile = $basePath + "\progress.log"
if (Test-Path $logFileName) {
    Remove-Item $logFileName
}
if (Test-Path $progressLogFile) {
    Remove-Item $progressLogFile
}

try {
    Start-Transcript -Path $upgradeTraceLog

    $xml = [xml](Get-Content ($basePath + '\config.xml'))

    $projectCounter = 1
    $projects = $xml.config.project
    foreach ($project in $projects) {
        $projectName = $project.name
		
        "`nUpdating project '$projectName'"
		
        $packages = $project.package
        $packageCounter = 1
        $totalCount = @($packages).Count

        Remove-Packages -ProjectName $projectName -PackagesToRemove $PackagesToRemove

        # Remove transitive dependencies that conflict with major version changes
        # in the target packages. This prevents NuGet resolver failures when
        # Update-Package processes packages sequentially.
        Remove-ConflictingTransitiveDependencies -ProjectName $projectName -Packages $packages

        foreach ($package in $packages) {
            $packageName = $package.name
            $packageVersion = $package.version
			
            "`nPackage '$packageName' version '$packageVersion'"
			
            $projectPackages = Get-Package -ProjectName $projectName
            $oldPackage = $projectPackages | Where-Object { $_.Id -eq $packageName }
            $oldPackageVersion = if (!$oldPackage.Version) { $null } else { $oldPackage.Version.ToString() }
		
            if ($oldPackageVersion -ne $null -and $oldPackageVersion -ne $packageVersion -and $oldPackageVersion -ne ($packageVersion + '.0') -and ($oldPackageVersion + '.0') -ne $packageVersion) {
                $isUpdateRequired = IsUpgradeRequired $oldPackageVersion $packageVersion
                if ($isUpdateRequired) {
                    "`nUpgrading from '$oldPackageVersion' to '$packageVersion'"
                    $errorMessage = $null;
                    Invoke-Expression "Update-Package -Id $packageName -ProjectName `"$projectName`" -Version $packageVersion -FileConflictAction OverwriteAll -IncludePrerelease -ErrorVariable errorMessage" 
					
                    if ($errorMessage -ne $null) {
                        Write-Error -Message "`nError occured while upgrading $packageName. The error was: $errorMessage" -ErrorAction Stop
                    }
                }
                else {
                    "`npackage is on higher version '$oldPackageVersion' and will not be downgraded to '$packageVersion'"
                }
            }
            else {
                "`npackage already on version '$packageVersion'"
            }
			
            $progressOut = "(" + $projectCounter + " \ " + @($projects).Count + ") --- " + $projectName + " --- " + $packageCounter.ToString() + ' / ' + $totalCount.ToString()
            $progressOut | Out-File -FilePath $progressLogFile
            $packageCounter = $packageCounter + 1
        }
		
        $projectCounter = $projectCounter + 1
    }
	
    New-Item -Path $basePath -Name "result.log" -ItemType "file" -Value "success"
}
catch {
    $text = "fail - " + $_.Exception.Message + "Check the $upgradeTraceLog for more details"
    New-Item -Path $basePath -Name "result.log" -ItemType "file" -Value $text
}
finally {
    if (Test-Path $progressLogFile) {
        Remove-Item $progressLogFile
    }

    Stop-Transcript
}
