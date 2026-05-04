::Tips Set the CSIGNCERT as your path.
@echo off
path D:\ProjectsTmp\SignPack;%path%
echo 任意键签名 系统信息硬件记录读取工具...
pause > nul
cmd.exe /c signcmd.cmd "%CSIGNCERT%" "%~dp0HardWareInfo\bin\Release\hwi.exe"
echo.
echo 完成！
echo 任意键退出...
pause > nul