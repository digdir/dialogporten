function Get-RandomCharacters([int]$length, [string]$characters) {
    $random = 1..$length | ForEach-Object { Get-Random -Maximum $characters.length }
    $private:ofs=""
    return [string]$characters[$random]
}
function Scramble-String([string]$inputString){     
    $characterArray = $inputString.ToCharArray()   
    $scrambledStringArray = $characterArray | Get-Random -Count $characterArray.Length     
    $outputString = -join $scrambledStringArray
    return $outputString 
}
function GeneratePassword{
    param(
		[Parameter()]
        [ValidateRange(8,64)]
		[int]$length=25,
		[Parameter()]
        [ValidateRange(0,64)]
		[int]$minLower=1,
		[Parameter()]
        [ValidateRange(0,64)]
		[int]$minUpper=1,
		[Parameter()]
        [ValidateRange(0,64)]
		[int]$minNumber=1,
		[Parameter()]
        [ValidateRange(0,64)]
		[int]$minSpecial=1
	)
	$lowercase = 'abcdefghiklmnoprstuvwxyz'
	$uppercase = 'ABCDEFGHKLMNOPRSTUVWXYZ'
	$numbers = '1234567890'
	$special = '@#*+&%$!~'
	$characters = $lowercase + $uppercase + $numbers + $special
	$password = Get-RandomCharacters $minLower $lowercase
	$password += Get-RandomCharacters $minUpper $uppercase
	$password += Get-RandomCharacters $minNumber $numbers
	$password += Get-RandomCharacters $minSpecial $special
	$password += Get-RandomCharacters $($length-$password.Length) $characters
	$password = Scramble-String $password
	$Bytes = [System.Text.Encoding]::Unicode.GetBytes($password)
	$EncodedText =[Convert]::ToBase64String($Bytes)
	return @{
	    Password = $password
	    EncodedPassword = $EncodedText
	}
}
