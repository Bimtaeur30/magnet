# board-start-prefill Sequence 1

> Phase 1 구현 기록 · 칸-확률 프리필 + 번들 구멍 + 퍼펙트/올클 이벤트

## 1 — 2026-09-03 · 시작 보드 프리필 재도입

### 프리필 본체

- 파일: `Scripts/Data/BoardPrefillConfigSO.cs` (신규)
  - 심볼: `BoardPrefillConfigSO` — SO (추가)
    - 설명: `Enabled` / `FillProbability` / `Seed` / `HoleBundleMaxCells` / `HoleClusterRadius` /
      `MinEmptyCellsAfterHole` / `MaxGenerateAttempts`.
    - 이유: 채움 강도·구멍 크기·전멸 방지 하한을 코드 수정 없이 조정. `Seed>=0`은 QA 재현용.

- 파일: `Scripts/Domain/Board/BoardPrefillGenerator.cs` (신규)
  - 심볼: `BoardPrefillGenerator.Generate(int, BoardPrefillConfigSO, IReadOnlyList<AreaBundleEntry>, System.Random)` — 정적 메서드 (추가)
    - 설명: 칸마다 `FillProbability`로 채우고, Normal 번들 하나를 3피스 모양대로 뚫는다.
      빈 칸이 하한 미만이면 `MaxGenerateAttempts`까지 재생성. 채울 셀 목록 반환(실패 시 빈 목록).
    - 이유: 순수 함수로 분리해 부트스트랩·테스트에서 재사용. 시작 직후 막힘을 번들 구멍으로 방지.
  - 심볼: `CollectHoleCandidates` / `PunchBundleHole` / `ResolveAnchor` — private (추가)
    - 설명: 큼지막한 번들 제외, 구멍을 중심 주변으로 흩되 모양이 보드 안에 들어오도록 클램프.
    - 이유: 구멍이 한 곳에 뭉치거나 보드 밖으로 새는 것 방지.

- 파일: `Scripts/Domain/Skin/SkinSession.cs`
  - 심볼: `SkinSession.MaxVariant` — 프로퍼티 (추가)
    - 설명: 스킨 하나가 가진 색 변형 개수의 최대치(최소 1).
    - 이유: 프리필 셀 색을 이 범위에서 랜덤 추첨.

- 파일: `Scripts/Presentation/PlacedBlocksView.cs`
  - 심볼: `PlacedBlocksView.SpawnCells(IReadOnlyList<Vector2Int>, IReadOnlyList<int>)` — 메서드 (추가)
    - 설명: 풀(`blockItemSO`)에서 Block을 꺼내 색 변형별로 묶어 `BlockCreatedEvent` 발행 후 `ReplaceCell`.
    - 이유: 스킨 매니저가 변형별로 스프라이트를 입히는 기존 경로를 그대로 태움.
  - 심볼: `blockItemSO` — SerializeField (추가)
    - 설명: 프리필 칸 Block을 꺼낼 풀 아이템. ShapeBlock이 쓰는 것과 동일.
    - 이유: 프리필도 같은 풀을 쓰게.

- 파일: `Scripts/Presentation/GameBoard.cs`
  - 심볼: `GameBoard.PrefillCells(IReadOnlyList<Vector2Int>, IReadOnlyList<int>)` — 메서드 (추가)
    - 설명: `_blocksView.SpawnCells` 후 `Grid.SetOccupied(cell, true)`.
    - 이유: 뷰와 그리드 점유를 한 번에 맞춤.

- 파일: `Scripts/Bootstrap/BlockSpawnBootstrap.cs`
  - 심볼: `boardPrefillConfigSO` — SerializeField (추가)
  - 심볼: `_skinSession` — 필드 (추가)
    - 설명: `BlockSupply`에 넘기던 `SkinSession`을 필드로 보관.
    - 이유: 프리필 색 추첨에 `MaxVariant`가 필요.
  - 심볼: `BlockSpawnBootstrap.PrefillBoard()` — 메서드 (추가)
    - 설명: `Start()`에서 `Fill()` 앞에 1회. config가 없거나 `Enabled=false`면 즉시 반환.
      RNG는 `Seed>=0`이면 고정. 생성기 결과를 `GameBoard.PrefillCells`로 반영.
    - 이유: 첫 손 3개가 채워진 보드를 기준으로 뽑히도록 순서 보장.

### 동반: 퍼펙트 / 올클리어 이벤트

- 파일: `Scripts/Domain/BlockSelection/Simulation/HandOptimalSolver.cs` (신규)
  - 심볼: `HandOptimalSolver.Solve(BoardGrid, IReadOnlyList<ShapeBlockData>, int)` — 정적 메서드 (추가)
    - 설명: 3피스를 전부 놓았을 때 지울 수 있는 라인 수의 최대치를 완전탐색. 전치 중복 접기 +
      보드 비트팩 메모이제이션 + 노드 예산(기본 100만). 예산 초과 시 `IsValid=false`.
    - 이유: 마지막 배치 시점에 플레이어 누적 클리어 수와 비교해 "퍼펙트"를 판정.

- 파일: `Scripts/Domain/Board/BoardGrid.cs`
  - 심볼: `BoardGrid.CountOccupied()` — 메서드 (추가)
    - 설명: 점유 칸 수.
    - 이유: 프리필 하한 체크(생성기 내부는 별도)·퍼펙트 탐색 점유율 게이트.
  - 심볼: `BoardGrid.IsEmpty()` — 메서드 (추가)
    - 설명: 점유 칸이 하나도 없는지.
    - 이유: 올클리어 판정.
  - 심볼: `BoardGrid.TryPackBits(out ulong)` — 메서드 (추가)
    - 설명: 8×8 이하일 때 보드를 64비트로 팩. 초과면 false.
    - 이유: 탐색 중복 상태 제거(메모이제이션) 키.

- 파일: `_Shared/Magnet.Core/Events/MagnetGameEvents.cs`
  - 심볼: `MagnetGameEvents.PerfectClearEvent` / `AllClearEvent` — 필드 + 클래스 (추가)
    - 설명: 핸드 최적값 달성 / 배치 후 보드 완전 비움. 둘 다면 `AllClearEvent`만.
    - 이유: HUD·연출이 "잘했을 때" 이펙트를 걸 트리거. `new` 이벤트 금지 규칙 → 싱글톤 필드.

- 파일: `Scripts/Bootstrap/BlockSpawnBootstrap.cs`
  - 심볼: `LastHandWasPerfect` — 프로퍼티 (추가)
  - 심볼: `perfectSolveMinOccupancy` — SerializeField (추가, 기본 0.4)
    - 설명: 보드 점유율이 이 값 미만이면 `SolveHandOptimal`이 탐색을 건너뛴다.
    - 이유: 빈 보드는 합법 배치가 폭증해 완전탐색이 무겁고, 퍼펙트도 의미가 옅음.
  - 심볼: `SolveHandOptimal()` — 메서드 (추가) · `RecordPlayerMove`에 `clearedLineCount` 파라미터 (변경)
    - 설명: 핸드 확정 직후 점유율 게이트 통과 시 `HandOptimalSolver.Solve`. 마지막 배치에서
      누적 클리어 수 == 최적값이면 `LastHandWasPerfect=true`.
    - 이유: 퍼펙트 판정 상태를 배치 부트스트랩이 읽어 이벤트로 승격.

- 파일: `Scripts/Bootstrap/BoardPlacementBootstrap.cs`
  - 심볼: `magnetGameChannel` — SerializeField (추가)
  - 심볼: `RaiseClearMilestone(bool lastDrop)` — 메서드 (추가)
    - 설명: 라인 클리어 반영 후 `Grid.IsEmpty()`면 `AllClearEvent`, 아니면 마지막 배치 +
      `LastHandWasPerfect`일 때 `PerfectClearEvent`.
    - 이유: 올클과 퍼펙트가 겹치면 올클만 나가도록 한 곳에서 정리.

### 배선

- `NewNew_02_Main.unity` — `BlockSpawnBootstrap.boardPrefillConfigSO` → `DefaultBoardPrefillConfig`,
  `BoardPlacementBootstrap.magnetGameChannel` → `magnetGameChannel`
- `Placed Blocks View.prefab` — `PlacedBlocksView.blockItemSO` → 풀 `Block` 아이템

### 미해결 / 후속

- Unity 에디터 컴파일·플레이 확인 (콘솔 클린 + 시작 시 보드 채워짐 + 첫 손 배치 가능)
- `perfectSolveMinOccupancy` 실측 튜닝 (0.4 임시)
- 퍼펙트/올클 이벤트를 구독하는 HUD 연출은 UI 담당 범위
