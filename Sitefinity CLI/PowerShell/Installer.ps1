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
		Write-Error -Message "`nError occured while installing $packageName. The error was: $errorMessage" -ErrorAction Stop
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
