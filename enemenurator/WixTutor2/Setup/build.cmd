REM 

msbuild.exe enemenurator.csproj 
msbuild.exe /property:WixSdkPath=c:\wix\sdk\ /property:WixTasksPath=c:\wix\WixTasks.dll /property:WixCATargetsPath=c:\wix\SDK\wix.ca.targets /property:WixTargetsPath=c:\wix\wix.targets /property:WixTasksPath=c:\wix\WixTasks.dll /property:WixToolPath=c:\wix\ enemenurator.wproj  /t:clean,rebuild

