# board-start-prefill Phase 1 — 계획

## 목표

새 게임을 빈 8×8 보드로 시작하지 않는다. 시작 시 ~40% 안팎을 채우고, 시작 직후
배치가 막히지 않도록 번들 모양의 빈 공간을 하나 확보한다.

## 구현 내용

1. `BoardPrefillConfigSO` — 채움 확률·시드·구멍 번들 셀 상한·구멍 산포 반경·빈 칸 하한·재시도 횟수
2. `BoardPrefillGenerator.Generate(boardSize, config, normalBundles, rng)` — 순수 함수.
   칸-확률 채움 → 번들 구멍 → 빈 칸 하한 체크 → 실패 시 재생성. 채울 셀 목록 반환
3. `GameBoard.PrefillCells(cells, skinIds)` — 셀 뷰 스폰 + 그리드 점유 반영
4. `PlacedBlocksView.SpawnCells(cells, skinIds)` — 풀에서 Block 꺼내 색 변형별로 묶어
   `BlockCreatedEvent` 발행 후 셀에 배치
5. `BlockSpawnBootstrap.PrefillBoard()` — `Start()`에서 `Fill()` **앞에** 호출
6. `SkinSession.MaxVariant` — 프리필 랜덤 색 추첨 범위

## 범위 밖

- 원작 색-덩어리(피스 단위 색) 재현 — 필요 시 후속
- 이어하기(새 게임일 때만 프리필) — 이어하기 구현 시 조건 추가
- 프리필 보드에서 완성 줄 금지 — 허용

## 코드·에셋 맵

| 종류 | 경로 |
|------|------|
| 설정 SO | `Scripts/Data/BoardPrefillConfigSO.cs` |
| 생성기 | `Scripts/Domain/Board/BoardPrefillGenerator.cs` |
| 프레젠테이션 | `Scripts/Presentation/GameBoard.cs` · `Scripts/Presentation/PlacedBlocksView.cs` |
| 부트스트랩 | `Scripts/Bootstrap/BlockSpawnBootstrap.cs` |
| 에셋 | `ScriptableObjects/DefaultBoardPrefillConfig.asset` |
| 씬 배선 | `_MemberWorkspace/KTJ/01_Scene/NewNew/NewNew_02_Main.unity` (`boardPrefillConfigSO`) |
| 프리팹 배선 | `Prefabs/Board/Placed Blocks View.prefab` (`blockItemSO`) |
