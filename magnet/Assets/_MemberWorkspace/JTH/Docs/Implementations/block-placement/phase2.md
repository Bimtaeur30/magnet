# Phase 2 — 보드 상태 부트스트랩·블록 선택·스테이징 뷰

> **구현:** `block-placement` · **Jira:** [SCRUM-19](https://bimtaeur30.atlassian.net/browse/SCRUM-19) · **마일스톤:** M3  
> **상태:** 구현됨 · 사용자 확인 대기  
> **변경 기록:** [sequence2.md](sequence2.md) (1:1)

## 목표 (완료 기준)

- [x] `BoardConfigSO` 기반 `BoardSession` + `BlockPlacementService` 런타임 생성
- [x] `BlockCandidatesUpdatedEvent` 구독 → 3후보 스냅샷 유지 (`BlockSelectionInput`)
- [x] 키보드 **1/2/3** → 슬롯 선택 → `BlockSelectedEvent` Raise
- [x] 선택 형태를 **고정 staging Y, x=0** 에 `BlockPieceView`로 표시
- [x] `BlockSpawnBootstrap` 참조 보유 (`Consume` 호출은 Phase 4)
- [x] `read_console` 컴파일 에러 0

## 구현 내용 (뭘 어떻게)

| 클래스 | 위치 | 책임 |
|--------|------|------|
| `BoardPlacementBootstrap` | `Bootstrap/` | `Awake`에서 세션·서비스 생성. `BlockSelectedEvent` 수신 → 스테이징 뷰 갱신. SO·Bootstrap 참조는 `[SerializeField]` |
| `BlockSelectionInput` | `Input/` | `MagnetInputSO.OnSlotSelected` 구독 → `BlockSelectedEvent` Raise |
| `MagnetInputSO` | `Data/` | `Controls.IPlayerActions` 구현. `new Controls()` + `Player.SetCallbacks(this)` |
| `Controls` | `Input/` | Unity Input System **자동 생성** (`Controls.inputactions`에서) |
| `BlockPieceView` | `Presentation/` | `IBlockShape` + pivot → 칸마다 `SpriteRenderer` 표시. `Clear()`로 숨김 |
| `PlacementConfigSO` | `Data/` | `GetStagingY(cellsPerSide)` = `-(cellsPerSide + extraBelow)`, 피스 색·칸 fill |
| `BlockSelectedEvent` | `Events/` | `SlotIndex`, `Shape` 페이로드 |

### 스테이징 규칙

- **Y:** `PlacementConfigSO.GetStagingY` — 기본 `-(CellsPerSide + 1)` (N=9 → `y=-5`)
- **X (Phase 2):** 초기 pivot `x=0` — **pivot 기준** 표시만. Phase 3에서 **블록 중심 = 포인터 X** 로 변경 ([phase3.md](phase3.md) §입력·좌표).
- **선택:** 후보 갱신만으로 자동 선택하지 않음 — 1/2/3 키 필요

### 설계 근거

- **Domain / Bootstrap 분리:** `BoardSession`·`BlockPlacementService`는 Bootstrap에서 `new`. Reflex 등록 없음.
- **입력:** `Update`·`Input.GetKeyDown` **금지**. `MagnetInputSO` → C# event → MonoBehaviour 구독 (`FOLDER_STRUCTURE.md` §입력).
- **PTY 경계:** `IBlockShape` 계약만 소비. PTY 코드 수정 없음.

## 이 Phase 범위 밖

- x축 드래그, `Simulate`/`TryPlace` 호출
- 부착 잔존 뷰, `BlockPlacedEvent`, `Consume`
- LitMotion 흡착 연출, 하단 후보 UI (M7)

## 코드·에셋 — 어디를 보면 되나

| 보려는 것 | 경로 |
|-----------|------|
| 배치 부트스트랩 | `Scripts/Bootstrap/BoardPlacementBootstrap.cs` |
| 슬롯 선택 입력 | `Scripts/Input/BlockSelectionInput.cs` |
| 입력 SO | `Scripts/Data/MagnetInputSO.cs` |
| Input Actions | `Scripts/Input/Controls.inputactions` |
| Input Actions 생성 코드 | `Scripts/Input/Controls.cs` |
| 기본 입력 SO | `ScriptableObjects/DefaultMagnetInput.asset` |
| 스테이징 뷰 | `Scripts/Presentation/BlockPieceView.cs` |
| 스테이징 설정 SO | `Scripts/Data/PlacementConfigSO.cs` |
| 선택 이벤트 | `Scripts/Events/MagnetGameEvents.cs` → `BlockSelectedEvent` |
| 기본 설정 에셋 | `ScriptableObjects/DefaultPlacementConfig.asset` |
| 씬 배선 | `Scenes/Phase0_Bootstrap.unity` — `BoardPlacementBootstrap`, `StagingBlock`, `BlockSelectionInput` |
