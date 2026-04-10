#!/usr/bin/env bash
set -euo pipefail

echo "Building..."

docker buildx build \
    --output type=local,dest=dist/linux \
    --target linux-export \
    .

echo "Build complete -> dist/linux"