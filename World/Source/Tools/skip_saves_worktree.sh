#!/usr/bin/env bash
# Mark every tracked file under World/Saves/ as skip-worktree so local server writes do not
# show as modified. Untracked files/dirs (Achievements, Craft, Player, new .bin, etc.) are
# hidden via .gitignore: World/Saves/**
#
# Unset: bash World/Source/Tools/unsave_saves_worktree.sh

set -euo pipefail
cd "$(git rev-parse --show-toplevel)"
n=0
while IFS= read -r -d '' f; do
	git update-index --skip-worktree "$f"
	n=$((n + 1))
done < <(git ls-files -z -- "World/Saves/")
echo "World/Saves: skip-worktree set for $n tracked file(s)."
