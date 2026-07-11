# Phase 3 — 3후보 공급 (`BlockSupply`)

> **구현:** `random-block-spawn` · **Jira:** [SCRUM-18](https://bimtaeur30.atlassian.net/browse/SCRUM-18) · **마일스톤:** M2
> **상태:** 완료
> **변경 기록:** [sequence3.md](sequence3.md) (1:1)

## 목표 (완료 기준)

- [x] 항상 **3슬롯** 후보 유지 (`DESIGN.md` §4.2)
- [x] `Fill()` — 시작 시 3슬롯 전부 `BlockDrawer`로 채움
- [x] `Consume(slotIndex)` — 해당 슬롯만 재추첨
- [x] 현재 후보 읽기 (`Candidates`, `GetCandidate`)
- [x] 후보 갱신 시 `BlockCandidatesUpdatedEvent` → `magnetGameChannel` Raise (`[SerializeField]`)
- [x] 형태 소스: `[Inject] IBlockShapeSource` (PTY `BlockShapeSourceSO` 풀)
- [x] `read_console` 컴파일 에러 0

## 구현 내용 (뭘 어떻게)

| 클래스 | 위치 | 책임 |
|--------|------|------|
| `BlockSupply` | `Domain.Spawn` | 순수 Domain. 3슬롯 상태·`Fill`/`Consume`/스냅샷. Unity·이벤트 의존 0 |
| `BlockCandidatesUpdatedEvent` | `Events` | 후보 3개 스냅샷 담는 `GameEvent` |
| `BlockSpawnBootstrap` | `Bootstrap` | `Start`에서 `Fill` + 이벤트 Raise. `Consume` 공개 (SCRUM-19 연동용) |

### 설계 근거

- **Domain / Bootstrap 분리:** `BlockSupply`는 이벤트·DI 모름. Raise는 Bootstrap만.
- **형태 소스:** `[Inject] IBlockShapeSource` — PTY `BlockShapeSourceInstaller` 등록 풀 사용 (Path A, DI 예외).
- **이벤트 스냅샷:** `CreateSnapshot()`으로 복사본 전달, 외부 변조 방지.

## 이 Phase 범위 밖

- 하단 후보 UI (M7/M9)
- `BlockPlacedEvent`와 `Consume` 자동 연결 (SCRUM-19)
- 「3개 모두 배치 불가 → 게임오버」(SCRUM-22)
- 가중치 추첨 (Phase 4)

## 코드 — 어디를 보면 되나

| 보려는 것 | 경로 |
|-----------|------|
| 3슬롯 공급 | `Scripts/Domain/Spawn/BlockSupply.cs` |
| 후보 갱신 이벤트 | `Scripts/Events/MagnetGameEvents.cs` |
| 부트스트랩 | `Scripts/Bootstrap/BlockSpawnBootstrap.cs` |
| 씬 배선 | `Scenes/Phase0_Bootstrap.unity` — `BlockSpawnBootstrap` |
| 형태 풀 (PTY) | `PTY/ScriptableObjects/BlockShapeSource.asset` |
