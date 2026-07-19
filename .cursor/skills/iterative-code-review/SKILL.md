---
name: iterative-code-review
description: >-
  Orchestrate an automated multi-agent code review loop for a feature scope.
  NOT a shell command — the orchestrator agent reads this skill and runs the
  workflow using the Task tool. Agent A (Reviewer) writes a structured report,
  Agent B (Implementer) fixes or rejects each issue, then Agent A re-reviews —
  repeating until no issues remain. Use when the user says "iterative-code-review",
  "run a code review loop", or names a feature scope like "FR-02" or "fr02-fr05".
---

# Iterative Code Review Loop

Two sub-agents alternate — Reviewer and Implementer — until the Reviewer finds
no remaining issues.

**Project:** Outlook Case Classifier Add-in (`anchorai-email-group`) — React/Office.js
task-pane frontend (`src/`) + Vercel serverless API (`api/`) in a single
TypeScript monorepo.

---

## Orchestrator Instructions (READ FIRST)

This skill is an **agent workflow**, not a terminal command. There is no
`iterative-code-review` binary, npm script, or shell wrapper in this repo.

When the user sends a prompt like:

```
iterative-code-review fr01 \
  --specs doc/dev-log/2026-06-14-phase-0-scaffold-and-first-owa-test.md \
  --reviewer-model claude-5-sonnet \
  --implementer-model composer-2.5-fast
```

**You (the orchestrator agent) must:**

1. **Read this entire skill file** and follow it step by step.
2. **Parse the prompt** into parameters (see **User prompt syntax** below).
3. **Run the loop yourself** — launch Reviewer and Implementer sub-agents via the
   **Task tool** (`subagent_type: "generalPurpose"`), not via Shell.
4. **Verify outputs** after each sub-agent run (read the report file, check
   termination, resume if needed).
5. **Report completion** using the Completion Report template at the end.

**Do NOT:**

- Run `iterative-code-review` in Shell — it does not exist and will fail.
- Search for a CLI script, npm script, or wrapper to invoke this workflow.
- Delegate the entire loop to a single sub-agent — you are the orchestrator.
- Skip Step 0 (round detection) or the termination check between rounds.

**Tools the orchestrator uses:**

| Step | Tool | Purpose |
|------|------|---------|
| Parse args, detect round | Read, Glob, Shell (`git status`, `git diff`) | Prep only — not to run the skill |
| Launch Reviewer | **Task** (`subagent_type: "generalPurpose"`, `model: REVIEWER_MODEL`, **`run_in_background: false`**) | Block until review report is written |
| Verify termination | Read | Check `## Issues Found` in report |
| Launch Implementer | **Task** (`subagent_type: "generalPurpose"`, `model: IMPLEMENTER_MODEL`, **`run_in_background: false`**) | Block until response file is written |

> **CRITICAL — always use `run_in_background: false`** for both sub-agents.
> Background tasks (`run_in_background: true`) split the workflow across separate
> Cursor turns. When the completion notification arrives, the orchestrator has a
> minimal context prompt ("Perform any follow-up actions") and will lose the skill
> instructions, resulting in a malformed or empty sub-agent call and a stalled loop.
> Blocking keeps the entire loop inside a single turn with full context.

**The orchestrator must NOT do the sub-agents' work itself.** Do not read source
files, run `git diff`, or analyse code before launching the Reviewer — that is the
Reviewer's job. The orchestrator's only actions before launching a sub-agent are:
- Parse arguments
- Detect the current round (Glob + Read)
- Build and inject the prompt
- Call Task with `run_in_background: false`
- Read the output file to verify completion/termination

---

## User Prompt Syntax

The user invokes this skill with a **chat prompt**, not a shell command.
Parse these arguments from the user's message:

```
iterative-code-review <scope-slug> [--specs <path1,path2,...>] [--max-rounds <n>]
                                   [--reviewer-model <slug>] [--implementer-model <slug>]
```

| Argument | Description |
|----------|-------------|
| `scope-slug` | Short identifier for filenames, e.g. `fr02`, `fr02-fr05`, `phase-1` |
| `--specs` | Comma-separated extra spec paths (see **Spec resolution** below) |
| `--max-rounds` | Safety cap (default: 6) |
| `--reviewer-model` | Model for Agent A / Reviewer (default: `claude-opus-4-8-thinking-high`) |
| `--implementer-model` | Model for Agent B / Implementer (default: `composer-2.5-fast`) |

**Parsing notes:**

- `--specs` paths may be absolute or relative; normalize to repo-relative paths.
- If the user lists `doc/feature-requests.md` in `--specs`, still apply the FR
  section filter from **Spec resolution** — do not read the entire file blindly.
- Defaults apply when optional flags are omitted.

Examples (user chat prompts — not shell commands):

```
iterative-code-review fr02
iterative-code-review fr02-fr05 --max-rounds 5
iterative-code-review fr06 --specs doc/dev-log/2026-06-14-phase-0-scaffold-and-first-owa-test.md
iterative-code-review fr02-fr05 \
  --reviewer-model claude-opus-4-8-thinking-high \
  --implementer-model composer-2.5-fast
```

### Model Reference

| Slug | Best for |
|------|----------|
| `claude-opus-4-8-thinking-high` | Deep reasoning; finding subtle bugs and spec gaps — **default for Reviewer** |
| `claude-fable-5-thinking-high` | Strong thinking; good alternative for Reviewer on large diffs |
| `claude-4.6-sonnet-medium-thinking` | Balanced; good for Reviewer on smaller scopes |
| `composer-2.5-fast` | Fast code edits; high throughput — **default for Implementer** |
| `gpt-5.5-medium` | Alternative Implementer; strong at systematic multi-file edits |
| `gpt-5.3-codex` | Alternative Implementer; strong at code generation |

**Recommended pairings:**

| Use case | `--reviewer-model` | `--implementer-model` |
|----------|-------------------|-----------------------|
| Default (quality + speed) | `claude-opus-4-8-thinking-high` | `composer-2.5-fast` |
| Max quality (both agents strong) | `claude-fable-5-thinking-high` | `gpt-5.5-medium` |
| Fast iteration (small scopes) | `claude-4.6-sonnet-medium-thinking` | `composer-2.5-fast` |
| Cost-conscious | `claude-4.6-sonnet-medium-thinking` | `composer-2.5-fast` |
| Compliance-heavy scopes (FR-04, FR-06, FR-09) | `claude-opus-4-8-thinking-high` | `gpt-5.5-medium` |

Only use slugs from the list above. If the user requests an unavailable model, do not
substitute — report which models are available.

---

## Spec Resolution

Feature requirements live in a **single authoritative file**, not per-feature
markdown files. Resolve specs before launching Agent A.

### Default spec files (always read)

| File | Purpose |
|------|---------|
| `doc/feature-requests.md` | Acceptance criteria — read only the `### FR-NN` sections matching the scope |
| `doc/design-doc.md` | Architecture, API shapes, data models — read sections from the mapping table below |

### Scope slug → FR sections

Normalize the slug: lowercase, strip hyphens/underscores, extract `fr` + digits.

| scope-slug examples | FR sections to read in `feature-requests.md` |
|---------------------|-----------------------------------------------|
| `fr02` / `fr-02` / `FR-02` | FR-02 only |
| `fr02-fr05` | FR-02, FR-03, FR-04, FR-05 |
| `fr06` | FR-06 |
| `fr07-fr09` | FR-07, FR-08, FR-09 |
| `phase-1` | FR-02, FR-03 (regex extraction + UI shell per roadmap) |
| `phase-2` | FR-04, FR-05 |
| `phase-3` | FR-06, FR-08, FR-09 |

### Scope slug → design-doc sections

| Scope | Read in `doc/design-doc.md` |
|-------|----------------------------|
| FR-01 | §2.1, §4.1, §10.1 |
| FR-02 | §3.1, §3.2, §3.6, §7.3, §8.1 |
| FR-03 | §3.1, §3.3, §3.6, §7.3 |
| FR-04 | §3.1, §3.4, §7.3, §8.2 |
| FR-05 | §3.1, §3.5, §4.2 |
| FR-06 | §5.2 (`/api/file-to-onedrive`), §6.2, §6.3 |
| FR-07 | §4.2, §4.3, §8.1 |
| FR-08 | §5.2 (`/api/cases/list`), §7.1 |
| FR-09 | §5.2 (`/api/audit/log`), §7.2 |

### Scope slug → code paths (review focus)

| Scope | Primary paths |
|-------|---------------|
| FR-01 | `src/`, `manifest.xml`, `taskpane.html`, `src/hooks/useEmailItem.ts` |
| FR-02–FR-05 | `api/extract/`, `config/case-patterns.json`, `src/components/ClassificationPanel.tsx`, `src/services/api.ts`, `test/unit/`, `test/integration/api/extract.test.ts` |
| FR-06 | `api/file-to-onedrive/`, `test/integration/api/file-to-onedrive.test.ts` |
| FR-07 | `src/components/`, `src/App.tsx` |
| FR-08 | `api/cases/`, `config/known-cases.json`, `test/integration/api/cases-list.test.ts` |
| FR-09 | `api/audit/`, `test/integration/api/audit-log.test.ts` |

Append any `--specs` paths to the spec file list passed to sub-agents.

---

## Step 0 — Determine Current Round

Ensure `doc/code-review/` exists (`mkdir -p doc/code-review` on first run).

Scan `doc/code-review/` for files matching `code-review-{scope-slug}-round*.md`.
Count existing review files to find `CURRENT_ROUND`:

```
No files found                              → CURRENT_ROUND = 1  (fresh start)
round1.md exists                            → CURRENT_ROUND = 2  (round 1 review done; response may be pending)
round1.md + round1-response.md both exist   → CURRENT_ROUND = 2, proceed to Reviewer pass
```

**Resume logic:** if `round{N}.md` exists but `round{N}-response.md` does not,
skip the Reviewer and launch only the Implementer for round N before continuing
the loop.

---

## Step 1 — Reviewer Sub-Agent (Agent A)

Launch via the **Task tool** with:
- `subagent_type: "generalPurpose"`
- `model: REVIEWER_MODEL`
- `run_in_background: false` ← **required; do not omit**

> **Why `generalPurpose`:** The Reviewer must write the review report file.
> `explore` is read-only and cannot write files. Source-code changes are blocked
> by the WRITE CONSTRAINT inside the prompt, not by the subagent type.
>
> **Why `run_in_background: false`:** Keeps the whole loop in one turn. If you
> use `run_in_background: true`, the orchestrator will receive a notification in
> a separate turn with minimal context ("Perform any follow-up actions"), lose the
> skill instructions, and produce a malformed or empty Implementer call — stalling
> the workflow entirely.

### Output file

```
doc/code-review/code-review-{scope-slug}-round{N}.md
```

### Prompt template

```
You are a strict code reviewer for the Outlook Case Classifier Add-in
(anchorai-email-group).

Your task: review the current implementation of [{scope}] and write a
structured report to `doc/code-review/code-review-{scope-slug}-round{N}.md`.

IMPORTANT WRITE CONSTRAINT: You may write ONLY the review report file at
`doc/code-review/code-review-{scope-slug}-round{N}.md`. Do NOT edit, create,
or delete any source files, test files, or config files — those are the
Implementer's responsibility.

Spec files to read first:
{spec_file_list}

FR sections to focus on in feature-requests.md:
{fr_section_list}

Design-doc sections:
{design_doc_sections}

Code paths to prioritize:
{code_paths}

Prior review/response pairs (read all of them before reviewing):
{prior_rounds_list}

---

## Project rules (read before reviewing)

- .cursor/rules/01-project-context.mdc
- .cursor/rules/02-dev-practices.mdc
- .cursor/rules/03-developer-interaction.mdc
- .cursor/rules/04-compliance-privacy.mdc

---

Review the UNCOMMITTED working-tree changes (`git diff HEAD` and `git status`
untracked files). Also read the committed code in the scope paths above — the
review is against spec compliance, not only the diff.

For each issue found, classify severity:

- **Core** — wrong behaviour, compliance violation, or logic that could file
  an email to the wrong case (catastrophic in this domain)
- **Primary** — missing acceptance criterion from the spec; blocks merge
- **Secondary** — style, naming, missing edge-case test, minor refactor

---

## Mandatory compliance checks (flag any violation as Core)

- [ ] Human-in-the-loop: no auto-filing without explicit user confirmation
- [ ] Tier progression is strictly sequential 1 → 2 → 3 → 4 (never skip/parallel)
- [ ] Regex patterns live in `config/case-patterns.json` only — not hardcoded
- [ ] Tier 3 AI result validated against known case list before acceptance
- [ ] Audit log stores metadata only — no email body content
- [ ] Access tokens in `sessionStorage` only — never `localStorage`
- [ ] API responses use `ExtractionResult`, `AuditEntry`, `KnownCase` types from design-doc §7
- [ ] Confidence thresholds: ≥0.90 high, 0.70–0.89 medium, <0.70 manual UI
- [ ] Structured error envelope on all API failures (`success: false`, `requestId`)

---

## Extraction-pipeline checks (when scope includes FR-02–FR-05)

- [ ] Tier 2 pre-processes body: strip reply chains, signatures, HTML
- [ ] First matching regex pattern wins; no further patterns evaluated
- [ ] Tier 3 uses `gpt-4o-mini`, returns UNCERTAIN when unsure, retries once on malformed JSON
- [ ] Tier 4 always available as manual fallback

---

Use EXACTLY this file structure (do not deviate — the loop checks for these
exact headings):

```markdown
# Code Review: {scope} — Round {N}

| Field | Value |
|-------|-------|
| **Review date (UTC)** | {ISO datetime} |
| **Prior reviews** | {links to prior rounds} |
| **Reviewer** | AI code review (Cursor Agent) |
| **Reviewer model** | {REVIEWER_MODEL} |
| **Scope** | {one-sentence scope description} |
| **FR sections** | {e.g. FR-02, FR-03} |

---

## Executive Summary

{paragraph}

---

## Compliance Checklist

| Check | Status |
|-------|--------|
| Human-in-the-loop preserved | ✅ / ❌ / N/A |
| No email body in audit logs | ✅ / ❌ / N/A |
| Tier progression correct | ✅ / ❌ / N/A |
| Types match design-doc §7 | ✅ / ❌ / N/A |

---

## Prior Response — Verification

{table verifying each claim from the prior response, or "N/A — first round"}

---

## Issues Found

{numbered list, or write exactly `None.` if no issues remain}

---

## Verdict

{APPROVED or CHANGES REQUESTED}
```

TERMINATION RULE: if you find no issues, write **exactly** `None.` (with the
period) under `## Issues Found` and set **Verdict** to `APPROVED`.
```

### Termination check

After the sub-agent completes, read `doc/code-review/code-review-{scope-slug}-round{N}.md`.

Check the `## Issues Found` section. If it contains only `None.` (case-insensitive
match: `/^none\.?\s*$/m`), **stop the loop** and report completion.

---

## Step 2 — Implementer Sub-Agent (Agent B)

Launch via the **Task tool** with:
- `subagent_type: "generalPurpose"`
- `model: IMPLEMENTER_MODEL`
- `run_in_background: false` ← **required; do not omit**

> **Orchestrator:** Use the Task tool — do NOT run a shell command.

### Output file

```
doc/code-review/code-review-{scope-slug}-round{N}-response.md
```

### Prompt template

```
You are a careful implementer for the Outlook Case Classifier Add-in
(anchorai-email-group).

Read the code review report at:
  `doc/code-review/code-review-{scope-slug}-round{N}.md`

Project rules (follow strictly):
- .cursor/rules/01-project-context.mdc
- .cursor/rules/02-dev-practices.mdc
- .cursor/rules/03-developer-interaction.mdc
- .cursor/rules/04-compliance-privacy.mdc

For EACH numbered issue:
  - If valid: apply the fix in the codebase, then describe what you changed.
  - If invalid or out-of-scope: write a clear rejection with reasoning.
  - Do NOT silently skip any issue — every issue must have an explicit action.

## Escalation — do NOT fix silently; reject with reasoning instead

- Regex pattern content changes in `config/case-patterns.json` → reject; patterns
  are stakeholder-owned (see 03-developer-interaction.mdc)
- Weakening human-in-the-loop or confidence thresholds → reject
- Adding auto-filing logic → reject
- Storing email body or new PII fields in audit log → reject
- Changing AI model away from `gpt-4o-mini` → reject
- Modifying the hallucination guard in the Tier 3 system prompt → reject

After all fixes/rejections, write your response to:
  `doc/code-review/code-review-{scope-slug}-round{N}-response.md`

---

Use EXACTLY this file structure:

```markdown
# Code Review Response: {scope} — Round {N}

| Field | Value |
|-------|-------|
| **Response date** | {date} |
| **Review referenced** | [Round {N}](./code-review-{scope-slug}-round{N}.md) |
| **Author** | AI agent (Cursor) |
| **Implementer model** | {IMPLEMENTER_MODEL} |
| **Scope** | {one-sentence description} |

---

## Summary

| Issue | Severity | Action | Status |
|-------|----------|--------|--------|
| {number} — {title} | {severity} | {Fixed / Rejected} | ✅ / ❌ |

---

## Issue {N} — {title} {✅ Fixed / ❌ Rejected}

**Assessment:** {Accept or reject with reason}

**Fix / Rejection reason:**
{description and code snippets where applicable}

---

## Validation

| Command | Result |
|---------|--------|
| `npm run typecheck` | ✅ pass / ❌ fail |
| `npm run lint` | ✅ pass / ❌ fail |
| `npm test` | ✅ pass / ❌ fail |
| `npm run test:integration` | ✅ pass / ❌ fail / skipped |
| `npm run test:eval` | ✅ pass / ❌ fail / skipped |

{Include failure output if any command failed.}

---

## Pre-Merge Checklist (Updated)

- [x] {completed items}
- [ ] {remaining items}
```

---

## Validation commands (run from repo root)

Always run after fixes:

```bash
npm run typecheck
npm run lint
npm test
```

Run additionally when relevant:

```bash
# API or integration test changes
npm run test:integration

# Extraction pipeline or config/case-patterns.json changes (Phase 4+)
npm run test:eval
```

Report results in the `## Validation` section of the response file.
```

---

## Step 3 — Loop Control

The **orchestrator agent** (you) runs this loop synchronously in a **single turn**.
All Task calls must use `run_in_background: false` so control returns here
immediately after each sub-agent finishes.

```
REVIEWER_MODEL    = --reviewer-model    || claude-opus-4-8-thinking-high
IMPLEMENTER_MODEL = --implementer-model || composer-2.5-fast

loop:
  N = CURRENT_ROUND

  // ── Reviewer pass ──
  Task (generalPurpose, run_in_background: false, model: REVIEWER_MODEL)
    → writes doc/code-review/code-review-{slug}-round{N}.md
  Read round{N}.md
  if ## Issues Found == "None."  →  DONE (go to Completion Report)

  // ── Implementer pass ──
  Task (generalPurpose, run_in_background: false, model: IMPLEMENTER_MODEL)
    → writes doc/code-review/code-review-{slug}-round{N}-response.md
  Read round{N}-response.md  (confirm file was written)

  N++
  if N > MAX_ROUNDS  →  STOP, report max rounds reached
```

### Recovery — if a sub-agent call fails or produces no output file

If after a Task call the expected output file does not exist or is empty:

1. Read the file (confirm it is missing/empty).
2. Retry the same Task call **once** with the same prompt — do not skip to the
   next step.
3. If the retry also produces no file, stop the loop and report:
   `"Sub-agent failed to write output after 2 attempts — manual intervention required."`
   Include the last known state (round number, which agent failed, scope slug).

---

## Completion Report

When the loop terminates (approved or max rounds), output a summary:

```
Code review loop complete for [{scope}].

Rounds run:          {N}
Final verdict:       APPROVED / MAX ROUNDS REACHED
Reviewer model:      {REVIEWER_MODEL}
Implementer model:   {IMPLEMENTER_MODEL}

Files written:
  doc/code-review/code-review-{scope-slug}-round1.md
  doc/code-review/code-review-{scope-slug}-round1-response.md
  ...

Next steps:
  1. Manual QA per the Pre-Merge Checklist in the final response file
  2. Sideload add-in and verify in Outlook on the Web if frontend changed
  3. Stage feature files and commit (only when user requests)
  4. Open PR to main — CI runs lint, unit, integration, and eval gates
```

---

## File Naming Reference

| Round | Reviewer output | Implementer output |
|-------|----------------|-------------------|
| 1 | `code-review-{slug}-round1.md` | `code-review-{slug}-round1-response.md` |
| 2 | `code-review-{slug}-round2.md` | `code-review-{slug}-round2-response.md` |
| N | `code-review-{slug}-round{N}.md` | `code-review-{slug}-round{N}-response.md` |

All files live in `doc/code-review/`.

---

## Project Context for Sub-Agent Prompts

Always inject this block into both sub-agent prompts:

```
Project: Outlook Case Classifier Add-in (anchorai-email-group)
Repo layout:
  src/          React + Office.js add-in (Fluent UI, Vite)
  api/          Vercel serverless functions (Node 20, TypeScript)
  config/       known-cases.json, case-patterns.json
  test/         Vitest unit + integration + ground-truth eval

Architecture rules:  .cursor/rules/01-project-context.mdc
Dev practices:       .cursor/rules/02-dev-practices.mdc
Interaction rules:   .cursor/rules/03-developer-interaction.mdc
Compliance/privacy:  .cursor/rules/04-compliance-privacy.mdc

Feature specs:       doc/feature-requests.md (FR-NN sections)
Technical design:    doc/design-doc.md
Code review output:  doc/code-review/

Key constraints:
  - Human-in-the-loop always — never auto-file
  - Cheapest path first: regex (Tier 1–2) before AI (Tier 3)
  - Fail open: uncertain → manual selector (Tier 4)
  - >99% precision target for case filing
  - No database for MVP — static JSON configs
```
