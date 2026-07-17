# Sequence 3 — blocked-ring-dim Phase 3

## 1 — 2026-07-17 · 갱신 배선 + Dim Config SO

**바뀐 것** — 생성: `BlockedRingDimConfigSO.cs`, `DefaultBlockedRingDimConfig.asset`  
수정: `PlacementConfigSO.cs`, `DefaultPlacementConfig.asset`, `BoardPlacementBootstrap.cs`, `PlacedBlocksView.cs`, `INSPECTOR_TOOLTIPS.md`

**변경 상세 (왜/무엇)**

- 파일: `Scripts/Data/BlockedRingDimConfigSO.cs`
  - 심볼: `BlockedRingDimConfigSO.dimMultiply` — SerializeField (추가)
    - 설명: 비활성 링 점유 칸 RGB 배수(0.05~1, 기본 0.35).
    - 이유: Phase 2 View 로컬 필드를 SO로 옮겨 인게임 튜닝·에셋 공유.
  - 심볼: `BlockedRingDimConfigSO.DimMultiply` — 프로퍼티 (추가)
    - 설명: `dimMultiply` 읽기 전용 노출.
    - 이유: Presentation이 SO 값만 소비.
  - 심볼: `BlockedRingDimConfigSO.OnValidate()` — 메서드 (추가)
    - 설명: 인스펙터 편집 시 배수를 클램프.
    - 이유: 0 근처·1 초과 입력으로 칸이 사라지거나 밝아지는 실수 방지.

- 파일: `Scripts/Data/PlacementConfigSO.cs`
  - 심볼: `PlacementConfigSO.blockedRingDim` — SerializeField (추가)
    - 설명: dim 설정 SO 참조.
    - 이유: 다른 배치 연출 SO와 같이 PlacementConfig 한곳에서 묶기.
  - 심볼: `PlacementConfigSO.BlockedRingDim` — 프로퍼티 (추가)
    - 설명: `blockedRingDim` 노출.
    - 이유: `PlacedBlocksView.ResolveDimMultiply`가 읽음.

- 파일: `Scripts/Bootstrap/BoardPlacementBootstrap.cs`
  - 심볼: `BoardPlacementBootstrap.TryConfirmPlacement` — 메서드 (수정)
    - 설명: `PlayPlaceAsync` 직후 `RefreshBlockedRingDim` 호출. Rotate 전후는 호출하지 않음.
    - 이유: grill — 배치 성공 후 점유 기준 dim 갱신. 회전은 상대 관계가 같아 생략.
  - 심볼: `BoardPlacementBootstrap.PlayClearReassemblyWavesAsync` — 메서드 (수정)
    - 설명: 각 웨이브 폭발·재배치 연출 직후 `RefreshBlockedRingDim`.
    - 이유: View 좌표가 웨이브 단위로 맞춰진 뒤 dim을 다시 계산. Domain은 선확정이라 연출 후 refresh가 UX에 맞음.

- 파일: `Scripts/Presentation/PlacedBlocksView.cs`
  - 심볼: `PlacedBlocksView.blockedRingDimMultiply` — SerializeField (삭제)
    - 설명: View 로컬 배수 제거.
    - 이유: SO로 이전.
  - 심볼: `PlacedBlocksView.ResolveDimMultiply()` — 메서드 (추가)
    - 설명: `placementConfig.BlockedRingDim.DimMultiply`, null이면 0.35 폴백.
    - 이유: 에셋 미할당 시에도 dim이 동작하게.
  - 심볼: `PlacedBlocksView.RefreshBlockedRingDim()` — 메서드 (수정)
    - 설명: 배수를 `ResolveDimMultiply`에서 가져옴.
    - 이유: SO 연동.

- 파일: `ScriptableObjects/DefaultBlockedRingDimConfig.asset` (추가)
  - 심볼: 에셋 `dimMultiply = 0.35`
    - 설명: 기본 dim 설정 에셋.
    - 이유: DefaultPlacementConfig에 연결할 기본값.
- 파일: `ScriptableObjects/DefaultPlacementConfig.asset` (수정)
  - 심볼: `blockedRingDim` 참조 할당
    - 설명: DefaultBlockedRingDimConfig 연결.
    - 이유: 씬이 쓰는 PlacementConfig에 dim SO가 비지 않게.

**메모** — 드래그 프리뷰 dim은 범위 밖.

---
