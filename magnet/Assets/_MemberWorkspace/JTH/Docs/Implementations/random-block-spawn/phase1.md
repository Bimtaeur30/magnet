# Phase 1 — IBlockShape 계약·임시 데이터

> **구현:** `random-block-spawn` · **Jira:** [SCRUM-18](https://bimtaeur30.atlassian.net/browse/SCRUM-18) · **마일스톤:** M2  
> **상태:** 진행 중 — 계약·임시 데이터만 (추첨 로직·SO 연동은 보류)  
> **변경 기록:** [sequence1.md](sequence1.md) (1:1)

## 목표 (완료 기준)

- [x] 게임 로직이 참조할 `IBlockShape` 계약 정의
- [x] SO 없이 코드로 형태를 넣을 수 있는 `BlockShapeData` + `BlockShapePresets`
- [ ] `BlockSupply` 3후보 추첨 (SCRUM-25 SO 또는 풀 에셋 준비 후)
- [ ] 딩고 `BlockShapeSO` → `IBlockShape` 연동

## 구현 내용 (뭘 어떻게)

| 클래스 | 책임 |
|--------|------|
| `IBlockShape` (`Magnet.Contracts`) | 공용 계약 — `ShapeId`, `CellOffsets` |
| `BlockShapeData` (JTH) | JTH 임시 구현. `new BlockShapeData(id, offsets)` |
| `BlockShapePresets` (JTH) | DESIGN 4.2 기본 형태 정적 프리셋 (`All` 배열) |

### 좌표 규칙

- `CellOffsets`: 배치 피벗 `(0,0)` 기준, `BoardCoordinates`와 동일 축

### SO 연동 시 (예정)

1. 딩고 `BlockShapeSO`가 `Magnet.Contracts.BlockShapes.IBlockShape` 구현
2. 딩고 asmdef에 `Magnet.Contracts` 참조 추가 (`GUID:eb415e56e20154449aab8a7efaff0147`)
3. JTH 게임 로직은 계약만 참조 — JTH asmdef를 딩고가 참조할 필요 없음

## 이 Phase 범위 밖

- SCRUM-25 블록 생성 에디터·형태 에셋 제작
- `BlockSupply` 추첨·3슬롯 관리
- 하단 후보 UI

## 코드 — 어디를 보면 되나

| 보려는 것 | 경로 |
|-----------|------|
| 공용 계약 | `Assets/_Shared/Magnet.Contracts/BlockShapes/IBlockShape.cs` |
| JTH 임시 구현 | `Scripts/Domain/BlockShapeData.cs` |
| JTH 개발용 프리셋 | `Scripts/Domain/BlockShapePresets.cs` |
