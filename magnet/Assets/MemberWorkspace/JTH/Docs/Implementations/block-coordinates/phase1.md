# Phase 1 — BoardConfigSO·좌표·격자 렌더

> **구현:** `block-coordinates` · **Jira:** [SCRUM-17](https://bimtaeur30.atlassian.net/browse/SCRUM-17) · **마일스톤:** M1  
> **상태:** 구현됨 · 사용자 확인 대기  
> **변경 기록:** [sequence1.md](sequence1.md) (1:1)

이 파일은 이 Phase에서 **뭘 어떻게 구현하는지**를 적는다. 실제로 바뀐 내용은 `sequence1.md`에 기록.

## 목표 (완료 기준)

- N×N 격자가 화면에 렌더된다 (기본 N=9)
- 중앙 자석 축 1칸이 색으로 구분된다
- `BoardConfigSO`로 크기·색을 에디터에서 조정할 수 있다
- 격자 좌표 ↔ 월드 좌표 변환이 한 곳(`BoardCoordinates`)에 모인다

## 구현 내용 (뭘 어떻게)

| 레이어 | 클래스 | 책임·구현 방식 |
|--------|--------|----------------|
| Data | `BoardConfigSO` | N(홀수 검증), cellSize, 일반 칸 색, 자석 축 색. `[CreateAssetMenu]` |
| Domain | `BoardCoordinates` | `GridToWorld` / `WorldToGrid`, `IsInBounds`. 자석 = `(0,0)` 기준 순수 static 유틸 |
| Domain | `BoardGrid` | `Dictionary<Vector2Int, bool>` 점유 상태. 배열 없이 보드 밖 좌표도 키로 허용 (경계 판정은 `IsInBounds`) |
| Presentation | `BoardView` | `[SerializeField] BoardConfigSO` (Reflex X). `-half..half` 이중 루프로 셀 스프라이트 생성, 자석 칸만 색 교체 |

### 좌표 규칙

- 격자: 자석 = `(0, 0)`, N=9 → 유효 범위 `[-4 .. 4]`
- 월드: `(gx * cellSize, gy * cellSize)`, 자석 = `Vector2.zero`
- 보드 밖: `|gx| > N/2` 또는 `|gy| > N/2` → 경계 이탈 (game-over 구현에서 사용)

### 씬 구성

- `Phase0_Bootstrap` 씬에 `Board` 오브젝트 추가, 카메라 Orthographic

## 이 Phase 범위 밖

- 블록·흡착·폭발·회전 → 다른 구현 (SCRUM-18~21)
- `BoardGrid`를 뷰/게임플레이에 연결 → block-placement 이후

## 코드·에셋 — 어디를 보면 되나

| 보려는 것 | 경로 |
|-----------|------|
| 보드 설정 SO | `Scripts/Data/BoardConfigSO.cs` |
| 좌표 변환 | `Scripts/Domain/BoardCoordinates.cs` |
| 점유 Dictionary | `Scripts/Domain/BoardGrid.cs` |
| 격자 렌더 | `Scripts/Presentation/BoardView.cs` |
| 기본 설정 에셋 | `ScriptableObjects/DefaultBoardConfig.asset` |
| 씬 | `Scenes/Phase0_Bootstrap.unity` → `Board` |
