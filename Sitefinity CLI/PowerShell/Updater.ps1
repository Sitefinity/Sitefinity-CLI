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
            
            # Save the project after each uninstall to flush DTE state
            # This prevents "reference already exists" errors during the subsequent upgrade
            $proj = Get-Project $ProjectName
            $proj.Save()
            "`nProject '$ProjectName' saved after uninstalling '$($deprecatedPackage.Id)'"
        }
    }
}

# Visual Studio sometimes fails to remove the old assembly reference while NuGet updates a packages.config package
# (https://github.com/NuGet/Home/issues/10594), so adding the new reference fails with
# "A reference to the component 'X' already exists in the project". NuGet rolls back the update,
# so the stale reference is removed through the project system and the update is retried.
function Update-PackageWithReferenceConflictRetry {
    param(
        [string] $ProjectName,
        [string] $PackageName,
        [string] $PackageVersion
    )

    $removedReferences = @()
    while ($true) {
        $errorMessage = $null
        # Only the error must be returned, so the cmdlet output is sent to the host
        Invoke-Expression "Update-Package -Id $PackageName -ProjectName `"$ProjectName`" -Version $PackageVersion -FileConflictAction OverwriteAll -IncludePrerelease -ErrorVariable errorMessage" | Out-Host
        if (!$errorMessage) {
            return $null
        }

        # Out-String wraps long lines to the console width, so the raw messages are matched instead
        $errorText = ($errorMessage | ForEach-Object { $_.ToString() }) -join ' '
        $match = [regex]::Match($errorText, "A reference to the component '([^']+)' already exists")
        if (!$match.Success) {
            $match = [regex]::Match($errorText, "Failed to add reference to '([^']+)'")
        }

        if (!$match.Success) {
            return $errorMessage
        }

        $referenceName = $match.Groups[1].Value
        if ($removedReferences -contains $referenceName) {
            Write-Host "`nReference '$referenceName' was already removed once, but the conflict persists"
            return $errorMessage
        }

        $project = Get-Project $ProjectName
        $reference = Find-ProjectReference -Project $project -ReferenceName $referenceName
        if ($null -eq $reference) {
            Write-Host "`nReference '$referenceName' was not found in '$ProjectName', so the conflict cannot be resolved"
            return $errorMessage
        }

        Write-Host "`nRemoving stale reference '$referenceName' from '$ProjectName' and retrying the upgrade of '$PackageName'"
        $reference.Remove()
        $project.Save()
        $removedReferences += $referenceName
    }
}

function Find-ProjectReference {
    param(
        $Project,
        [string] $ReferenceName
    )

    $references = $Project.Object.References
    try {
        $reference = $references.Item($ReferenceName)
        if ($null -ne $reference) {
            return $reference
        }
    }
    catch {
        # Item throws when no reference has this exact name, so fall back to a case-insensitive search
    }

    foreach ($reference in $references) {
        if ($reference.Name -ieq $ReferenceName -or $reference.Identity -ieq $ReferenceName) {
            return $reference
        }
    }

    return $null
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

        foreach ($package in $packages) {
            $packageName = $package.name
            $packageVersion = $package.version
			
            "`nPackage '$packageName' version '$packageVersion'"

            if ($package.prerequisite -eq "true") {
                "`nUpgrading prerequisite package '$packageName', because its dependency constraints are incompatible with the upgrade"
            }
			
            $projectPackages = Get-Package -ProjectName $projectName
            $oldPackage = $projectPackages | Where-Object { $_.Id -eq $packageName }
            $oldPackageVersion = if (!$oldPackage.Version) { $null } else { $oldPackage.Version.ToString() }
		
            if ($oldPackageVersion -ne $null -and $oldPackageVersion -ne $packageVersion -and $oldPackageVersion -ne ($packageVersion + '.0') -and ($oldPackageVersion + '.0') -ne $packageVersion) {
                $isUpdateRequired = IsUpgradeRequired $oldPackageVersion $packageVersion
                if ($isUpdateRequired) {
                    "`nUpgrading from '$oldPackageVersion' to '$packageVersion'"
                    $errorMessage = Update-PackageWithReferenceConflictRetry -ProjectName $projectName -PackageName $packageName -PackageVersion $packageVersion
					
                    if ($errorMessage) {
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
