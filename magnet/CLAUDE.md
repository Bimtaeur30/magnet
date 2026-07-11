# CLAUDE.md

프로젝트 AI용 **핵심 규칙 (compact)**. 상세·배경은 `Docs/AI_RULES_REFERENCE.md`.

## Language

- 사용자-facing 설명·질문: **한국어**
- 코드 식별자·Unity API: **English**

## 코딩 (요약)

- 요청 범위만 수정. 추측 구현 금지 → 불명확하면 한국어로 질문.
- 새 기능·구조 변경 전 **grill-me** 스킬로 설계 확인.
- `Debug.Assert`: **Awake/Start에서 즉시 쓰는 SerializeField SO**만. 메서드마다 Assert X.

## Workspace

- 코드: `Assets/_MemberWorkspace/[username]/` 만 (남의 Workspace 수정 금지)
- 개인 문서: `Assets/_MemberWorkspace/[username]/Docs/`
- 공용: `Docs/DESIGN.md`, `Docs/TODO.md` (본인 `## [username]` 섹션만)

## Phase / Sequence (새 구현·Phase 작업 시)

1. 한국어 계획 → 사용자 승인(`진행해`/`OK` 등) → **Phase 하나**만 구현
2. 기록: `sequenceN.md` 항목 추가 → `phases.md` → `IMPLEMENTATIONS.md`
3. 새 세션: `IMPLEMENTATIONS.md` + 현재 `phases.md` + `phaseN.md` + `sequenceN.md` **만** 읽기 (전체 히스토리 X)
4. 규칙·마일스톤 변경 시 `DESIGN.md` / `TODO.md` 동기화

## DI (Reflex)

- **SO·프로젝트 에셋**(프리팹 SO 등) → `[SerializeField]` (**`[Inject]`·RegisterValue 금지**)
- **씬 MonoBehaviour·다른 GO 컴포넌트** → **`[Inject]` 필수** (**SerializeField 금지** — 프리팹 분리 시 참조 끊김)
- **직렬화 불가 인터페이스** → `[Inject]` (예: `IBlockShapeSource`)
- Installer에서 씬 서비스 `RegisterValue` → 소비자 `[Inject]`
- 금지: singleton/static service, 컨테이너 밖 `Resolve`, Domain 순수 클래스 외 `new` 서비스
- 메인 채널 필드명: `magnetGameChannel`
- 상세: `jth-reflex-di.mdc` · 감사 기록: `Assets/_MemberWorkspace/JTH/Docs/DI_FIELD_AUDIT.md`

## Events (EventChannelSO)

- 객체 간 통신: `EventChannelSO`만 (C# `event`/`UnityEvent` X)
- `channel.AddListener` / `RemoveListener` (OnDisable)
- `channel.RaiseEvent(GameEvents.X.Init(...))` — `new` 금지

## Stack

- Async: **UniTask** (새 Coroutine 금지)
- Tween: **LitMotion** (DOTween 등 금지)
- Tooltip 추가 시 `Docs/INSPECTOR_TOOLTIPS.md` 갱신

## Unity 디버깅 (토큰)

- 기본: `read_console` (컴파일·로그)
- Game/Scene 뷰·스크린샷·hierarchy 대량 조회 **금지** — 사용자가 시각 확인 요청할 때만

## glob 규칙 (해당 파일 편집 시 자동)

- `jth-reflex-di.mdc` — SerializeField vs `[Inject]` (씬 GO는 Inject 필수)
- `jth-csharp.mdc` — SerializeField `_` 금지 등
- `jth-event-channel.mdc` — SO Inject 금지, `magnetGameChannel`
- `jth-sequence-log.mdc` — sequence 심볼 단위 기록
- `jth-tests.mdc` — JTH 영구 테스트 없음
