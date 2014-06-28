$jenkins_slave_home='D:\java\Jenkins\slave\bin'
pushd $jenkins_slave_home 
set-location -Path 'basicService'
invoke-expression -command "C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe WindowsService.sln"
set-location -path 'WindowsService\bin\Debug'
copy-item -Path "*.*" -Destination $jenkins_slave_home -force -Include '*' -recurse
popd

$ServiceName = 'dummy_service_3'
$service = Get-WmiObject -Class Win32_Service -Filter "Name='${ServiceName}'"

if ($service -ne $null){
	invoke-expression -command 'c:\windows\system32\sc.exe query "${ServiceName}"'
	invoke-expression -command 'c:\windows\system32\sc.exe stop "${ServiceName}"'
	invoke-expression -command 'c:\windows\system32\sc.exe delete "${ServiceName}"'
	# $service.delete()
	start-sleep -seconds 15
	$service = Get-WmiObject -Class Win32_Service -Filter "Name='${ServiceName}'"
	if ($service -eq $null ){
		write-output "Recycled service ""${ServiceName}""."
	}
}


write-output "Installing service ""${ServiceName}"""
invoke-expression -command 'c:\Windows\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe -install "D:\java\Jenkins\slave\bin\WindowsService.exe"'

<# 
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
	# Without credentials service is set to run under LocalSystem
	# on Windows Server 2012 the account should be LocalServiced 
        New-Service -name "dummy_service_3" -Description 'This service runs Jenkins Slave Agent' -binaryPathName "D:\java\jenkins\slave\bin\WindowsService.exe" -displayName "dummy_service_3" -startupType Automatic
#>
