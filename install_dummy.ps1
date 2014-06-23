# NOTE: Unblock-File .\update_dummy_service.ps1
# http://msdn.microsoft.com/en-us/library/windows/desktop/ms684188%28v=vs.85%29.aspx
$jenkins_slave_home ='c:\java\Jenkins\slave\bin'
pushd $jenkins_slave_home 
set-location -Path 'basicService'
invoke-expression -command "C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe WindowsService.sln"
set-location -path 'WindowsService\bin\Debug'
copy-item -Path "*.*" -Destination $jenkins_slave_home -force -Include '*' -recurse
popd

$ServiceName = 'dummy_service_4'
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
invoke-expression -command 'c:\Windows\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe -install "${jenkins_slave_home}\WindowsService.exe"'

