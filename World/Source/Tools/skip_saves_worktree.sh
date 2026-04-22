#!/usr/bin/env bash
# Mark tracked World/Saves files as skip-worktree so local server changes do not appear in status.
# Undo: git ls-files -z -- World/Saves/ | xargs -0 git update-index --no-skip-worktree

set -euo pipefail
cd "$(git rev-parse --show-toplevel)"
while IFS= read -r f; do
	[ -n "$f" ] && git update-index --skip-worktree "$f"
done < <(git ls-files -- World/Saves/)
echo "World/Saves: skip-worktree set for tracked files"
