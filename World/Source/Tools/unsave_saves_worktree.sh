#!/usr/bin/env bash
# Clear skip-worktree for all tracked files under World/Saves/ (e.g. to refresh baseline commit).

set -euo pipefail
cd "$(git rev-parse --show-toplevel)"
n=0
while IFS= read -r -d '' f; do
	git update-index --no-skip-worktree "$f"
	n=$((n + 1))
done < <(git ls-files -z -- "World/Saves/")
echo "World/Saves: skip-worktree cleared for $n tracked file(s)."
