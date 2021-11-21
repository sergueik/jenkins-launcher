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



# this script tweaks  the dummy service  installed on Windows  platform and  swaps the name of the executable
# to compensate for the issues winsw has with Windows 2012 .


$ServiceName = 'selenium-node'
$drive = 'C:'

$selenium_node_home ="${drive}\selenium-local"
$dummy_service_home = "${drive}\selenium-local\BasicService"

$dummy_binary = 'WindowsService.exe'

$binary_path = "${dummy_service_home}\WindowsService\bin\Debug\${dummy_binary}"

$real_binary  = "selenium-node.exe"
$realbinary_path = "${selenium_node_home}\${real_binary}"

$service = Get-WmiObject -Class Win32_Service -Filter "Name='${ServiceName}'"


if ($service -ne $null){



write-output "Stopped service ${ServiceName}"
invoke-expression -command 'c:\windows\system32\sc.exe stop "${ServiceName}"'

write-output "Truncating selenium-node logs"
<#
Prevent error:
Service cannot be started. System.UnauthorizedAccessException: Access to the path 'd:\java\selenium-node\slave\bin\selenium-node.wrapper.log' is denied.
#>

@('selenium-node.err.log', 'selenium-node.out.log', 'selenium-node.wrapper.log') | foreach-object {set-content -Path $_ -value ''}
# TODO: ACL

write-output "Changing service parameters in the registry"
pushd HKLM:
cd /System/CurrentControlSet/Services
cd "${ServiceName}"
get-itemproperty -name 'ImagePath' -path "HKLM:\System\CurrentControlSet\Services\${ServiceName}"
set-itemproperty -name 'ImagePath' -path "HKLM:\System\CurrentControlSet\Services\${ServiceName}" -value $realbinary_path
write-output "Changed service ${ServiceName} binary Path to ""${realbinary_path}"""
popd

write-output 'Change service identity to LocalSystem'
invoke-expression -command 'c:\windows\system32\sc.exe config ${ServiceName} obj= LocalSystem'

pushd $selenium_node_home

invoke-expression -command "c:\windows\system32\sc.exe start ""${ServiceName}"""
write-output "Started service ${ServiceName}"
start-sleep 15
write-output "Waiting for java logs ${ServiceName}"
get-content -path 'selenium-node.wrapper.log' | out-string
popd 

}
