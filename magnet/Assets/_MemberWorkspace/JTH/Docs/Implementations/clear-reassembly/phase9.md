# Phase 9 — 파괴 칸 파티클 (`PlayParticleEffectEvent`)

> **구현:** `clear-reassembly` · **Jira:** [SCRUM-20](https://bimtaeur30.atlassian.net/browse/SCRUM-20) · **마일스톤:** M5  
> **상태:** 구현됨 · **선행:** Phase 8 · PTY `presentation-events` Phase 2  
> **변경 기록:** [sequence9.md](sequence9.md) (1:1)  
> **grill-me:** 2026-07-17 확정 (변=직교/모서리=대각 · Sprite.texture · 칸마다 1발 · View SerializeField · BoardView 회전 반영)

## 목표 (완료 기준)

- [x] `DestroyCellViews`에서 파괴 칸마다 `PresentationEvents.PlayParticleEffectEvent` raise
- [x] texture = 현재 스킨 `Sprite.texture`
- [x] rotation = 테두리 바깥 방향(변 직교·모서리 대각) + `BoardView.transform.rotation`
- [x] `BlockBlast` PoolItemSO · `presentationChannel`을 `PlacedBlocksView` SerializeField로 연결
- [x] `read_console` 컴파일 에러 0

## 구현 내용 (뭘 어떻게)

| 대상 | 내용 |
|---|---|
| `PlacedBlocksView.DestroyCellViews` | Destroy 전에 칸마다 `PlayParticleEffectEvent.Init(blockBlastEffect, worldPos, worldRot, skinTexture)` raise |
| `GetOutwardGridDirection` | `\|x\|==\|y\|` → 대각 `(Sign x, Sign y)` / `\|x\|>\|y\|` → 좌우 / 그 외 → 상하 |
| `GetOutwardWorldRotation` | 파티클 기본 +Y → 바깥 방향 `FromToRotation` 후 `BoardView.rotation * local` |
| `_currentSkin` | `SkinInitialized`/`SkinChanged`에서 캐시 → 파괴 시 `Sprite.texture` |
| SerializeField | `presentationChannel`, `blockBlastEffect`(`BlockBlast.asset`) |
| `Magnet.JTH.asmdef` | `ObjectPool.Runtime` 참조 추가 (`PoolItemSO`) |
| `Board.prefab` | 위 두 필드 직렬화 할당 |

## 이 Phase 범위 밖

- 파티클 프리팹/`PoolItemSO` 제작 (PTY 완료 · `BlockBlast` 기존)
- 아틀라스 Sprite UV 지원
- `_System.prefab`의 별도 PlacedBlocks 복제본 정리 (필드는 동작용으로 할당해 둠)

## 코드 — 어디를 보면 되나

| 보려는 것 | 경로 |
|-----------|------|
| raise·방향 | `Scripts/Presentation/PlacedBlocksView.cs` |
| 이벤트 계약 | `Assets/_Shared/Magnet.Core/Events/PresentationEvents.cs` |
| 풀 아이템 | `Assets/GameLib/ObjectPool/Items/BlockBlast.asset` |
