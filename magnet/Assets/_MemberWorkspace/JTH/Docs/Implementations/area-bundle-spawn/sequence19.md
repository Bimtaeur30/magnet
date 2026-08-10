# sequence19 — Phase 19 변경 기록

## 1 — 2026-08-10 · 올클 고정 풀 + Exact

- 생성/수정: `Scripts/Data/AreaBundleStarterData.cs`
  - 심볼: `CreateAllClear()` — 메서드 (추가)
    - 설명: Blocks2 대형·고빈도 핸드 12개를 `ac01`~`ac12`로 반환한다.
    - 이유: 올클 전용 고정 후보. Exact 검사 비용을 소수로 제한.

- 수정: `Scripts/Data/AreaBundlePoolSO.cs`
  - 심볼: `allClearBundles` — 필드 (추가)
    - 설명: 올클 Exact 검사에 쓸 고정 번들 리스트.
    - 이유: Normal 샘플+빔과 분리.
  - 심볼: `allClearMaxOccupied` — 필드 (추가, 기본 16)
    - 설명: 점유 칸이 이 값 이하일 때만 올클 풀을 검사한다.
    - 이유: “칸이 적은 판”에서만 Exact를 돌려 성능을 지킨다.
  - 심볼: `AllClearBundles` / `AllClearMaxOccupied` — 프로퍼티 (추가)
  - 심볼: `FillStarterBundles` — 메서드 (수정)
    - 설명: AllClear 리스트도 `CreateAllClear`로 채운다.
    - 이유: 에셋 일괄 갱신.

- 수정: `Scripts/Domain/AreaBundleSpawn/AreaBundleMetrics.cs`
  - 심볼: `CanEmptyBoard(board, pieces, sequenceCap)` — 메서드 (추가)
    - 설명: 완주 DFS 중 최종 점유 0인 경로가 있으면 true. 첫 성공에서 종료.
    - 이유: 올클 Exact 판정. 빔 미사용.
  - 심볼: `SearchEmpty` / `CountOccupiedCells` — (추가)
    - 설명: CanEmptyBoard용 탐색·점유 카운트.
    - 이유: MaxArea 탐색과 분리해 조기 성공 반환.

- 수정: `Scripts/Domain/AreaBundleSpawn/AreaBundleOrchestrator.cs`
  - 심볼: `TrySelectNormalPriority` — 메서드 (수정)
    - 설명: 올클은 `FilterBoardEmptied`(빔) 대신 점유≤max일 때 `TrySelectAllClearExact` → 확률. 이후 멀티/Area는 ScoreSurvivors(빔) 유지.
    - 이유: 올클 미검출(빔 가지치기) 회피.
  - 심볼: `TrySelectAllClearExact` — 메서드 (추가)
    - 설명: 올클 풀 전수 Exact. 통과 중 예측 Area 최대 번들 선택.
    - 이유: 고정 풀에서 보드에 맞는 패만 지급.
  - 심볼: `FilterBoardEmptied` / `ExcludeBoardEmptied` — (삭제)
    - 설명: 빔 기반 올클 필터를 제거한다.
    - 이유: 올클 경로가 Exact 고정 풀로 대체됨.

- 문서: `phases.md` · `phase19.md` · `IMPLEMENTATIONS.md` · `TUNING_STAGES.md` · `INSPECTOR_TOOLTIPS.md`
- 에셋: `DefaultAreaBundlePool` — ContextMenu Fill 또는 에디터에서 AllClear 12개 채움 필요
