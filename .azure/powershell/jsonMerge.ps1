function JoinObjects($source, $extend){
    if($source.GetType().Name -eq "PSCustomObject" -and $extend.GetType().Name -eq "PSCustomObject"){
        foreach($Property in $source | Get-Member -type NoteProperty, Property){
            if($extend.$($Property.Name) -eq $null){
              continue;
            }
            $source.$($Property.Name) = JoinObjects $source.$($Property.Name) $extend.$($Property.Name)
        }
    }else{
       $source = $extend;
    }
    return $source
}
function AddPropertyRecurse($source, $toExtend){
    if($source.GetType().Name -eq "PSCustomObject"){
        foreach($Property in $source | Get-Member -type NoteProperty, Property){
            if($toExtend.$($Property.Name) -eq $null){
              $toExtend | Add-Member -MemberType NoteProperty -Value $source.$($Property.Name) -Name $Property.Name `
            }
            else{
               $toExtend.$($Property.Name) = AddPropertyRecurse $source.$($Property.Name) $toExtend.$($Property.Name)
            }
        }
    }
    return $toExtend
}
function JsonMerge($source, $extend){
    $merged = JoinObjects $source $extend
    $extended = AddPropertyRecurse $merged $extend
    return $extended
}
function JsonMergeFromPath($sourcePath, $extendPath){
    $params = Get-Content $sourcePath -Raw | ConvertFrom-Json
    if (Test-Path $extendPath) {
        $params = JsonMerge $params (Get-Content $extendPath -Raw | ConvertFrom-Json)
    }
	return $params
}
function AddMemberPath($source, $path, $value){
	$path = $path -split "\."
	$current = $source
	for($i = 0; $i -lt $path.Length; $i++){
		if($i -eq $path.Length - 1){
			$current | Add-Member -MemberType NoteProperty -Value $value -Name $path[$i] -Force
            return
		}
		if($current.$($path[$i]) -eq $null){
			$current | Add-Member -MemberType NoteProperty -Value ("{}" | ConvertFrom-Json) -Name $path[$i]
		}
		$current = $current.$($path[$i])
	}
}