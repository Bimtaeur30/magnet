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

## Language Rulef

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

## Implementation / Phase / Sequence Workflow

개인 작업은 **구현 → Phase → Sequence** 3단계로 기록한다. `Docs/DESIGN.md` **마일스톤(M0~M10)** 은 팀 전체 로드맵이며, 개인 Phase와 **다른 개념**이다.

| 용어 | 범위 | 문서 |
|------|------|------|
| **마일스톤** | 게임 전체 기능 영역 (M0~M10) | `Docs/DESIGN.md` §8 |
| **구현** | 개인 기능·Jira 이슈·요청 단위 (예: 인벤토리) | `IMPLEMENTATIONS.md` |
| **Phase** | 그 구현을 쪼갠 단계. **뭘 어떻게 구현하는지 자세히** 적는다 (예: 구조 → 핵심 기능 → 부가 기능) | `Implementations/[slug]/phaseN.md` |
| **Sequence** | 그 Phase에서 **뭐가 바뀌었는지** 순서대로 적는 변경 기록. **Phase 파일과 1:1** | `Implementations/[slug]/sequenceN.md` |

**코드·에셋·씬을 건드리기 전에** 아래 순서를 반드시 따른다.

### 워크플로 (필수)

1. **계획 제시 (구현 전)** — 사용자에게 **한국어**로 다음을 먼저 전달한다.
   - 대상 **구현**·**Phase** 목표·완료 기준 (`phaseN.md`에 들어갈 「뭘 어떻게」)
   - 생성·수정할 파일·폴더 목록
   - 클래스 책임·구조 (grill-me로 검토한 내용 요약)
   - 검증 방법 (컴파일 에러 확인 등)
2. **사용자 승인 대기** — 사용자가 명시적으로 허락할 때까지 **구현하지 않는다**.
   - 승인 예: 「진행해」, 「허락」, 「시작」, 「OK」
   - 「Phase N 시작」만으로는 계획 승인이 아니다. 계획을 보여준 뒤 별도 승인을 받는다.
3. **구현** — 승인 후 **한 Phase만** 구현한다. 여러 Phase를 한 번에 하지 않는다.
4. **기록** — 계획·구조는 `phaseN.md`에, 실제로 바뀐 것은 짝이 되는 `sequenceN.md`에 `## N — 날짜 · 제목` 항목으로 **추가** → `phases.md` → `IMPLEMENTATIONS.md` 순으로 인덱스를 맞춘다.
5. **완료 처리** — 사용자가 결과를 확인하고 완료를 알려준 뒤에만 `DESIGN.md` 마일스톤 / `TODO.md` 상태를 `✅`로 갱신한다.

### 추가 규칙

- 새 **구현** 시작 전: 기능을 Phase로 쪼개는 계획을 먼저 제시한다 (코드 X). 예: 인벤토리 → 1) 구조 2) 아이템 넣기 3) 부가 기능(단축키·드래그).
- 별도 md 파일을 읽으라고 지시받으면, 그 구현의 Phase 순서대로 진행한다.
- 구조가 불명확하면 grill-me로 검토하고, 여전히 불명확하면 한국어로 질문한다. 추측 구현 금지.
- 한 **구현**의 모든 Phase·Sequence가 끝나면 완료 보고서를 작성한다.

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

### 예외 — 직렬화가 불가능할 때만 Reflex 허용

**SO가 아닌** 것 중, Inspector `[SerializeField]`로 연결할 수 없을 때만 Reflex `[Inject]`를 쓴다.

- **인터페이스 계약** (`IBlockShapeSource` 등) — 구체 구현이 다른 asmdef·다른 멤버 Workspace에 있을 때

**SO는 예외 없음:** 모든 `ScriptableObject`는 소비 `MonoBehaviour`에 `[SerializeField]`로 연결. `[Inject]`·Installer `RegisterValue`로 SO를 넘기지 않는다.

패턴: **SO = `[SerializeField]`** · **크로스-asmdef 계약 = Installer `RegisterValue` + `[Inject]` 계약 타입**

### 공통

- `new`로 서비스 직접 생성 금지 (순수 Domain 데이터 클래스 제외)
- 싱글톤 / static 서비스 접근 금지
- 컨테이너 밖에서 수동 `Resolve` 금지

**예:** `BoardView` → `[SerializeField] BoardConfigSO boardConfig` · `Phase0Bootstrap` → `[SerializeField] EventChannelSO magnetGameChannel` · `BlockSpawnBootstrap` → `[Inject] IBlockShapeSource` (인터페이스, 직렬화 불가)

### Inspector Tooltip

이름만으로 역할이 불명확한 `[SerializeField]`에는 `[Tooltip("…")]`(한국어)을 붙인다. 추가·수정 시 `Docs/INSPECTOR_TOOLTIPS.md` 표를 갱신한다.

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

## Defensive Checks / Assertions

방어 코드는 **정말 필요할 때만** 넣는다. 남발하지 않는다.

- "일어나면 안 되는" 개발자 설정 오류(예: `[SerializeField]` 미할당, 잘못된 사용)는 `if (x == null) { Debug.LogError(...); return; }` 대신 **`Debug.Assert`** 를 쓴다.
  - 예: `Debug.Assert(config != null, "[BoardView] BoardConfigSO is not assigned.", this);`
  - 이유: `Debug.Assert`는 에디터/개발 빌드에서 크게 실패하고 릴리스 빌드에서 제거된다. 정상 흐름을 방해하지 않고 코드가 짧아진다.
- 다음 경우에만 명시적 처리(early return, 예외, 로그)를 쓴다.
  - 런타임에 **실제로 발생 가능한** 입력(외부 데이터, 네트워크, 사용자 입력).
  - 실패 시 **복구·대체 동작**이 필요한 경우.
- 불가능한 시나리오에 대한 방어 코드는 만들지 않는다 (`CLAUDE.md` §2 Simplicity First와 동일 취지).

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
- 메인 채널: `[SerializeField] EventChannelSO magnetGameChannel` — **모든 SO와 동일, `[Inject]` 금지** (`CLAUDE.md` DI · `jth-event-channel.mdc`)

## Folder & File Ownership (Team)

Each member has a separate workspace. Follow these location rules when creating or editing files.

- Create code only inside `Assets/_MemberWorkspace/[username]/`.
  - Never modify code in another member's `_MemberWorkspace` folder.
- Write the user's personal work log under `Assets/_MemberWorkspace/[username]/Docs/`.
  - **구현 인덱스:** `IMPLEMENTATIONS.md`
  - **구현별 Phase 인덱스:** `Implementations/[slug]/phases.md`
  - **Phase 계획 (뭘 어떻게):** `Implementations/[slug]/phaseN.md` — 목표(완료 기준) · 구현 내용(클래스·책임·방식 상세) · 범위 밖 · 코드·에셋 맵
  - **Sequence 변경 기록:** `Implementations/[slug]/sequenceN.md` — **Phase와 1:1** 파일. `## N — 날짜 · 제목` 항목을 순서대로 추가 (항목마다 파일 분리 X). 각 항목: 바뀐 것(생성/수정/삭제 파일) · 메모.
  - New sessions (AI): read `IMPLEMENTATIONS.md` + **current** 구현의 `phases.md` + **current** `phaseN.md`·`sequenceN.md` only. Paths are under `Assets/` so Unity Project 창에서도 열 수 있음.
- Shared docs (`Docs/README.md`, `Docs/DESIGN.md`, `Docs/TODO.md`): any member may edit.
  - In `Docs/TODO.md`, each member owns a `## [username]` section. Edit only the current user's own section; `## Common` may be edited by anyone.
  - `Docs/README.md` and `Docs/DESIGN.md` change less often. Keep edits small, and summarize what changed and why to the user so they can share it with the team.

If you don't know the username when work starts, ask the user before creating code or personal docs.

## Document Locations (AI sessions)

| What | Path |
|------|------|
| Team design & milestone table | `Docs/DESIGN.md` |
| Team TODO | `Docs/TODO.md` |
| AI prompt guide | `Docs/AI_COLLAB_GUIDE.md` |
| This rule file | `CLAUDE.md` |
| **Member implementation index** | `Assets/_MemberWorkspace/[username]/Docs/IMPLEMENTATIONS.md` |
| **Phase index (per implementation)** | `Assets/_MemberWorkspace/[username]/Docs/Implementations/[slug]/phases.md` |
| **Phase plan (what & how)** | `Assets/_MemberWorkspace/[username]/Docs/Implementations/[slug]/phaseN.md` |
| **Sequence change log (1:1 with Phase)** | `Assets/_MemberWorkspace/[username]/Docs/Implementations/[slug]/sequenceN.md` |

On a new session: read `IMPLEMENTATIONS.md` + **current** `phases.md` + **current** `phaseN.md`·`sequenceN.md` only (not full history). Personal docs live under `Assets/` so they appear in Unity Project window.

## Debugging (Token Saving)

Unity MCP 디버깅은 **콘솔 로그 중심**으로 한다. **플레이 자체가 금지는 아니다.**

- 평소에는 `read_console`로 **컴파일 에러·경고**만 봐도 충분하다.
- **런타임 확인이 필요하다고 판단되면** Play 진입해도 된다. 이때 **`read_console`로 로그만** 확인하고, Game/Scene 뷰·스크린샷·하ierarchy 대량 조회 같은 **시각적 디버깅은 하지 않는다** (토큰 절약).
- 사용자가 이미 Play한 뒤 남은 콘솔만 읽어도 된다.

### 금지 (토큰 많이 소모)

- Game 뷰·Scene 뷰·스크린샷·하ierarchy/컴포넌트 MCP 등 **화면을 보고** 런타임을 확인하는 것.

### 예외

- 사용자가 명시적으로 「화면 봐서 확인해」, 「Game 뷰로 봐줘」처럼 **시각 확인**을 요청한 경우.

## How to work in this repo

- Read the relevant rule files before making large edits.
- Prefer small, reviewable changes over broad refactors.
- When a change affects architecture, explain the reason and the touched systems first.
