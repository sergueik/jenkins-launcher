@echo OFF
set HTTP_PORT=4444
set HTTPS_PORT=-1
set APP_VERSION=2.42.2
set JAVA_HOME=d:\java\jdk1.6.0_45
set GROOVY_HOME=d:\java\groovy-2.3.2
set HUB_URL=http://127.0.0.1:4444/grid/register
set NODE_CONFIG=%CD%\node-configuration.json
PATH=%JAVA_HOME%\bin;%PATH%;%GROOVY_HOME%\bin
REM Install Firefox using standalone installer,
REM customize the installation path
REM Put the information here
REM another alternative is the selenium command option
PATH=%PATH%;C:\Program Files\Mozilla Firefox\
PATH=%PATH%;C:\Program Files (x86)\Mozilla Firefox\
PATH=%PATH%;D:\Program Files\Mozilla Firefox\
PATH=%PATH%;C:\Program Files (x86)\Mozilla Firefox\
where.exe firefox.exe
REM
CHOICE /T 1 /C ync /CS /D y
set LAUNCHER_OPTS=-XX:MaxPermSize=1028M -Xmn128M

java %LAUNCHER_OPTS% -jar selenium-server-standalone-%APP_VERSION%.jar -hub %HUB_URL% -role node -nodeConfig:%NODE_CONFIG%
    