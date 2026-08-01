# Phase 1 — 골격 + Hospitality·Pressure 42-ID 이식

## 목표

핸드오프 체인을 기본 공급기로 두고 특수 티어 게이트를 얹는 병합 오케스트레이터를 만들어 Drawer 배선을 교체한다. 이 시점에 게임이 완전 동작(특수 티어 없이도 체인이 전 턴 담당).

## 구현 내용

### 오케스트레이터 (`Domain/HybridSpawn/`)

- `HybridSpawnOrchestrator` — 게이트 순서: Relife → Trap → ComboBreak → Hospitality → Pressure → **BaseChain**(핸드오프). 특수 티어 확정 시 `BlockBlastAlgorithm.RecordExternalRound`로 히스토리 기록 + 라운드 진행 (트레이트는 우회 — 솔버 보장 보호).
- `HybridPiecePool` — 42-ID 가중 추첨 (칸 수 가중표). 전부 동일·직전 트리플(다중집합) 재추첨 회피.
- `HybridHospitalityGenerator` / `HybridPressureGenerator` — 구 생성기의 42-ID 이식판. 로직 동일(기회 게이트 + 품질 가중 / 유일해 + 난이도 하한 + `UniqueSolution` 보관), 입력만 42-ID 풀.
- `HybridSelectionResult` / `HybridTier` — 진단·UI 훅 DTO (Pressure 유일해, BaseChain 알고리즘 ID 포함).
- `HybridSpawnProbes` — placementFreedom 프로브: 회전 중복 없는 대표 canonical 13종.

### 기존 코드 재사용을 위한 인터페이스 추출 (동작 불변)

- `IBoardHealthTuning` / `IBlameTuning` / `IOpportunityTuning` 신설, `BoardHealthCalculator`·`BlameTracker`·`OpportunityScorer` 시그니처를 인터페이스로 변경.
- `BlockSelectionTuningSO`·`HybridTuningSO` 둘 다 구현 → 구 코드도 그대로 컴파일.

### 튜닝·배선

- `HybridTuningSO` — Health·Blame·티어 게이트·샘플 예산 + 42-ID 칸 수 가중표 5종 (구 SO에서 번들·Easy·Momentum·17종 가중표 필드는 승계 안 함).
- `HybridDrawer : AbstractDrawer` 신설, `BlockSpawnBootstrap` 배선 교체.
- Bootstrap에 BoardHealth·BlameTracker 입력 복구: 매 `Fill`에서 Health 계산 → 직전 턴 Blame 정산(`OnTurnEnded`) → 컨텍스트 주입 → 턴 시작 스냅샷 갱신. 턴 정산·티어 선택 로그 1줄씩.

## 범위 밖

- Relife·Trap·ComboBreak (Phase 2)
- `IsRetrySession` 배선 (game-over 미구현 — 스텁 false)
- 구 `BlockSelection`·`BlockBlastDrawer` 삭제 (롤백 보존)

## 코드·에셋 맵

| 경로 | 역할 |
|------|------|
| `Scripts/Domain/HybridSpawn/HybridSpawnOrchestrator.cs` | 게이트 스택 + 체인 위임 (신규) |
| `Scripts/Domain/HybridSpawn/HybridPiecePool.cs` | 42-ID 가중 풀 + 트리플 샘플러 (신규) |
| `Scripts/Domain/HybridSpawn/HybridHospitalityGenerator.cs` | 접대 42-ID 이식 (신규) |
| `Scripts/Domain/HybridSpawn/HybridPressureGenerator.cs` | 유일수 42-ID 이식 (신규) |
| `Scripts/Domain/HybridSpawn/HybridSelectionResult.cs` · `HybridTier.cs` · `HybridSpawnProbes.cs` | DTO·enum·프로브 (신규) |
| `Scripts/Domain/Spawn/HybridDrawer.cs` | AbstractDrawer 구현 (신규) |
| `Scripts/Data/HybridTuningSO.cs` · `CellCountWeightTable.cs` | 튜닝 SO (신규) |
| `Scripts/Domain/BlockSelection/Health/IBoardHealthTuning.cs` 외 인터페이스 2종 | 재사용 심 (신규) |
| `Scripts/Domain/BlockBlast/BlockBlastAlgorithm.cs` | `RecordExternalRound` 추가 (수정) |
| `Scripts/Bootstrap/BlockSpawnBootstrap.cs` | Drawer·Health·Blame 배선 (수정) |
| `ScriptableObjects/HybridSpawn/DefaultHybridTuning.asset` | 튜닝 에셋 (신규) |
