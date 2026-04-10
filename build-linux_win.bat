@echo off

echo Building...

docker buildx build ^
    --output type=local,dest=dist/linux ^
    --target linux-export ^
    .

if %ERRORLEVEL% neq 0 (
    echo Build failed
    pause
    exit /b 1
)

echo Build complete -> dist/linux
pause