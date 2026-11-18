# Lua

## Toolchain and enforcement

Tools enforce every rule they can; a rule a tool could check belongs in its configuration, not in
this file.

- **StyLua is the formatter** and `stylua.toml` the single source of formatting rules: 2-space
  indents, double quotes, `line_endings = "Unix"` to match `.gitattributes`. Its defaults (tabs)
  aren't the community's, so the file sets them.
- **EmmyLua is the type checker and linter**, run from the command line as `emmylua_check
  --warnings-as-errors` so its results don't depend on anyone's editor (without the flag, warnings
  don't fail it). `.emmyrc.json` holds the runtime version, the library definitions and the enabled
  diagnostics; never loosen it to make a change pass.
- **The Lua version is the host's**, set in `.emmyrc.json`: an embedded Lua (a game's) is checked
  against that host's version and its API definitions, not a standalone Lua.
- **Tests, where a project has them, use busted.** Spec files are `<module>_spec.lua` in `spec/`,
  mirroring the source folders; a test reaches internals only through what the module returns.
  Names are busted's sentences: `describe("parse")` around `it("returns no weapons for empty
  input")`.

## Lua naming

The usual Lua style, which the game APIs Lua is embedded in (Factorio's among them) follow too:

- **`snake_case`** for locals, functions, fields, parameters and module files
  (`export_file.lua`, `write_export`, `is_researched`).
- **`PascalCase`** only for `---@class` names (`ExportFile`), matching how type definitions name
  host types (`LuaTechnology`).
- **`UPPER_SNAKE_CASE`** for module-level constants that never change (`EXPORT_FILE_NAME`).
- **`_` for a value deliberately ignored** (`for _, technology in pairs(...)`), a `_` prefix for an
  ignored parameter; never for a value that's used.
- **A module's table is named after what it is**, the same name as its file, and a `require` result
  is bound to that name (`local export_file = require("export_file")`).

## Lua conventions

What the tools enforce lives in `stylua.toml` and `.emmyrc.json`; this section holds what needs
judgment.

- **`local` everything.** No globals, except what a host requires to be global.
- **A module is a file returning one table** (or one function), loaded with `require`; it changes no
  global state on load. Functions only the module uses stay `local` to it.
- **Strictly typed: every function carries EmmyLua annotations** (`---@param`, `---@return`), local
  ones included, and `---@type` where a value's type can't be inferred. They're Lua's types, so the
  type checker can check callers; the `incomplete-signature-doc` diagnostic enforces them (a
  callback passed to a typed API is typed by it). This overrides "Doc comments only where name and
  signature aren't enough" for annotations; prose descriptions still follow it.
- **A table shape is a `---@class`**, and the "Types say what things are" and "A choice is
  structure" rules apply to classes as they do to types.
- **A cast (`--[[@as T]]`) where the host's type definitions are looser or wrong** compared with
  its documented behavior, with a short comment naming the documented fact. Prefer one function
  that casts over casts at every call site; the "Project" part lists the known cases.
- **No other escape hatches** (`---@diagnostic disable`, a cast to silence a mismatch in our own
  code, `any`) without a narrow, stated reason. Where a project needs one, its "Project" part says
  where.
- **No mutable module-level state** unless the host requires it, documented under "Project".
- **`#` only on sequences.** Tables with other keys are counted explicitly.
- **`nil` means absent, `false` means no.** A field that can be either says which in its
  annotation.
- **`error` for broken invariants, never as control flow;** `pcall` only where the caller handles the
  failure, and never to hide one (see "Fail loud").
