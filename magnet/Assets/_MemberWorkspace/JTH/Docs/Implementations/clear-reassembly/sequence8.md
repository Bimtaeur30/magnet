# sequence8 — Phase 8 변경 기록

> Phase 계획: [phase8.md](phase8.md)

## 1 — 2026-07-16 · 각도 제거, 수선 복도 폭

**바뀐 것** — 부채꼴 각도를 없애고, 원점–원래칸 직선에 대한 수직 반폭 복도로 재배치 후보를 고른다.

**변경 상세 (왜/무엇)**
- 파일: `Scripts/Data/PlacementConfigSO.cs`
  - 심볼: `CorridorHalfWidth` — 프로퍼티 (추가), `HalfSectorDegrees` — 프로퍼티 (삭제)
    - 설명: 격자 단위 수선 반폭(기본 0.75, 0.01~3).
    - 이유: 각도 대신 폭으로 대각선도 안정적으로 굴리기 위해.
- 파일: `Scripts/Domain/Clear/CellRelocationTargetFinder.cs`
  - 심볼: `TryFind` / `CollectCorridorCandidates` / `TryFindIdealPerp` / `TryPickEmptyAtIdealPerp` / `PerpendicularDistance` / `TryReadSlot` — 메서드 (수정·추가)
    - 설명: 축 투영 구간(0, maxDist] ∩ 수선거리≤반폭. 이상 수선거리 빈칸 중 축투영 최소. 각도 API 제거.
    - 이유: “수선 닿는 칸 + 폭” 모델. 이상 슬롯 막히면 제자리.
  - 심볼: `TryFindIdealAngle` / `TryPickEmptyAtIdealAngle` / `CollectSectorCircleCandidates` — 메서드 (삭제)
    - 설명: 각도 기반 API 제거.
    - 이유: 각도 개념 폐기.
- 파일: `Scripts/Domain/Clear/ClearReassemblyService.cs`
  - 심볼: `ResolveAllWaves(session, corridorHalfWidth)` — 메서드 (수정)
    - 설명: 인자명을 복도 반폭으로 바꾸고 Finder에 전달.
    - 이유: SO·Finder API 변경 반영.
- 파일: `Scripts/Bootstrap/BoardPlacementBootstrap.cs`
  - 심볼: Place 경로 `ResolveAllWaves` 호출 — (수정)
    - 설명: `placementConfig.CorridorHalfWidth` 전달.
    - 이유: HalfSectorDegrees 삭제.
- 파일: `Scripts/Presentation/CellRelocationTargetGizmo.cs`
  - 심볼: `DrawCorridor` — 메서드 (추가), `DrawSectorAndCircle` — 메서드 (삭제)
    - 설명: 축 평행 복도 사각형을 그린다.
    - 이유: 새 기하 시각화.
- 파일: `ScriptableObjects/DefaultPlacementConfig.asset`
  - 심볼: `CorridorHalfWidth` = 0.75 (추가/교체)
    - 설명: 구 HalfSectorDegrees:45 제거.
    - 이유: 단위가 도→격자로 바뀜.

## 2 — 2026-07-16 · 보드밖 주차 제거 (막히면 제자리만)

**바뀐 것** — 안쪽이 막히면 보드밖으로 가지 않고 제자리.

**변경 상세 (왜/무엇)**
- 파일: `Scripts/Domain/Clear/CellRelocationTargetFinder.cs`
  - 심볼: `TryAllocateOutsidePark` — 메서드 (삭제)
    - 설명: 보드 밖 주차 폴백 제거. 실패 시 `false`.
    - 이유: 자기 자리를 이미 차지하므로 바깥으로 밀리지 않음.

## 3 — 2026-07-16 · 중간 빈칸 건너뛰기 수정 (nearest inward)

**바뀐 것** — 자석 최근접 빈칸으로 점프하지 않고, 복도 안 **안쪽 방향 최근접 빈칸**으로 이동해 중간 구멍을 메운다.

**변경 상세 (왜/무엇)**
- 파일: `Scripts/Domain/Clear/CellRelocationTargetFinder.cs`
  - 심볼: `TryPickNearestInwardEmpty` — 메서드 (추가)
    - 설명: 빈칸 순위 = 수선거리 최소 → 축 투영 **최대**(지금 위치에서 안쪽으로 가장 가까운 구멍) → 시계.
    - 이유: min along(자석 최근접)은 (0,3) 구멍을 건너 (0,1)로 점프하는 버그를 냄.
  - 심볼: `TryFindIdealPerp` / `TryPickEmptyAtIdealPerp` — 메서드 (삭제)
    - 설명: 점유 칸 이상 수선 잠금·innermost 선택 제거.
    - 이유: 복도 폭 모델과 구멍 메우기에 맞게 단순화.
- 파일: `Docs/relocation_gap_repro.py` — (추가 후 삭제)
  - 심볼: 검증 스크립트
    - 설명: min_along이 (0,1)로 점프·nearest_inward가 (0,3) 메움 재현.
    - 이유: 일시 검증. JTH 영구 테스트 없음 정책으로 삭제.
