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
			
            $projectPackages = Get-Package -ProjectName $projectName
            $oldPackage = $projectPackages | Where-Object { $_.Id -eq $packageName }
            $oldPackageVersion = if (!$oldPackage.Version) { $null } else { $oldPackage.Version.ToString() }
		
            if ($oldPackageVersion -ne $null -and $oldPackageVersion -ne $packageVersion -and $oldPackageVersion -ne ($packageVersion + '.0') -and ($oldPackageVersion + '.0') -ne $packageVersion) {
                $isUpdateRequired = IsUpgradeRequired $oldPackageVersion $packageVersion
                if ($isUpdateRequired) {
                    "`nUpgrading from '$oldPackageVersion' to '$packageVersion'"
                    $errorMessage = $null;
                    Invoke-Expression "Update-Package -Id $packageName -ProjectName `"$projectName`" -Version $packageVersion -FileConflictAction OverwriteAll -ErrorVariable errorMessage" 
					
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
