rem @echo off
rem SET MANAGER PATH TO THE DIRECTORY OF THIS FILE
set managerPath=LOCAL_DIR_OF_THIS_FILE
cd %managerPath%
start "%managerPath%" ProjectManager.exe
