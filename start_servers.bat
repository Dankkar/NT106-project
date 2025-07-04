@echo off
echo ===============================================
echo       Starting File Sharing Server System
echo ===============================================
echo.

echo [1/3] Starting Backend Server 1 on port 5100...
start "Backend Server 1 (Port 5100)" cmd /k "cd /d "%~dp0" && .\FileSharingServer\bin\Debug\FileSharingServer.exe 5100"

echo [2/3] Starting Backend Server 2 on port 5101...
start "Backend Server 2 (Port 5101)" cmd /k "cd /d "%~dp0" && .\FileSharingServer\bin\Debug\FileSharingServer.exe 5101"

echo [3/3] Starting Load Balancer on port 5000...
start "Load Balancer (Port 5000)" cmd /k "cd /d "%~dp0" && .\LoadBalancerServer\bin\Debug\net48\LoadBalancerServer.exe"

echo.
echo ===============================================
echo All servers started successfully!
echo.
echo - Backend Server 1: localhost:5100
echo - Backend Server 2: localhost:5101  
echo - Load Balancer:    localhost:5000
echo.
echo Client should connect to: 127.0.0.1:5000
echo ===============================================
echo.
echo Press any key to exit this window...
pause >nul 