# Sequence — Phase 1 (random-block-spawn)

> **Phase:** [phase1.md](phase1.md) 와 1:1.

## 1 — 2026-07-07 · IBlockShape 계약·임시 데이터

**바뀐 것**

- 생성: `Scripts/Domain/IBlockShape.cs`
- 생성: `Scripts/Domain/BlockShapeBounds.cs`
- 생성: `Scripts/Domain/BlockShapeData.cs`
- 생성: `Scripts/Domain/BlockShapePresets.cs`

## 2 — 2026-07-07 · 공용 어셈블리로 계약 이동

**바뀐 것**

- 생성: `Assets/Shared/Magnet.Contracts/` — `Magnet.Contracts.asmdef`
- 이동: `IBlockShape`, `BlockShapeBounds` → `Magnet.Contracts.BlockShapes`
- 수정: `Magnet.JTH.asmdef` — `Magnet.Contracts` 참조 추가
- 삭제: `Scripts/Domain/IBlockShape.cs`, `BlockShapeBounds.cs` (JTH 중복)

**메모**

- 계약은 멤버 Workspace 밖. 딩고 SO는 `Magnet.Contracts`만 참조하면 됨.
- JTH 임시 데이터(`BlockShapeData`, `BlockShapePresets`)는 JTH에 유지.

---
## 3 — 2026-07-07 · 계약 최소화 (Bounds 제거)

**바뀐 것**

- 수정: `Assets/Shared/Magnet.Contracts/BlockShapes/IBlockShape.cs` — `BoundsSize` 제거 (최소 계약)
- 수정: `Scripts/Domain/BlockShapeData.cs` — `BoundsSize` 및 계산 로직 제거
- 삭제: `Assets/Shared/Magnet.Contracts/BlockShapes/BlockShapeBounds.cs`

**메모**

- 크기 정보는 필요 시 `CellOffsets`로 계산하도록 하고, 현재 Phase에서는 계약을 최소화한다.

---