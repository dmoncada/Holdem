#!/usr/local/bin/bash

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

build_dotnet_project() {
    local proj_dir="$1"
    local output_dir="$2"
    local framework="${3:-netstandard2.1}"
    local configuration="${4:-Release}"

    rm -fr "$output_dir"

    dotnet build "$proj_dir" \
        --configuration "$configuration" \
        --framework "$framework" \
        --output "$output_dir"
}

CORE_PROJ="${SCRIPT_DIR}/../../../server/src/Holdem.Core"
CORE_OUTPUT="${SCRIPT_DIR}/../Assets/Plugins/Holdem.Core"

PROTO_PROJ="${SCRIPT_DIR}/../../../server/src/Holdem.Proto"
PROTO_OUTPUT="${SCRIPT_DIR}/../Assets/Plugins/Holdem.Proto"

# Restore projects.
build_dotnet_project "$CORE_PROJ" "$CORE_OUTPUT"
build_dotnet_project "$PROTO_PROJ" "$PROTO_OUTPUT"

# Restore packages.
cd "${SCRIPT_DIR}/.."
dotnet nugetforunity restore
cd -
