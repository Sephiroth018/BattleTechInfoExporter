# Repository setup

What's needed once per repository. No `CLAUDE.md` imports this file: read it when setting up a
repository or copying the shared files by hand.

## New repository setup

Once per repository, before the rest of the workflow applies:

1. **First commit, directly on `main` and without an issue** (`chore: set up repository`):
   `CLAUDE.md` importing the shared files, their copy in `.claude/shared/` (see "Copying the
   shared files by hand"), the stack's standard `.gitignore`, a `.gitattributes` fixing line
   endings to LF on every machine (`* text=auto eol=lf`, CRLF only for `.cmd`/`.bat`), a README
   with the project goal and the AI disclaimer, the LICENSE (MIT unless the project says
   otherwise) and the initial project skeleton.
2. **Create the GitHub repository** (name and visibility decided with the user) and push.
3. **Repository settings:** merge commits only (no squash or rebase merging), default merge message
   from the PR title and description, head branches deleted automatically after merging.
4. **Ruleset on `main`:** changes only through PRs, signed commits required, no force pushes or
   deletion, and the CI checks required once CI exists.
5. **Labels:** delete `good first issue`, `help wanted`, `accessibility` and `invalid`; add `chore`
   (build, tooling, dependencies), `refactor` (restructuring without behavior change) and
   `research` (finding out how something works before building on it). Work that is only about
   tests gets `enhancement` or `chore`.
6. **CI** is a GitHub Actions workflow, `.github/workflows/ci.yml`, running on pushes to `main`
   and on PRs, but not on drafts: every job has `if: ${{ !github.event.pull_request.draft }}`, and
   the `pull_request` trigger has `types: [opened, synchronize, reopened, ready_for_review]`.
7. **No Dependabot or other scheduled workflows**: they use up Actions minutes even on an abandoned
   repository.
8. **Register the repository in `claude-instructions`** with an issue there: its update workflow's
   list gets the repository, and the user adds it to the workflow's token.

## Copying the shared files by hand

For a new repository's first copy, and for an update `claude-instructions` opened no PR for:
`git fetch https://github.com/Sephiroth018/claude-instructions main`, then
`git rm -r -q .claude/shared` (not the first time) and
`git read-tree --prefix=.claude/shared -u FETCH_HEAD:shared`. The first copy is part of the
repository's first commit; an update gets its own commit (see "Hygiene" in `general.md`).
