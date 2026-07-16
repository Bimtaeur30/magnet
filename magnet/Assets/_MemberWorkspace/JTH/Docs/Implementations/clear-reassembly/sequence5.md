# sequence5 — Phase 5 변경 기록

> Phase 계획: [phase5.md](phase5.md)

## 1 — 2026-07-16 · 안쪽 진입 허용 + 반대편 폴백 제거

**바뀐 것** — 폭발 빈칸으로 바깥 칸이 들어올 수 있게 하고, 부채꼴을 SO 반각→최대 90°로 제한해 반대편 이동을 막는다.

**변경 상세 (왜/무엇)**
- 파일: `Scripts/Data/PlacementConfigSO.cs`
  - 심볼: `HalfSectorDegrees` (추가)
    - 설명: 재배치 부채꼴 반각(1~90, 기본 45).
    - 이유: 각도를 인스펙터에서 조정한다.
    - 영향: Bootstrap → ClearReassemblyService → TargetFinder.
- 파일: `ScriptableObjects/DefaultPlacementConfig.asset`
  - 심볼: `HalfSectorDegrees` = 45 (추가)
    - 설명: 기본 에셋에 필드 직렬화.
    - 이유: 새 필드 기본값.
- 파일: `Scripts/Domain/Clear/CellRelocationTargetFinder.cs`
  - 심볼: `TryFind` / `TryFindInternal` (수정), `AllocateOutsidePark` (삭제)
    - 설명: `preserveBelowChebyshev` 제거(안쪽 빈칸 후보 포함). 탐색 = 설정각∩서클 → 설정각 → 90° → 안쪽·자석 최근접(각도 무시). 180°/비안쪽/보드밖 주차 제거.
    - 이유: 바깥→안 진입 실패·반대편 이동 버그 수정.
    - 영향: ClearReassemblyService만 호출.
- 파일: `Scripts/Domain/Clear/ClearReassemblyService.cs`
  - 심볼: `ResolveAllWaves` (수정)
    - 설명: `halfSectorDegrees` 인자. 목표 chebyshev &lt; half 시 OutsidePark로 쫓아내던 가드 삭제. 목표 없으면 원위치 재점유.
    - 이유: Finder가 고른 안쪽 목표를 Service가 무효화하지 않게.
- 파일: `Scripts/Bootstrap/BoardPlacementBootstrap.cs`
  - 심볼: `TryConfirmPlacement` (수정)
    - 설명: `placementConfig.HalfSectorDegrees` 전달.
    - 이유: Domain에 SO 의존 없이 설정값만 넘긴다.
- 파일: `Docs/INSPECTOR_TOOLTIPS.md`
  - 심볼: `HalfSectorDegrees` 행 (추가)
    - 설명: Tooltip 목록 동기화.

**메모** — 낙하 모션·스폰 회전은 다음 Phase.
---
