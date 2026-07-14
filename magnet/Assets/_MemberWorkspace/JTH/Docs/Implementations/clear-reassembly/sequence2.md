# sequence2 — Phase 2 변경 기록

> Phase 계획: [phase2.md](phase2.md)

## 1 — 2026-07-14 · 부채꼴 목표 배정 + ClearWave 연쇄

**바뀐 것** — Domain에서 테두리 파괴 후 바깥 칸 재배치를 웨이브 단위로 선확정하고 연쇄한다.

**변경 상세 (왜/무엇)**
- 파일: `Scripts/Domain/Clear/ClearWave.cs`
  - 심볼: `CellRelocation` / `ClearWave` / `ClearReassemblyResult` (추가)
    - 설명: 파괴 칸·재배치 목록·웨이브 배열 DTO.
    - 이유: Bootstrap/Presentation이 Domain 결과를 재생만 하게 한다.
- 파일: `Scripts/Domain/Clear/CellRelocationOrder.cs`
  - 심볼: `Sort` / `Chebyshev` / `ClockAngle01` (추가)
    - 설명: half+1 링 우선, 링 안 12시→시계방향 정렬.
    - 이유: 배정·연출 스태거 순서를 Domain에서 고정한다.
- 파일: `Scripts/Domain/Clear/CellRelocationTargetFinder.cs`
  - 심볼: `TryFind` (추가)
    - 설명: 자석 방향 90° 부채꼴∩서클 안 빈칸 중 자석 최근접 칸 선택.
    - 이유: OverlapCircle 규칙을 Domain 격자로 동치 구현.
- 파일: `Scripts/Domain/Clear/ClearReassemblyService.cs`
  - 심볼: `ResolveAllWaves` / `CollectEjectors` (추가)
    - 설명: 최내곽 파괴→이젝터 배정·MoveCell→재검사를 연쇄한다.
    - 이유: 웨이브 단위 Domain 선확정(설계 A).
- 파일: `Scripts/Domain/Clear/SquareClearService.cs`
  - 심볼: `DetectAndApply` (수정)
    - 설명: `RemoveCellsAt`만 사용(테두리). 주 경로는 ReassemblyService.
    - 이유: 구 API 잔존 호환.

**메모** — 이벤트·Bootstrap 연동은 sequence3.
---
## 2 — 2026-07-14 · 이젝터 전원 이륙 + 목표 탐색 폴백

**바뀐 것** — 배정 전 이젝터 점유를 모두 해제하고, 부채꼴/서클 실패 시 단계적 폴백. Assert 대신 경고 후 원위치 복구.

**변경 상세 (왜/무엇)**
- 파일: `Scripts/Domain/BoardSession.cs`
  - 심볼: `ReleaseOccupancy` (추가)
    - 설명: 셀 엔티티는 유지한 채 격자 점유만 푼다.
    - 이유: 전원 이륙 후 빈칸 풀을 공유해야 같은 면 밀집 배정이 된다.
- 파일: `Scripts/Domain/Clear/CellRelocationTargetFinder.cs`
  - 심볼: `TryFind` / `TryFindInternal` (수정)
    - 설명: 90°+서클 → 부채꼴만 → 180° → 360°, 후보는 반드시 안쪽 링.
    - 이유: 유클리드 원/점유 때문에 목표가 비는 케이스 완화.
- 파일: `Scripts/Domain/Clear/ClearReassemblyService.cs`
  - 심볼: `ResolveAllWaves` (수정)
    - 설명: 파괴 후 이젝터 `ReleaseOccupancy` → 배정. 실패 시 원위치 재점유.
    - 이유: Assert로 턴이 깨지지 않게.

**메모** — PreRotationDelay는 Bootstrap/PlacementConfig.
---
