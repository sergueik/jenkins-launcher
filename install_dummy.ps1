#Copyright (c) 2016,2021 Serguei Kouzmine
#
#Permission is hereby granted, free of charge, to any person obtaining a copy
#of this software and associated documentation files (the "Software"), to deal
#in the Software without restriction, including without limitation the rights
#to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
#copies of the Software, and to permit persons to whom the Software is
#furnished to do so, subject to the following conditions:
#
#The above copyright notice and this permission notice shall be included in
#all copies or substantial portions of the Software.
#
#THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
#IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
#FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
#AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
#LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
#OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
#THE SOFTWARE.

$drive = ${env:temp}

$selenium_node_home ="${drive}\selenium-local"
$dummy_service_home = "${drive}\selenium-local\BasicService"
$dummy_binary = 'WindowsService.exe'
$binary_path = "${dummy_service_home}\WindowsService\bin\Debug\${dummy_binary}"

$ServiceName = 'selenium-node'
pushd $selenium_node_home -erroraction  SilentlyContinue
set-location -Path $dummy_service_home
<# 

WindowsService.sln(2): Solution file error MSB5014: File format version is not recognized.  MSBuild can only read solution files between versions 7.0 and 9.0, inclusive.
#>


$msbuild = "C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"
# $msbuild = "C:\Windows\Microsoft.NET\Framework\v2.0.50727\MSBuild.exe"
invoke-expression -command "${msbuild} WindowsService.sln"
set-location -path 'WindowsService\bin\Debug'
copy-item -Path "*.*" -Destination $selenium_node_homr -force -Include '*' -recurse
popd


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
invoke-expression -command "c:\Windows\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe -install ""${binary_path}"""

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
        New-Service -name "dummy_service_3" -Description 'This service runs selenium-node Slave Agent' -binaryPathName "D:\java\selenium-node\master\WindowsService.exe" -displayName "dummy_service_3" -startupType Automatic
#>
