### Info

A customized replica of [winsw](https://github.com/winsw/winsw) classic Windows Service facade that wraps and manages any application as a Windows service that only required to be deployed on generic Windows platforms with a .NET Framework 4.5 LTS or later versions installed. 

### Note
For systems without .NET Framework, the original [winsw](https://github.com/winsw/winsw) project provides native 64-bit and 32-bit executables based on __.NET__ __7__

Patching the undocumented dependencies on weird quirks of the system will lead to chasing down every last one of them

### Usage

* build project
* install using elevated prompt

```cmd
c:\developer\sergueik\jenkins-launcher> c:\Windows\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe -install WindowsService\bin\Debug\WindowsService.exe
```
will see the success messages
```txt
Microsoft (R) .NET Framework Installation utility Version 4.0.30319.33440
Copyright (C) Microsoft Corporation.  All rights reserved.


Running a transacted installation.

Beginning the Install phase of the installation.
See the contents of the log file for the c:\developer\sergueik\jenkins-launcher\
WindowsService\bin\Debug\WindowsService.exe assembly's progress.
The file is located at c:\developer\sergueik\jenkins-launcher\WindowsService\bin\Debug\WindowsService.InstallLog.
Installing assembly 'c:\developer\sergueik\jenkins-launcher\WindowsService\bin\Debug\WindowsService.exe'.
Affected parameters are:
   logtoconsole =
   logfile = c:\developer\sergueik\jenkins-launcher\WindowsService\bin\Debug\WindowsService.InstallLog
   assemblypath = c:\developer\sergueik\jenkins-launcher\WindowsService\bin\Debug\WindowsService.exe
   install =
Installing service winws...
Service winws has been successfully installed.
Creating EventLog source winws in log Application...

The Install phase completed successfully, and the Commit phase is beginning.
See the contents of the log file for the c:\developer\sergueik\jenkins-launcher\WindowsService\bin\Debug\WindowsService.exe assembly's progress.
The file is located at c:\developer\sergueik\jenkins-launcher\WindowsService\bin\Debug\WindowsService.InstallLog.
Committing assembly 'c:\developer\sergueik\jenkins-launcher\WindowsService\bin\Debug\WindowsService.exe'.
Affected parameters are:
   logtoconsole =
   logfile = c:\developer\sergueik\jenkins-launcher\WindowsService\bin\Debug\WindowsService.InstallLog
   assemblypath = c:\developer\sergueik\jenkins-launcher\WindowsService\bin\Debug\WindowsService.exe
   install =

The Commit phase completed successfully.

The transacted install has completed.

```
* find the service via `services.msc` application or in the console

```cmd
sc.exe query winws

SERVICE_NAME: winws
        TYPE               : 10  WIN32_OWN_PROCESS
        STATE              : 1  STOPPED
        WIN32_EXIT_CODE    : 1077  (0x435)
        SERVICE_EXIT_CODE  : 0  (0x0)
        CHECKPOINT         : 0x0
        WAIT_HINT          : 0x0
```
### See Also
  * __Getting Started Building Windows Services with Topshelf__  Pluralsight [course](https://app.pluralsight.com/library/courses/topshelf-getting-started-building-windows-services/table-of-contents?aid=7010a000001xAKZAA2)
  * [Topshelf](https://github.com/Topshelf/Topshelf) - "service hosting framework" nuget package simplifying testing Windows services written in .NET
  * [example project](https://github.com/lampo1024/TopshelfDemoService) claiming to reach interactive user desktops from a service,  uses topshelf nuget 
  * memo [about desktop 0 restrictions](https://codedefault.com/p/launch-a-gui-application-from-a-windows-service-on-windows) (in Chinese)
  * `DotNetCore.WindowsService` [project](https://github.com/PeterKottas/DotNetCore.WindowsService) and [nuget](https://www.nuget.org/pack$ages/DotNetCore.WindowsServices) and [source](https://github.com/icenorge/DotNetCore.WindowsServices)
  * https://www.nuget.org/packages/Microsoft.Extensions.Hosting.WindowsServices 
---

### Author
[Serguei Kouzmine](kouzmine_serguei@yahoo.com)

