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

`Docs/DESIGN.md`의 Phase 표를 기준으로 진행한다. **코드·에셋·씬을 건드리기 전에** 아래 순서를 반드시 따른다.

### Phase 워크플로 (필수)

1. **계획 제시 (구현 전)** — 사용자에게 **한국어**로 다음을 먼저 전달한다.
   - 이번 Phase 목표·완료 기준 (`DESIGN.md` 기준)
   - 생성·수정할 파일·폴더 목록
   - 클래스 책임·구조 (grill-me로 검토한 내용 요약)
   - 검증 방법 (컴파일 에러 확인 등)
2. **사용자 승인 대기** — 사용자가 명시적으로 허락할 때까지 **구현하지 않는다**.
   - 승인 예: 「진행해」, 「허락」, 「시작」, 「OK」
   - 「Phase N 시작」만으로는 계획 승인이 아니다. 계획을 보여준 뒤 별도 승인을 받는다.
3. **구현** — 승인 후 **한 Phase만** 구현한다. 여러 Phase를 한 번에 하지 않는다.
4. **기록** — Phase 작업이 끝나면 `Assets/MemberWorkspace/[username]/Docs/Sequence/phaseN.md`를 갱신하고, `Assets/MemberWorkspace/[username]/Docs/SEQUENCE.md` 인덱스 상태를 맞춘다.
5. **완료 처리** — 사용자가 결과를 확인하고 완료를 알려준 뒤에만 `DESIGN.md` / `TODO.md`의 Phase 상태를 `✅`로 갱신한다.

### 추가 규칙

- 별도 md 파일을 읽으라고 지시받으면, 그 파일의 Phase 순서대로 진행한다.
- 구조가 불명확하면 grill-me로 검토하고, 여전히 불명확하면 한국어로 질문한다. 추측 구현 금지.
- 모든 Phase가 끝나면 완료 보고서를 작성한다.

## Dependency Injection (Reflex)

런타임 **씬 오브젝트 간** 참조가 필요할 때만 Reflex를 쓴다.

### Reflex 사용 (O)

- 씬에 있는 다른 `MonoBehaviour` / 런타임 서비스를 주입할 때
- 프리팹 인스턴스끼리 씬에서 연결하기 어려운 **동적 의존성**
- `[Inject]` 또는 Installer `RegisterValue` — 대상이 **씬·런타임 객체**일 때

### Reflex 사용 안 함 (X) — Inspector `SerializeField`

- **ScriptableObject** (`BoardConfigSO`, `BlockShapeSO`, `EventChannelSO` 등 **에셋**)
- 프리팹에 넣어도 참조가 유지되는 **에셋·프리팹 참조**
- 이유: SO/에셋은 프리팹 분리·씬 저장 시 SerializeField로 충분하고, DI 컨테이너에 넣을 필요 없음

### 공통

- `new`로 서비스 직접 생성 금지 (순수 Domain 데이터 클래스 제외)
- 싱글톤 / static 서비스 접근 금지
- 컨테이너 밖에서 수동 `Resolve` 금지

**예 (Phase 1+):** `BoardView` → `[SerializeField] BoardConfigSO` · 턴 오케스트레이터 → `[Inject] EventChannelSO`는 **에셋이면 SerializeField**, 씬 매니저 참조만 `[Inject]`

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
- Raise: `channel.RaiseEvent(GameEvents.MyEvent.Init(...))` — `GameEvents` static 인스턴스 + `Init()`, `new` 금지
- **에셋(`EventChannelSO`)은 `[SerializeField]`로 연결.** Reflex 주입하지 않음 (`CLAUDE.md` DI 규칙).

## Folder & File Ownership (Team)

Each member has a separate workspace. Follow these location rules when creating or editing files.

- Create code only inside `Assets/MemberWorkspace/[username]/`.
  - Never modify code in another member's `MemberWorkspace` folder.
- Write the user's personal work log under `Assets/MemberWorkspace/[username]/Docs/`.
  - **One file per Phase:** `Sequence/phase0.md`, `phase1.md`, … (`Docs/DESIGN.md` Phase 번호와 맞춤).
  - Each file answers: **what was done in this Phase** → **which code paths to read** (표로 경로 정리).
  - Sections: 목표 · 한 일(포함/제외) · 코드·에셋 맵 · 메모(함정·비자명한 결정만).
  - Maintain `Assets/MemberWorkspace/[username]/Docs/SEQUENCE.md` as a **Phase index** with status.
  - New sessions (AI): read `SEQUENCE.md` + **current** `Sequence/phaseN.md` only. Paths are under `Assets/` so Unity Project 창에서도 열 수 있음.
- Shared docs (`Docs/README.md`, `Docs/DESIGN.md`, `Docs/TODO.md`): any member may edit.
  - In `Docs/TODO.md`, each member owns a `## [username]` section. Edit only the current user's own section; `## Common` may be edited by anyone.
  - `Docs/README.md` and `Docs/DESIGN.md` change less often. Keep edits small, and summarize what changed and why to the user so they can share it with the team.

If you don't know the username when work starts, ask the user before creating code or personal docs.

## Document Locations (AI sessions)

| What | Path |
|------|------|
| Team design & Phase table | `Docs/DESIGN.md` |
| Team TODO | `Docs/TODO.md` |
| AI prompt guide | `Docs/AI_COLLAB_GUIDE.md` |
| This rule file | `CLAUDE.md` |
| **Member Phase index** | `Assets/MemberWorkspace/[username]/Docs/SEQUENCE.md` |
| **Member Phase log** | `Assets/MemberWorkspace/[username]/Docs/Sequence/phaseN.md` |

On a new session: read `SEQUENCE.md` + **current** `phaseN.md` only (not full history). Personal docs live under `Assets/` so they appear in Unity Project window.

## Debugging (Token Saving)

Unity MCP로 디버깅할 때 **콘솔 로그만** 사용한다. 플레이 모드는 토큰을 많이 소모한다.

### 금지

- `manage_editor`의 **play / pause / stop**으로 에이전트가 직접 플레이 모드에 진입하는 것.
- 런타임 검증을 핑계로 에이전트가 임의로 플레이 테스트를 실행하는 것.

### 허용·권장

- `read_console`로 **컴파일 에러·경고**를 확인한다 (에디터가 스크립트를 리컴파일한 뒤의 콘솔).
- 이미 사용자가 플레이한 뒤 남은 `Debug.Log` 출력이 있으면 그것만 읽는다.
- 런타임 동작 확인이 필요하면 **사용자에게 플레이를 요청**하고, 사용자가 플레이한 뒤 콘솔 결과를 읽는다.

### 예외

- 사용자가 명시적으로 「플레이해서 확인해」, 「Play 모드로 테스트해」라고 요청한 경우에만 에이전트가 플레이 모드에 진입할 수 있다.

## How to work in this repo

- Read the relevant rule files before making large edits.
- Prefer small, reviewable changes over broad refactors.
- When a change affects architecture, explain the reason and the touched systems first.
