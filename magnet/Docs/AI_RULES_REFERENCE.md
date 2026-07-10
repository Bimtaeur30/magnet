# AI Rules Reference (상세)

`CLAUDE.md`에서 빼 둔 **배경·상세 규칙**. 매 세션 자동 로드 대상 아님. 필요할 때 `@Docs/AI_RULES_REFERENCE.md`.

---

## Behavioral guidelines (generic)

### Think Before Coding

- 가정을 명시. 불확실하면 질문.
- 해석이 여러 개면 선택지 제시.
- 더 단순한 방법이 있으면 말한다.

### Simplicity First

- 요청 밖 기능·추상화·과잉 방어 코드 금지.
- 200줄이 50줄로 될 수 있으면 줄인다.

### Surgical Changes

- 인접 코드 리팩터·포맷 변경 금지. 기존 스타일 유지.
- 내 변경으로 생긴 unused import/변수만 정리.

### Goal-Driven Execution

- 완료 기준을 먼저 정하고 검증한다.
- 다단계 작업은 `1. [Step] → verify: [check]` 형식 계획.

### Unconfirmed Requirement Protocol

요구·데이터 모델·UI 계층·책임 경계가 불명확하면:

1. 해당 영역 코드 수정 중단
2. ambiguity 요약 + 2~3 옵션 질문 (권장안 포함)
3. 사용자 확인 후 구현

---

## SOLID (요약)

- 클래스 단일 책임. data / logic / UI / presentation 분리.
- 3주 프로토타입 — 속도와 교체 가능한 구조 균형.

### grill-me 체크리스트

- 책임 소유 클래스는?
- 현재 Phase 범위인가?
- FSM·모듈과 충돌?
- ScriptableObject로 데이터 분리?
- UI·게임플레이·연출 분리?

---

## Phase / Sequence (상세)

| 용어 | 문서 |
|------|------|
| 마일스톤 M0~M10 | `Docs/DESIGN.md` |
| 구현 | `IMPLEMENTATIONS.md` |
| Phase 계획 | `Implementations/[slug]/phaseN.md` |
| Sequence 기록 | `Implementations/[slug]/sequenceN.md` (Phase와 1:1) |

승인 예: `진행해`, `허락`, `시작`, `OK` — `Phase N 시작`만으로는 승인 아님.

---

## DI (Reflex) 상세

**`[SerializeField]` (프리팹·에셋 분리해도 유지)**

- 모든 ScriptableObject (`BoardConfigSO`, `EventChannelSO`, …)
- 프로젝트 에셋 참조 (프리팹·Material 등) — 예: `ShapeBlock` 프리팹 필드

**`[Inject]` 필수 (SerializeField 금지)**

- **씬의 다른 GameObject / MonoBehaviour** — `BoardPlacementBootstrap`, `BlockSpawnBootstrap` …
- 직렬화 불가 **인터페이스** (크로스-asmdef) — `IBlockShapeSource`

**이유:** GO를 프리팹으로 나누면 씬 오브젝트 SerializeField가 끊긴다. Installer `RegisterValue` + 소비자 `[Inject]`만.

**Installer:** `MagnetSceneInstaller` 등에서 씬 서비스를 SerializeField로 **한 번** 잡고 `RegisterValue`. 소비자 Inspector에 씬 GO 드래그 **금지**.

**예:** `BoardView` → `[SerializeField] BoardConfigSO` · `BlockDragInput` → `[Inject] BoardPlacementBootstrap` · `BlockSpawnBootstrap` → `[Inject] IBlockShapeSource`

규칙 파일: `.cursor/rules/jth-reflex-di.mdc`

---

## UniTask / LitMotion

- Frame: `UniTask.Yield()` / `NextFrame()`
- Time: `UniTask.Delay` / `WaitForSeconds`
- Cancellation: `CancellationToken`, `UniTaskCompletionSource`
- LitMotion: `LMotion.Create(from, to, duration).Bind(target)` · `.ToUniTask()`

---

## Assert 정책 (상세)

- Inspector 미할당 등 **개발자 설정 오류**: `Debug.Assert` (릴리스에서 제거)
- **런타임 입력·복구 필요**: early return / 로그
- 불가능 시나리오 방어 코드 금지

---

## Encoding

- Markdown·C#·Unity 텍스트: UTF-8
- PowerShell: `Get-Content -Encoding UTF8`
- patch 기반 편집 선호 (한글 깨짐 방지)

---

## Document paths

| What | Path |
|------|------|
| Team design | `Docs/DESIGN.md` |
| Team TODO | `Docs/TODO.md` |
| AI prompt guide | `Docs/AI_COLLAB_GUIDE.md` |
| Member index | `Assets/_MemberWorkspace/[username]/Docs/IMPLEMENTATIONS.md` |
| Phase index | `.../Implementations/[slug]/phases.md` |
| Phase plan | `.../Implementations/[slug]/phaseN.md` |
| Sequence log | `.../Implementations/[slug]/sequenceN.md` |

---

## Debugging (상세)

- Play OK — `read_console`로 로그만
- 금지: Game/Scene 뷰, 스크린샷, hierarchy/컴포넌트 MCP 대량 조회
- 예외: 사용자가 시각 확인 명시 요청
