param(
	$PackageToInstall,
	$VersionToInstall,
	$TargetProjectFiles
)

function Install-NugetPackage($packageName, $version, $projectName){
	$installExpression = "Install-Package `"$($packageName)`""
	if ($null -ne $version)
	{
		$installExpression += " -Version `"$version`""
	}
	if($null -ne $projectName)
	{
		$installExpression += " -ProjectName `"$projectName`""
	}

	"`nRunning command '$installExpression"
}

$basePath = $PSScriptRoot
$logFileNamePath = $basePath + '\install-result.log'
$installTraceLogPath = $basePath + '\install.log'

if (Test-Path $logFileNamePath) 
{
	Remove-Item $logFileNamePath
}

try
{
	Start-Transcript -Path $installTraceLogPath

	if ($null -ne $TargetProjectFiles) 
	{
		$projectToUpgrade = $TargetProjectFiles.Split(";");
		foreach ($project in $projectToUpgrade){
			Install-NugetPackage -packageName $PackageToInstall -version $VersionToInstall -projectName $project
		}
	}
	else
	{
		Install-NugetPackage -packageName $PackageToInstall -version $VersionToInstall
	}
}
catch
{
	$text = "fail - " + $_.Exception.Message + "Check the $installTraceLogPath for more details"
	New-Item -Path $basePath -Name "install-result.log" -ItemType "file" -Value $text
}
finally
{
	Stop-Transcript
}