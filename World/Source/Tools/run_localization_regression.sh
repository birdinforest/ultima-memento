#!/usr/bin/env bash
set -euo pipefail
# Run from repo root (directory that contains World/). Requires WorldLinux.exe built.
ROOT="$(cd "$(dirname "$0")/../../.." && pwd)"
cd "$ROOT"
if [[ ! -f World/WorldLinux.exe ]]; then
	echo "run_localization_regression.sh: expected World/WorldLinux.exe under $ROOT" >&2
	exit 1
fi
exec mono World/WorldLinux.exe -localization-regression "$@"
