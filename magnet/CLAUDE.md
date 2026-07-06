# CLAUDE.md

Behavioral guidelines to reduce common LLM coding mistakes. Merge with project-specific instructions as needed.

**Tradeoff:** These guidelines bias toward caution over speed. For trivial tasks, use judgment.

## 1. Think Before Coding

**Don't assume. Don't hide confusion. Surface tradeoffs.**

Before implementing:
- State your assumptions explicitly. If uncertain, ask.
- If multiple interpretations exist, present them - don't pick silently.
- If a simpler approach exists, say so. Push back when warranted.
- If something is unclear, stop. Name what's confusing. Ask.

## 2. Simplicity First

**Minimum code that solves the problem. Nothing speculative.**

- No features beyond what was asked.
- No abstractions for single-use code.
- No "flexibility" or "configurability" that wasn't requested.
- No error handling for impossible scenarios.
- If you write 200 lines and it could be 50, rewrite it.

Ask yourself: "Would a senior engineer say this is overcomplicated?" If yes, simplify.

## 3. Surgical Changes

**Touch only what you must. Clean up only your own mess.**

When editing existing code:
- Don't "improve" adjacent code, comments, or formatting.
- Don't refactor things that aren't broken.
- Match existing style, even if you'd do it differently.
- If you notice unrelated dead code, mention it - don't delete it.

When your changes create orphans:
- Remove imports/variables/functions that YOUR changes made unused.
- Don't remove pre-existing dead code unless asked.

The test: Every changed line should trace directly to the user's request.

## 4. Goal-Driven Execution

**Define success criteria. Loop until verified.**

Transform tasks into verifiable goals:
- "Add validation" → "Write tests for invalid inputs, then make them pass"
- "Fix the bug" → "Write a test that reproduces it, then make it pass"
- "Refactor X" → "Ensure tests pass before and after"

For multi-step tasks, state a brief plan:
```
1. [Step] → verify: [check]
2. [Step] → verify: [check]
3. [Step] → verify: [check]
```

Strong success criteria let you loop independently. Weak criteria ("make it work") require constant clarification.

## Unconfirmed Requirement Protocol

If a requirement, behavior rule, data model, file ownership boundary, UI hierarchy, naming convention, migration direction, or runtime/editor responsibility is not clearly defined, do not guess and do not implement the uncertain part.

Instead:

1. Stop before modifying code related to the uncertain area.
2. Summarize the ambiguity briefly.
3. Ask the user focused questions with 2-3 concrete options when possible.
4. State the recommended option and why.
5. Continue implementation only after the user confirms the direction.

Assumptions must be explicitly labeled as assumptions. Do not silently turn assumptions into code, serialized data, prefab hierarchy changes, or migration logic.

---

**These guidelines are working if:** fewer unnecessary changes in diffs, fewer rewrites due to overcomplication, and clarifying questions come before implementation rather than after mistakes.

## Language Rule

All explanations, plans, summaries, implementation notes, and questions written to the user must be in Korean.

Code identifiers, class names, method names, enum names, file names, folder names, Unity API names, and package names stay in English.

When asking the user for confirmation, ask in Korean.

## SOLID First

Always prioritize SOLID principles when designing and implementing code.

- Keep each class focused on a single responsibility.
- Do not make one manager class handle unrelated systems.
- Separate data, battle logic, UI logic, input logic, and presentation/animation logic.
- Prefer clear dependencies over hidden global access.
- Design features so they can be extended later without rewriting existing code.
- Avoid over-engineering, but do not create temporary structures that block future expansion.

This is a 3-week prototype, so implementation speed matters. However, fast implementation must not mean mixing responsibilities or creating hard-to-replace systems.

### Use grill-me Before Implementation

Before implementing a new feature or changing an existing structure, use the grill-me skill to confirm the design first. Check:

- Which class or module should own this responsibility?
- Does this feature belong to the current phase?
- Does it conflict with the existing FSM or module system?
- Can it later support fusion, card grades, resonance gauge, rewards, and stage expansion?
- Is the implementation data-driven where appropriate?
- Are ScriptableObjects used for editable game data?
- Are UI, gameplay logic, and animation/presentation logic kept separate?

If the structure is unclear, do not guess. Ask the user in Korean before implementing.

## Phase Implementation

- When told to read a separate md file, implement the phases listed in that file in order.
- Never implement multiple phases at once. Implement ONE phase, then wait until the user says to start the next.
- Before implementing, always use the grill-me skill to confirm the structure first.
- When a phase is done and tested and the user confirms completion, mark it done in the md file and summarize or remove phases no longer needed.
- When all phases are complete, write a completion report describing how it was implemented and verified.

## Dependency Injection (Reflect)

Use Reflect DI for all dependency injection.

- Do not create service dependencies directly with `new`.
- Do not access services via singletons or static classes from MonoBehaviour.
- Use constructor injection or Reflect's injection.
- Do not resolve dependencies manually outside the DI container.

## Async / Await (UniTask)

Use UniTask for all async work. Do not write new coroutines.

- Do not write new `Coroutine` / `StartCoroutine` / `IEnumerator`.
- Use `UniTask` / `async UniTask` instead of `Task` / `async Task`.
- Frame wait: `await UniTask.Yield()` or `await UniTask.NextFrame()`
- Time wait: `await UniTask.Delay(TimeSpan)` or `await UniTask.WaitForSeconds(seconds)`
- Handle cancellation with `CancellationToken`; signal completion with `UniTaskCompletionSource`.
- Await LitMotion tweens via `.ToUniTask(cancellationToken)`.

## Tweening (LitMotion)

Use LitMotion for all tweening and value interpolation.

- Do not use DOTween, iTween, or other tweening libraries.
- Base pattern: `LMotion.Create(from, to, duration).Bind(target)`
- For sequences, combine with UniTask: `.ToUniTask()`
- Unify UI animation, camera movement, and value interpolation on LitMotion.

## Encoding

- Save Markdown, C# source, and Unity text assets as UTF-8.
- When using PowerShell to read files, prefer `Get-Content -Encoding UTF8`.
- When a tool must write text directly, specify UTF-8 explicitly and verify Korean text did not become mojibake.
- Prefer patch-based edits for shared docs and source files so Claude/Codex do not disagree on encoding.

## Event Communication (EventChannelSO)

Use `EventChannelSO` channeling for object-to-object event communication.

- Do not reference objects directly via C# `event Action` or `UnityEvent`.
- Define event data as classes inheriting `GameEvent`.
- Subscribe: `channel.AddListener<MyEvent>(OnMyEvent)`
- Unsubscribe: `channel.RemoveListener<MyEvent>(OnMyEvent)` — always in OnDisable or OnDestroy.
- Raise: `channel.RaiseEvent(new MyEvent(...))`
- Inject the `EventChannelSO` asset via Inspector (DI or SerializeField).

## Folder & File Ownership (Team)

Each member has a separate workspace. Follow these location rules when creating or editing files.

- Create code only inside `Assets/MemberWorkspace/[username]/`.
  - Never modify code in another member's `MemberWorkspace` folder.
- Write the user's personal work log to `Docs/Member/[username]/SEQUENCE.md`.
  - Each entry records the prompt, changed files, changes, and WHY.
- Shared docs (`Docs/README.md`, `Docs/DESIGN.md`, `Docs/TODO.md`): any member may edit.
  - In `Docs/TODO.md`, each member owns a `## [username]` section. Edit only the current user's own section; `## Common` may be edited by anyone.
  - `Docs/README.md` and `Docs/DESIGN.md` change less often. Keep edits small, and summarize what changed and why to the user so they can share it with the team.

If you don't know the username when work starts, ask the user before creating code or personal docs.

## Debugging (Token Saving)

When using Unity MCP, debug via the Console log only.

- Do not start with play tests (running the game directly), which use many tokens.
- Read error messages and `Debug.Log` output from the Console to find issues.
- Only suggest a play test to the user when the Console log alone can't find the cause.

## How to work in this repo

- Read the relevant rule files before making large edits.
- Prefer small, reviewable changes over broad refactors.
- When a change affects architecture, explain the reason and the touched systems first.
