# General

## Working agreement

- **Plan architecture before building it.** Anything that shapes structure (new projects or layers,
  dependencies, data formats, cross-cutting patterns) gets a written plan first, presented in chat,
  and waits for the user's sign-off. Work inside an agreed design doesn't need one. A signed-off
  parent plan covers what to build, not how; each issue's implementation approach gets its own
  short plan unless it's trivial.
- **Stop when the plan breaks.** If a problem mid-task invalidates the agreed approach, stop and ask
  how to proceed, with concrete options and a recommendation. Never switch approach silently.
- **Say so when there's a better way.** Raise it before implementing, with the trade-off, then do
  what the user decides.
- **Ask before widening scope.** Mention unrelated problems you notice and ask; don't fix them in
  passing.
- **English everywhere:** code, comments, docs, commits, issues and PRs.
- **Mark everything you write on GitHub** (issues, comments, PRs, review replies, releases) by
  ending it with `🤖 Generated with [Claude Code](https://claude.com/claude-code)`. Commits carry
  a `Co-Authored-By: Claude` trailer.
- **Keep chat reports short.** A line or two on what was done. Explain at length only what's
  non-standard, what went wrong, or what needs a decision.
- **Standing authorization** covers the source control steps below, except merging. Other
  destructive or history-rewriting operations (`reset --hard`, deleting unmerged branches, any
  force push not covered there) always need asking.
- **Required project sections:** "Verification" and "Versioning" under "Project" must each say how
  it's done, or that the project has none. If one is missing or not yet defined, tell the user
  before relying on it.
- **The General part and the language parts are shared files**, kept in the `claude-instructions`
  repository and copied into every other repository's `.claude/shared/`, where `CLAUDE.md` imports
  them. They're changed only in `claude-instructions`: when a change belongs in one, say so instead
  of editing the copy.

## New repository setup

Once per repository, before the rest of the workflow applies:

1. **First commit, directly on `main` and without an issue** (`chore: set up repository`):
   `CLAUDE.md` importing the shared files, their copy in `.claude/shared/` (see "Hygiene"), the
   stack's standard `.gitignore`, a `.gitattributes` fixing line endings to LF on every machine
   (`* text=auto eol=lf`, CRLF only for `.cmd`/`.bat`), a README with the project goal and the AI
   disclaimer, the LICENSE (MIT unless the project says otherwise) and the initial project
   skeleton.
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

## Source control

### Issues and projects

- **Every change starts from a GitHub issue**, small ones included. Search for an existing one
  before creating it.
- **A session renames itself to `Issue #<nr>: <issue title>`** as soon as it starts working on or
  investigating an issue ("Issue #239: Add a retry limit to the upload queue"). A
  session started for an issue from another session gets that title from the start.
- **The issue records every decision about it.** Signed-off plans and later decisions go into the
  description while work hasn't started, and into a new comment once it has.
- **Labels:** apply the repo's existing labels that fit; ask before creating a new one. They
  describe what the issue is, independent of the commit types its work uses: a `bug` issue can
  still have `refactor` commits that came up in review.
- **Help manage the GitHub Project** for planned features: break features into issues, add them to
  the project, and keep their status current as work starts and lands.

### Branches and commits

- **Never commit to `main`.** Branch off an up-to-date `main` as `<issue>-<short-slug>`
  (`12-upload-retry-limit`). If the session assigned a branch (a desktop worktree), use that
  one.
- **Commit messages:** `<type>(#<issue>): <imperative summary>`, with type `feat`, `fix`,
  `refactor`, `docs`, `test` or `chore` (build, dependencies, CI, tooling).
- **The per-commit rule:** every commit lands on `main` (PRs merge with merge commits), so each one
  is formatted with the language part's formatter, builds or loads, and breaks nothing that worked.
  New code may be unreachable or incomplete, but nothing crashes, the existing tests still pass,
  and the static checks report nothing at warning level or above except what that unfinished state
  causes (unreachable or unused code). It's checked before each commit.
- **Push every commit** right after making it.
- **Moves and renames get their own commit**, made with `git mv`, containing only the move and the
  minimal edits it forces (module paths, references), so git keeps following the file's history.
- **Rewriting is the exception, and only while the PR is a draft.** Prefer new commits. Amend or
  rebase (pushed with `--force-with-lease`) only when a commit is wrong on its own (it breaks the
  per-commit rule or has a wrong message), not when the work merely moved on. Once the PR is ready
  for review, changes are new commits so they can be reviewed on their own, in the normal message
  format (usually `refactor` or `chore`; no `fixup!` commits). `main` is never rewritten.
- **Commits are signed through 1Password's SSH agent.** If a commit or push fails because the agent
  is locked, stop, tell the user and wait. Don't retry in a loop, and never bypass signing.

### Pull requests

- **Open a draft PR into `main`** right after the branch's first commit. Its title follows the
  commit message format; it becomes the merge commit's message. The description starts with
  `Closes #<issue>`; title and description may stay provisional while it's a draft.
- **Mark the PR ready for review** once the change is done (see "Definition of done"), with its
  title and description final. The description explains what the code can't (why, rejected
  alternatives, risks, how it was verified, what deserves a close look). No file-by-file list.
- **Watch the PR** once it's ready: turn on the desktop app's monitoring (auto-fix CI, address
  comments). Fix CI failures at the cause; never skip or weaken a check.
- **The user's own review comments fire no events** (a solo author can only leave "comment"
  reviews). When the user says they've reviewed, read all three surfaces: PR comments, inline diff
  comments and review summaries. Answer each comment on the PR, inline ones in their own thread.
- **Before merging, check what `main` gained since the branch was cut**
  (`git log --first-parent --oneline <merge-base>..origin/main`). If another PR landed, merge
  `main` into the branch and redo what depends on `main`'s state, such as the lint and checks when
  the merge touched code.
- **Merge only after the user's go-ahead in chat, given after the latest commit.** A later commit
  that changes the PR's diff needs a fresh go-ahead; a conflict-free update from `main` doesn't.
  Merge with a merge commit, and never delete the remote branch yourself (no `--delete-branch`, no
  `git push --delete`): the repository deletes it after the merge, and only then does GitHub
  retarget the PRs stacked on it to `main`; a branch deleted by a client closes them instead.
- **After merging:** update `main` (`git pull --prune` on it; in a worktree, `git fetch --prune`)
  and delete the local branch, unless it's the worktree's own.

## Definition of done

A draft's commits only have to meet the per-commit rule (see "Branches and commits"). A change is
done when all of these hold, checked in this order before the PR is marked ready and before every
later push:

1. **Tests cover the new or changed behavior** (see "Testing"). A gap means the change isn't done.
2. **Formatted** with the language part's formatter, on the changed files.
3. **Builds or loads** with no errors or warnings.
4. **All tests pass.**
5. **Static checks are clean:** the language part's linter or inspections report nothing at warning
   level or above.
6. **Nothing is left stale:** comments next to changed code, the README and `CLAUDE.md` (commands,
   architecture, glossary) still match.
7. **The diff is self-reviewed** against `CLAUDE.md` and the shared files it imports: scope,
   naming, leftover debug code.

Steps 2-5 are what CI runs; running them first keeps CI from being where problems are found.

Before marking the PR ready, with the branch as target (its whole diff against `main`, not just
uncommitted changes):

1. **`/simplify`**, then steps 2-7 again for what it changed. A suggested fix that changes design or
   scope is asked about, not applied.
2. **`/code-review` at medium.** Fix the findings, or explain in the PR why one isn't a problem.

Later commits that change behavior get both again, `/simplify` first; trivial follow-ups need
neither. Nor does a PR that only updates `.claude/shared/`: the shared files are reviewed in their
own repository.

**Merging needs green CI.** If the project's "Verification" section defines manual checks (game
mods do: the game can't run in CI), they also become a checklist in the PR description, ticked by
the user before the go-ahead.

## Engineering principles

Apply these to every change and check them in every review.

### Design

- **Simplest thing that works.** No abstractions, options or extension points for needs that don't
  exist yet. Expressing a concept the code already has isn't a future need.
- **One source of truth.** No duplicated logic, constants or data. Search for an existing helper or
  type before writing a new one.
- **Types say what things are.** A concept several types share is a shared type: a base for the
  common fields, an interface for a role unrelated types play. Judge by concept, not by whether
  code uses the shared part today. Two types that merely look alike stay separate.
- **A thing that passes through states gets one type per state**, derived from the thing's base:
  an order that is first a draft and then placed is an order base with a draft order and a placed
  order. Each state also has an interface its types share, so a list of placed things is typed by
  what "placed" guarantees.
- **A choice is structure, not nullable fields.** "Only the fields its type uses are set" means one
  derived type per case. Fields that are null together become one nullable nested type.
- **Generics where only a member's type varies**, never its name or meaning. Where members are
  added, use a base type.
- **A type owns what's about itself**: its shape, its comparison, how it's written out. Code that
  handles many types knows only their common base.
- **Pure logic in the middle, side effects at the edges.** I/O, external systems, time and
  randomness stay at the boundaries, so the core is testable without them.
- **Validate at boundaries, trust inside.** Input is checked where it enters; inner code doesn't
  re-check it.
- **Persisted formats and public interfaces change only by decision.** Changing save data, exported
  files, config or a public API needs a plan covering migration or compatibility.

### Errors

- **Fail loud.** Never swallow an error silently. Catch only what you can handle; exceptions aren't
  control flow.
- **Don't silence the compiler, type checker or linter** without a narrow, stated reason (see the
  language part's conventions).

### Dependencies

- **Prefer an established package over hand-rolled logic**, but adding one is an architecture
  decision (see "Working agreement"). It must be mainstream, in a stable release, under a compatible
  license, and either actively maintained or feature-complete with no known open security issues.
- **Latest stable version only**, never a preview, RC or pre-release. Pin explicitly if a range
  would resolve to a pre-release.

### Hygiene

- **Change files only with the Edit and Write tools**, never with shell commands or scripts, so
  every edit shows as a reviewable diff. Exceptions: `git mv`, the language part's formatter, its
  CLI's own project and package commands, output of the project's own generators, and the update
  of `.claude/shared/`.
- **`.claude/shared/` is replaced as a whole, never edited.** When the shared files change,
  `claude-instructions` opens the issue and PR for it in every repository on its list. By hand, as
  for a new repository's first copy: `git fetch https://github.com/Sephiroth018/claude-instructions
  main`, then `git rm -r -q .claude/shared` (not the first time) and
  `git read-tree --prefix=.claude/shared -u FETCH_HEAD:shared`. An update is committed on its own
  as `chore(#<issue>): update shared instructions to <short sha>`, naming the fetched commit; the
  first copy is part of the repository's first commit.
- **Clean up and modernize the code you touch.** Small fixes and current idioms within the lines
  being changed are part of the change; anything beyond them is asked about first.
- **No dead code.** Delete it rather than comment it out; git keeps the history.
- **No `TODO` without an issue number.**
- **Never commit secrets**: no keys, tokens or personal paths in code, config or logs.

## Naming

Casing and other mechanics are in the formatter's and linter's configuration and the language
part's conventions; the project's settled terms are in its glossary. Check every new or changed
name against these rules, and check them in every review.

- **Reuse the existing term.** Search for the concept first and use the word the code already uses;
  never add a synonym.
- **One term, one concept.** Don't stretch an existing word over a second meaning; pick a distinct
  one.
- **Say what it is or returns, all of it.** A name true of only part of the value misleads.
- **Be specific.** No vague names (`data`, `info`, `details`, `item`, `value`, `result`, `obj`,
  `temp`). A role suffix (handler, manager, helper, processor, utils) is fine only when the rest
  of the name says exactly what it handles: header click handler, not handler or header handler.
  Examples here are in plain words; the language part sets the casing.
- **Domain words, not invented ones.** Use the problem domain's terms; no project jargon, made-up
  metaphors or cute names.
- **Grammar matches role.** Functions are verb phrases, and one that changes state says so in its
  verb. Fields are nouns. Booleans are positive and read as a question (is visible, has children,
  can retry; not not hidden).
- **Write words out.** Only widely known abbreviations (`id`, `url`, `json`, `min`, `max`).
- **Short scopes are the exception:** loop variables, short lambdas and comparers may use single
  letters or vague names.
- **External names stay in their own world.** An external system's spelling (an API's field names,
  a file format's keys) appears only in its values and in names that refer to one of them. Our
  code is named by what it does; when it mirrors an external function, cite that function's name
  in a comment.
- **A rename is finished only when everything follows:** comments, tests and their names, docs,
  config keys and the glossary. Renaming something persisted falls under "Persisted formats".
- **New or settled domain terms** are proposed to the user for the glossary, not added unasked.

## Comments and documentation

### Comments

Write a comment only for what the code can't show: why, an invariant or ordering constraint, a
deliberate workaround, or an external fact with its source (e.g. the API member it comes from).

- **Short:** one or two lines.
- **No history or discussion narration**; that belongs in the commit message.
- **Don't restate** the code or a nearby comment.
- **Refer to other code by symbol name, never by file path,** ours and external code's alike (e.g.
  a library's members). Where the language's doc comments can link a symbol, link it so renames
  follow; elsewhere the plain name, as `(Type.member)` where it's a side reference.
- **Change a comment in the same edit as its code**, or delete it.
- **No issue links**, except the issue number of a `TODO` or a skipped test, and to explain code
  that deliberately looks like a bug when the reason is too long to state inline.
- **Nothing machine-local:** no paths or notes that exist on one machine only; cite the original
  source.
- **Doc comments only where name and signature aren't enough**, apart from what the language part
  requires for its type checker. A project that ships a library for others may require more in its
  own section.

### Documentation

- **The README** says what the project is, how to install and use it, and how to build it. It
  never refers to issues: it describes the project as it is, not how it got there.
- **The README's `## AI disclaimer` section** holds the standard text, unchanged:

  ```markdown
  This project is AI-assisted: [Claude Code](https://claude.com/claude-code) does most of the
  implementation, under the direction and review of the maintainer. Commits written by Claude
  carry a `Co-Authored-By: Claude` trailer, and everything it posts on GitHub is marked as
  generated with Claude Code.
  ```
- **Decisions live in their GitHub issue**, not in separate decision records (see "Issues and
  projects").

## Testing

- **A bug fix starts with a failing test** that reproduces it, committed together with the fix.
- **Test behavior through the non-private API**, internals included, not implementation details.
  How tests reach internals is in the language part; never through hacks that break encapsulation.
- **Real collaborators.** A test double only at a boundary (file system, clock, randomness,
  external systems) or when the interaction itself is what's tested, made with the
  language part's mocking library; no hand-written fakes.
- **One behavior per test**, as Arrange-Act-Assert, with no loops or conditionals. Table-driven
  cases use the test framework's parameterized tests.
- **Deterministic:** no real time, randomness, network or sleeps, and no dependence on test order or
  shared mutable state.
- **Complex test data comes from typed builders** in a shared test-support folder.
- **Don't test what a library or the framework already guarantees.**
- **A failing or flaky test is a bug.** Never skip or disable one without an issue number.
- **Names** say the unit, the scenario and the expected result (parse, empty input, returns no
  entries), grouped by the unit under test; the form and the layout are in the language part.
- **Coverage thresholds are enforced in CI and only go up**: raise them when a change adds coverage.
  A project may set a different policy under "Project".
