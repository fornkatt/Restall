#!/usr/bin/env bash
set -euo pipefail

echo "Building..."

docker buildx build \
    --output type=local,dest=dist/windows \
    --target windows-export \
    .

echo "Build complete -> dist/windows"