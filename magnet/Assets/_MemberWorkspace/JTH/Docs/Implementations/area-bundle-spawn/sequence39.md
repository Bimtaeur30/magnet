# sequence39 — Phase 39 변경 기록

> Phase 계획: [phase39.md](phase39.md)

## 1 — 2026-08-13 · Unique 4칸 균형 + 손 최적 배치 기즈모

**바뀐 것** — Unique shape 가중을 4칸 중심으로 재조정하고, Scene 기즈모에 현재 손의 MaxArea 최적 배치 오버레이를 추가한다.

**변경 상세 (왜/무엇)**
- 파일: `Scripts/Data/AreaBundlePoolSO.cs`
  - 심볼: `AreaBundlePoolSO.DefaultUniqueShapeWeight` — 메서드 (수정)
    - 설명: 1~2칸·I4를 낮추고 T/S/Z·L4를 높게, 5칸+는 ≈0으로 둔다. 작은 ㄱ은 중간(5.5).
    - 이유: 소형만 쓰면 해가 많고, 대형만 쓰면 자리가 뻔해 Unique가 쉬워짐.
- 파일: `ScriptableObjects/AreaBundleSpawn/DefaultAreaBundlePool.asset`
  - 심볼: `uniqueShapeWeights` — 직렬화 배열 (수정)
    - 설명: `DefaultUniqueShapeWeight`와 동일 값으로 동기화(합 ≈ tiny7 / mid3 33 / tet4 161 / large 2).
    - 이유: Ensure는 길이만 맞으면 기본값을 다시 안 넣으므로 에셋을 직접 맞춤.
- 파일: `Scripts/Presentation/AreaBundleSelectionGizmo.cs`
  - 심볼: `AreaBundleSelectionGizmo.areaBundlePool` — SerializeField (추가)
    - 설명: MaxArea 캡·AreaScore 튜닝을 풀 SO에서 읽는다.
    - 이유: 손 최적 Explain이 스폰과 같은 점수/캡을 쓰게.
  - 심볼: `AreaBundleSelectionGizmo.drawLiveHandBest` / `liveHandFillScale` / `liveHandFillAlpha` — SerializeField (추가)
    - 설명: 손 최적 오버레이 on/off·채움 큐브 크기·알파.
    - 이유: 지급 Explain(#)과 시각 구분·튜닝.
  - 심볼: `AreaBundleSelectionGizmo._liveHandCacheKey` / `_liveHandSteps` / `_liveHandSlotMap` — 필드 (추가)
    - 설명: 보드·손 해시 캐시와 스텝·공급 슬롯 매핑.
    - 이유: OnDrawGizmos마다 DFS하지 않도록.
  - 심볼: `AreaBundleSelectionGizmo.DrawLiveHandBest` — 메서드 (추가)
    - 설명: 남은 Candidates로 MaxArea 경로를 그려 `H1…` 라벨·채움 큐브를 표시한다.
    - 이유: 지급 시점 Explain과 별개로, 지금 들고 있는 블럭의 최적 자리를 보여 줌.
  - 심볼: `AreaBundleSelectionGizmo.RefreshLiveHandCache` — 메서드 (추가)
    - 설명: 캐시 키가 바뀌면 `TryGetBestSequenceExplain`으로 스텝을 재계산한다.
    - 이유: 보드/손 변경 시에만 비싼 탐색.
  - 심볼: `AreaBundleSelectionGizmo.BuildLiveHandCacheKey` — 메서드 (추가)
    - 설명: 점유 격자 + 후보 offsets로 정수 해시를 만든다.
    - 이유: 캐시 무효화 키.
  - 심볼: `AreaBundleSelectionGizmo.ResolveLiveHandColor` — 메서드 (추가)
    - 설명: 공급 슬롯 기준 무지개색·live 알파를 돌려준다.
    - 이유: Unique blocked 빨강과 섞지 않고 슬롯 색으로 구분.
  - 심볼: `AreaBundleSelectionGizmo.OnDrawGizmos` — 메서드 (수정)
    - 설명: Area 다음·selection 여부와 무관하게 `DrawLiveHandBest`를 호출한다. 기존 Explain은 그대로.
    - 이유: 손 최적은 LastSelection 없이도 필요하고, 지급 Explain은 유지.
- 파일: `Prefabs/Debug/AreaBundleSelectionGizmo.prefab`
  - 심볼: `areaBundlePool` — 직렬화 참조 (추가/할당)
    - 설명: `DefaultAreaBundlePool`을 연결한다.
    - 이유: Play 중 live Explain이 풀 튜닝을 쓰게.

## 2 — 2026-08-13 · 손 최적 기즈모 제거 · HandCompare 디버그

**바뀐 것** — live hand 기즈모를 제거하고, 손 3개를 모두 둔 뒤에만 추천 Explain 대비 플레이어 수 비교 로그를 남긴다. 기타 AreaBundle 콘솔 로그는 끈다.

**변경 상세 (왜/무엇)**
- 파일: `Scripts/Presentation/AreaBundleSelectionGizmo.cs`
  - 심볼: `DrawLiveHandBest` / `RefreshLiveHandCache` / `BuildLiveHandCacheKey` / `ResolveLiveHandColor` — 메서드 (삭제)
    - 설명: 현재 손 MaxArea 오버레이 그리기를 제거한다.
    - 이유: 요청 — 방금 추가한 기즈모 삭제.
  - 심볼: `areaBundlePool` / `drawLiveHandBest` / `liveHandFillScale` / `liveHandFillAlpha` / `_liveHand*` — 필드 (삭제)
    - 설명: live hand 전용 직렬화·캐시 필드를 제거한다.
    - 이유: 미사용 참조 정리.
- 파일: `Scripts/Bootstrap/BlockSpawnBootstrap.cs`
  - 심볼: `BlockSpawnBootstrap.RecordPlayerMove` — 메서드 (추가)
    - 설명: 슬롯·절대 칸을 기록하고 `lastDrop`이면 `LogHandCompare` 후 목록을 비운다.
    - 이유: 3수 완료 시점에만 추천과 비교.
  - 심볼: `BlockSpawnBootstrap.LogHandCompare` — 메서드 (추가)
    - 설명: ExplainSteps와 플레이어 수를 순서·슬롯·칸집합으로 비교해 `[AreaBundle:HandCompare]` 한 줄을 찍는다.
    - 이유: 추천(#) vs 실제 수 검증용 유일 디버그.
  - 심볼: `BlockSpawnBootstrap.PlayerHandMove` — nested readonly struct (추가)
    - 설명: 슬롯 인덱스와 칸 배열을 담는다.
    - 이유: 손 기록 단위.
  - 심볼: `BlockSpawnBootstrap.LogSelection` — 메서드 (삭제)
    - 설명: 패 지급 시 tier/reason 로그를 제거한다.
    - 이유: HandCompare만 남기기.
  - 심볼: `BlockSpawnBootstrap.Fill` — 메서드 (수정)
    - 설명: `_playerMoves`를 비우고 `LogSelection` 호출을 하지 않는다.
    - 이유: 새 손 시작 시 기록 리셋·지급 로그 제거.
- 파일: `Scripts/Bootstrap/BoardPlacementBootstrap.cs`
  - 심볼: `BoardPlacementBootstrap.PlaceBlock` — 메서드 (수정)
    - 설명: Consume 직후 `RecordPlayerMove(slot, cells, lastDrop)`를 호출한다.
    - 이유: 실제 배치를 손 비교에 연결.
- 파일: `Scripts/Domain/AreaBundleSpawn/AreaBundleOrchestrator.cs`
  - 심볼: `AreaBundleOrchestrator.LogAllClear` — 메서드 (수정)
    - 설명: AllClear `Debug.Log`를 비활성화한다.
    - 이유: HandCompare 외 AreaBundle 로그 숨김.

## 3 — 2026-08-13 · HandCompare Area(rec/act/delta)

**바뀐 것** — 손 시작 보드에서 추천·실제 경로를 시뮬해 Area를 비교 출력한다.

**변경 상세 (왜/무엇)**
- 파일: `Scripts/Bootstrap/BlockSpawnBootstrap.cs`
  - 심볼: `BlockSpawnBootstrap._handStartBoard` — 필드 (추가)
    - 설명: `Fill` 시점 보드 클론을 보관한다.
    - 이유: 추천/실제 Area를 동일 시작점에서 시뮬.
  - 심볼: `BlockSpawnBootstrap.Fill` — 메서드 (수정)
    - 설명: 추첨 전 `_handStartBoard = grid.Clone()` 한다.
    - 이유: 손 시작 Area 기준점.
  - 심볼: `BlockSpawnBootstrap.LogHandCompare` — 메서드 (수정)
    - 설명: `recArea`/`actArea`/`delta`/`actVsRec(HIGHER|LOWER|SAME)`를 로그에 넣고, act가 낮으면 빨강·높으면 초록으로 색을 낸다.
    - 이유: 플레이어 수가 추천보다 Area가 낮은지 바로 보이게.
  - 심볼: `TryScoreRecommendPath` / `TryScorePlayerPath` / `TryApplyCells` / `FormatArea` — 메서드 (추가)
    - 설명: 시작 보드에 칸을 PlaceAndClear로 적용한 뒤 `ScoreTotal`한다. 실패 시 FAIL.
    - 이유: 클리어 순서까지 반영한 Area 비교.
- 파일: `Scripts/Bootstrap/BoardPlacementBootstrap.cs`
  - 심볼: `BoardPlacementBootstrap.PlaceBlock` — 메서드 (수정)
    - 설명: `RecordPlayerMove`를 라인클리어 이후로 옮긴다.
    - 이유: 기록 시점이 실제 배치 확정 후가 되도록.
