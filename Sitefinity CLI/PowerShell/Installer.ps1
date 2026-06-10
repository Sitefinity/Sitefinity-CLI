param(
	$PackageToInstall,
	$VersionToInstall,
	[string[]]$TargetProjectFiles
)

function Install-NugetPackage($packageName, $version, $projectName){
	$errorMessage = $null;
	$installExpression = "Install-Package -Id `"$($packageName)`" -ErrorVariable errorMessage"

	if ($null -ne $version)
	{
		$installExpression += " -Version `"$version`""
	}
	if ($null -ne $projectName)
	{
		$installExpression += " -ProjectName `"$projectName`""
	}

	"`nRunning command '$installExpression"
	Invoke-Expression $installExpression 
	
	if ($errorMessage)
	{
		# NuGet's Install-Package (running inside the Visual Studio Package Manager Console)
		# raises a non-terminating error when a package tries to add an assembly reference
		# that is already present in the target project, e.g.
		#   "Failed to add reference to 'Telerik.Sitefinity.AI'. A reference to the component
		#    'Telerik.Sitefinity.AI' already exists in the project."
		# This happens for packages such as Progress.Sitefinity.DynamicExperience, which
		# depend on Telerik.Sitefinity.AI while the CMS project already references it. In
		# that case NuGet still installs the package correctly (packages.config is updated
		# and content/lib files are copied); only the duplicate EnvDTE References.Add call
		# is rejected. Treat this specific error as a warning instead of failing the whole
		# install, while still failing on any other error NuGet reports.
		$duplicateReferencePattern = "A reference to the component '.*' already exists in the project"
		$fatalErrors = @($errorMessage | Where-Object { $_.ToString() -notmatch $duplicateReferencePattern })

		if ($fatalErrors.Count -gt 0)
		{
			Write-Error -Message "`nError occured while installing $packageName. The error was: $fatalErrors" -ErrorAction Stop
		}
		else
		{
			Write-Warning "Ignoring non-fatal duplicate-reference warning(s) while installing $packageName. Details: $errorMessage"
		}
	}
}

$resultLogFileName = "result.log"
$installLogFileName = "install.log"
$logFileNamePath = Join-Path $PSScriptRoot -ChildPath $resultLogFileName
$installTraceLogPath = Join-Path $PSScriptRoot -ChildPath $installLogFileName

if (Test-Path $logFileNamePath) 
{
	Remove-Item $logFileNamePath
}

try
{
	Start-Transcript -Path $installTraceLogPath

	if ($TargetProjectFiles -ne $null) 
	{
		foreach ($project in $TargetProjectFiles)
		{
			Install-NugetPackage -packageName $PackageToInstall -version $VersionToInstall -projectName $project
		}
	}
	else
	{
		Install-NugetPackage -packageName $PackageToInstall -version $VersionToInstall
	}

	New-Item -Path $PSScriptRoot -Name $resultLogFileName -ItemType "file" -Value "success"
}
catch
{
	$text = "fail - " + $_.Exception.Message + "Check the $installTraceLogPath for more details"
	New-Item -Path $PSScriptRoot -Name $resultLogFileName -ItemType "file" -Value $text
}
finally
{
	Stop-Transcript
}
