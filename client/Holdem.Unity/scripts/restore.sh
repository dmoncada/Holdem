#!/usr/local/bin/bash

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROTO_PROJ="${SCRIPT_DIR}/../../../server/src/Holdem.Proto"
OUTPUT_DIR="${SCRIPT_DIR}/../Assets/Plugins/Holdem.Proto"

rm -fr "$OUTPUT_DIR"

# Restore proto.
dotnet build "$PROTO_PROJ" \
    --configuration Release \
    --framework netstandard2.1 \
    --output "$OUTPUT_DIR"

# Restore packages.
dotnet nugetforunity restore
