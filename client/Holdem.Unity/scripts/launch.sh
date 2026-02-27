#!/usr/local/bin/bash

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

bash "${SCRIPT_DIR}/restore.sh"

pwsh -NoLogo -NoProfile -File "${SCRIPT_DIR}/launch.ps1"
