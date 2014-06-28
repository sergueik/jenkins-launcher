$ServiceName = 'dummy_service_3'
$service = Get-WmiObject -Class Win32_Service -Filter "Name='${ServiceName}'"

if ($service -ne $null){
$binaryPath = "d:\java\Jenkins\slave\bin\WindowsService.exe"
$realbinaryPath = "d:\java\Jenkins\slave\bin\jenkins.exe"
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
<#
Possible error:
Service cannot be started. System.UnauthorizedAccessException: Access to the path 'd:\java\Jenkins\slave\bin\jenkins.wrapper.log' is denied.


#>

invoke-expression -command 'c:\windows\system32\sc.exe config "dummy_service_3" obj= LocalSystem'
write-output "Started service ${ServiceName}"
invoke-expression -command 'c:\windows\system32\sc.exe start "${ServiceName}"'
start-sleep 15
get-content -path 'jenkins.wrapper.log' | out-string

}