$ServiceName = 'Dummy_Service_1'
$service = Get-WmiObject -Class Win32_Service -Filter "Name='${ServiceName}'"

if ($service -ne $null){
	invoke-expression -command 'c:\windows\system32\sc.exe query "${ServiceName}"'
	invoke-expression -command 'c:\windows\system32\sc.exe stop "${ServiceName}"'
	$service.delete()
	start-sleep -seconds 15
	$service = Get-WmiObject -Class Win32_Service -Filter "Name='${ServiceName}'"
	if ($service -eq $null ){
		write-output "Recycled service ""${ServiceName}""."
	}
}

write-output "Installing service ""${ServiceName}"""
if ($false) {
# creating credentials which can be used to run my windows service

	$computerName = $env:computerName
	$domain = $env:USERDOMAIN
	if ($domain -like 'UAT') {
		$user = '_uatmsdeploy'
	}
	if ($domain -like 'TESE') {
		$user = '_testmsdeploy'
	}
	$newAcct = "${domain}\${user}"
	$credential = Get-Credential -username $newAcct -message  ('Enter password for {0}, please' -f $newAcct   )
	$newAcct  = $credential.Username

	# $secpasswd = ConvertTo-SecureString "MyPa$$word" -AsPlainText -Force
	# $credential = New-Object System.Management.Automation.PSCredential ($newAcct, $secpasswd)

}

$binaryPath = "c:\Jenkins\slave\WindowsService.exe"

# Without credentials service is set to run under LocalSystem
New-Service -name "${ServiceName}" -Description 'Dummy c# Windows Service to set place for JNLP' -binaryPathName "${binaryPath}" -displayName "${ServiceName}" -startupType Automatic
write-output "installed service ${ServiceName} with application binary path ${binaryPath}"
invoke-expression -command 'c:\windows\system32\sc.exe start "${ServiceName}"'
write-output "Started service ${ServiceName}"
$realbinaryPath = "c:\Jenkins\slave\jenkins.exe"
write-output "Stopped service ${ServiceName}"
invoke-expression -command 'c:\windows\system32\sc.exe stop "${ServiceName}"'

@('jenkins.err.log', 'jenkins.out.log', 'jenkins.wrapper.log') | foreach-object {set-content -Path $_ -value ''}

pushd HKLM:
cd /System/CurrentControlSet/Services
cd "${ServiceName}"
get-itemproperty -name 'ImagePath' -path "HKLM:\System\CurrentControlSet\Services\${ServiceName}"
set-itemproperty -name 'ImagePath' -path "HKLM:\System\CurrentControlSet\Services\${ServiceName}" -value $realbinaryPath
popd
write-output "Change service ${ServiceName} binary Path to ""${realbinaryPath}"""

write-output "Started service ${ServiceName}"
invoke-expression -command 'c:\windows\system32\sc.exe start "${ServiceName}"'
start-sleep 15
get-content -path 'jenkins.wrapper.log' | out-string
