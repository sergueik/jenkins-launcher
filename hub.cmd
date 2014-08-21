@echo OFF
set HTTP_PORT=4444
set HTTPS_PORT=-1
set APP_VERSION=2.42.2
set JAVA_HOME=D:\java\jdk1.6.0_45
set GROOVY_HOME=D:\java\groovy-2.3.2
PATH=%JAVA_HOME%\bin;%PATH%;%GROOVY_HOME%\bin
set LAUNCHER_OPTS=-XX:MaxPermSize=1028M -Xmn128M

java %LAUNCHER_OPTS% -jar selenium-server-standalone-%APP_VERSION%.jar -port %HTTP_PORT% -role hub
goto :EOF    
