@echo off

echo Building...

docker buildx build ^
    --output type=local,dest=dist/windows ^
    --target windows-export ^
    .

if %ERRORLEVEL% neq 0 (
    echo Build failed
    pause
    exit /b 1
)

echo Build complete -> dist/windows
pause