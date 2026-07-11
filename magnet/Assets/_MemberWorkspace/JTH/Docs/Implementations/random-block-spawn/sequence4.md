# Sequence — Phase 4 (random-block-spawn)

> **Phase:** 취소 — 가중치 불필요(사용자 요청). 변경은 `sequence4` 항목 2에 기록.

## 1 — 2026-07-07 · 가중치 추첨 (추가)

(구현됨 → 항목 2에서 전부 제거)

---

## 2 — 2026-07-07 · 가중치 관련 전부 제거

**바뀐 것**

- 삭제: `Scripts/Domain/Spawn/IShapeWeightSource.cs`
- 삭제: `Scripts/Data/BlockSpawnWeightsSO.cs`
- 삭제: `ScriptableObjects/BlockSpawnWeights.asset`
- 수정: `Scripts/Domain/Spawn/BlockDrawer.cs` — 균등 추첨으로 복원
- 수정: `Scripts/Bootstrap/BlockSpawnBootstrap.cs` — `blockSpawnWeights` 제거
- 수정: `Scenes/Phase0_Bootstrap.unity` — `blockSpawnWeights` 배선 제거
- 삭제: `phase4.md`

**변경 상세 (왜/무엇)**

- 파일: `Scripts/Domain/Spawn/IShapeWeightSource.cs` (삭제)
  - 설명: shapeId별 가중치 계약.
  - 이유: 프로토타입에서 출현 가중치 불필요 — 사용자 요청으로 제거.
- 파일: `Scripts/Data/BlockSpawnWeightsSO.cs`, `BlockSpawnWeights.asset` (삭제)
  - 설명: 가중치 데이터 SO·에셋.
  - 이유: 가중치 기능 자체 미사용.
- 파일: `Scripts/Domain/Spawn/BlockDrawer.cs`
  - 심볼: `BlockDrawer._weights`, 가중치 생성자·누적 `Draw()` (삭제)
    - 설명: Phase 4에서 넣었던 가중치 추첨 로직.
    - 이유: 균등 추첨만 유지.
  - 심볼: `BlockDrawer(IBlockShapeSource, IRandom)` + 균등 `Draw()` (복원)
    - 설명: `Next(Count)`로 인덱스 선택.
    - 이유: Phase 2 동작으로 되돌림.
- 파일: `Scripts/Bootstrap/BlockSpawnBootstrap.cs`
  - 심볼: `blockSpawnWeights` 필드·assert·Drawer 인자 (삭제)
    - 설명: 가중치 SO SerializeField.
    - 이유: 가중치 제거에 따른 정리.

**검증**

- `read_console` 컴파일 에러 0.

**메모**

- `random-block-spawn` Phase 4(가중치)는 **취소**. SCRUM-18 범위는 Phase 1~3(형태·균등 추첨·3후보)로 완료.

---
