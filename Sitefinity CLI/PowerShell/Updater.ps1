param(
    [bool] $RemoveDeprecatedPackages
)

function IsUpgradeRequired($oldPackageVersion, $packageVersion) {
    #handles 13.1.7340-preview or 13.1.7340-beta versions
    $oldPackageVersion = $oldPackageVersion.split('-')[0];
    $packageVersion = $packageVersion.split('-')[0];

    return [System.Version]$packageVersion -gt [System.Version]$oldPackageVersion
}

function Remove-DeprecatedPackages($projectName, $sfPackageVersion) {
    $deprecatedPackages = @(
        @{
            Name                = "Telerik.DataAccess.Fluent"
            DeprecatedInVersion = [System.Version]"12.2.7200"
        },
        @{
            Name                = "Telerik.Sitefinity.OpenAccess"
            DeprecatedInVersion = [System.Version]"13.0.7300" 
        },
        @{
            Name                = "Telerik.Sitefinity.AmazonCloudSearch"
            DeprecatedInVersion = [System.Version]"13.3.7600" 
        },
        @{
            Name                = "PayPal"
            DeprecatedInVersion = [System.Version]"14.0.7700" 
        },
	@{
            Name                = "CsvHelper"
            DeprecatedInVersion = [System.Version]"14.0.7700" 
        },
	@{
            Name                = "payflow_dotNET"
            DeprecatedInVersion = [System.Version]"14.0.7700" 
        },
        @{
            Name                = "Progress.Sitefinity.Dec.Iris.Extension"
            DeprecatedInVersion = [System.Version]"14.0.7700" 
        },
	@{
            Name                = "Progress.Sitefinity.IdentityServer3"
            DeprecatedInVersion = [System.Version]"14.4.8100" 
        },
	@{
            Name                = "Progress.Sitefinity.IdentityServer3.AccessTokenValidation"
            DeprecatedInVersion = [System.Version]"14.4.8100" 
        },
	@{
            Name                = "Autofac"
            DeprecatedInVersion = [System.Version]"14.4.8100" 
        },
	@{
            Name                = "Autofac.WebApi2"
            DeprecatedInVersion = [System.Version]"14.4.8100" 
        },
	@{
            Name                = "Microsoft.AspNet.WebApi.Owin"
            DeprecatedInVersion = [System.Version]"14.4.8100" 
        },
	@{
            Name                = "Microsoft.AspNet.WebApi.Tracing"
            DeprecatedInVersion = [System.Version]"14.4.8100" 
        },
        @{
            Name                = "Telerik.Sitefinity.Analytics"
            DeprecatedInVersion = [System.Version]"15.0.8200" 
        },
	@{
            Name                = "Progress.Sitefinity.Ecommerce"
            DeprecatedInVersion = [System.Version]"15.0.8200" 
        },
	@{
            Name                = "Telerik.Sitefinity.AmazonCloudSearch"
            DeprecatedInVersion = [System.Version]"15.0.8200" 
        },
    )

    "`nRemoving deprecated packages for '$projectName'"
    foreach ($package in $deprecatedPackages) {
        if ($sfPackageVersion -ge $package.DeprecatedInVersion) {
            $deprecatedPackage = Invoke-Expression "Get-Package `"$($package.Name)`" -ProjectName `"$projectName`"" 
            if ($null -ne $deprecatedPackage) {
                "`nUninstalling package: '$($deprecatedPackage.Id)' from `"$projectName`""
                Invoke-Expression "Uninstall-Package `"$($deprecatedPackage.Id)`" -ProjectName `"$projectName`" -Force" 
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

        if ($RemoveDeprecatedPackages) {
            $sfPackageVersion = ($packages | Where-Object { $_.name -eq "Telerik.Sitefinity.Core" -or $_.name -eq "Telerik.Sitefinity.All" }).Version

            if ($null -ne $sfPackageVersion) {
                Remove-DeprecatedPackages -projectName $projectName -sfPackageVersion $sfPackageVersion
            }
        }

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
