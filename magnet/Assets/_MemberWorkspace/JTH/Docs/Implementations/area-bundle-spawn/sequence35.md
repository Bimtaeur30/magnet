# sequence35 — Phase 35 변경 기록

> Phase 계획: [phase35.md](phase35.md)

## 1 — 2026-08-11 · CornerRect Stage 에이전트 전환

**바뀐 것** — Stage 1~4(0.5/1/2/4)를 에이전트가 풀 에셋에 직접 넣고, Inspector ContextMenu는 쓰지 않는다.

**변경 상세 (왜/무엇)**
- 파일: `ScriptableObjects/AreaBundleSpawn/DefaultAreaBundlePool.asset`
  - 심볼: `areaScore.cornerRectPenalty` — 직렬화 (수정, **0.5** = Stage1)
    - 설명: 평가 시작 값을 Stage1로 둔다.
    - 이유: 사용자가 1→2→3→4 순으로 플레이하며 비교하기 위해.
- 파일: `Scripts/Data/AreaScoreTuning.cs`
  - 심볼: `AreaScoreTuning.cornerRectPenalty` — 필드 기본값 (수정, 0.5)
    - 설명: 코드 기본도 Stage1과 맞춘다.
    - 이유: 새 에셋/리셋 시 Stage1과 어긋나지 않게.
- 파일: `Scripts/Data/AreaBundlePoolSO.cs`
  - 심볼: `ApplyCornerRectStage1`~`4` / `ApplyCornerRectStage` — 메서드 (삭제)
    - 설명: Tuning ContextMenu 경로를 제거한다.
    - 이유: 전환은 에이전트가 에셋을 수정하는 방식만 쓴다.
- 파일: `Docs/Implementations/area-bundle-spawn/TUNING_STAGES.md`
  - 심볼: `CornerRect Round (평가 중)` 섹션 (수정)
    - 설명: Stage 1~4 표 + “에이전트에게 숫자로 말해 전환” 안내.
    - 이유: 평가 기록·전환 규칙을 문서에 고정.
