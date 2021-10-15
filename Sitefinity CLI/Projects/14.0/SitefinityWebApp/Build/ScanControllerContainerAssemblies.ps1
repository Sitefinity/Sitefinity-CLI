param(
	[Parameter(Mandatory=$true)]
	[string]$binariesDirectory
)

$assemblies = Get-ChildItem $binariesDirectory -Filter *.dll
$controllerAssemblies = @()

foreach ($assembly in $assemblies) {
    $loadedAssembly = [System.Reflection.Assembly]::LoadFrom($assembly.FullName)
    $assembliesWithBindingErrors = @();
    
    if(!$loadedAssembly.CustomAttributes){
      #See if the issue is related to version mismatch
      try{
        $result = [reflection.customattributedata]::GetCustomAttributes($loadedAssembly)
      }
      catch{
        $ErrorMessage = $_.Exception.Message
        
        if($ErrorMessage -like "*Could not load file or assembly*"){
            $assembliesWithBindingErrors += $assembly.Name;
		}
	  }
	}

    if ($loadedAssembly.CustomAttributes | ? { $_.AttributeType.FullName -eq "Telerik.Sitefinity.Frontend.Mvc.Infrastructure.Controllers.Attributes.ControllerContainerAttribute" -or $_.AttributeType.FullName -eq "Telerik.Sitefinity.Frontend.Mvc.Infrastructure.Controllers.Attributes.ResourcePackageAttribute"}) {
        $controllerAssemblies += $assembly.Name
    }

    #Loop through binding error items, add to json
    foreach($needsBinding in $assembliesWithBindingErrors){
        $controllerAssemblies += $needsBinding
	}
}

$controllerAssemblies | ConvertTo-Json -depth 100 | Set-Content "$binariesDirectory\ControllerContainerAsembliesLocation.json" -Force