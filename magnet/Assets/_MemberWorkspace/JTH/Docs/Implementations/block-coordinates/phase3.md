# block-coordinates Phase 3 — Block Blast 8×8·자석 제거

> **구현:** `block-coordinates` · **Jira:** [SCRUM-17](https://bimtaeur30.atlassian.net/browse/SCRUM-17) · **마일스톤:** M1  
> **DESIGN:** v0.7 §4.1 · §6.2.2

## 목표 (완료 기준)

- [ ] `BoardConfigSO` 기본 `N = 8`
- [ ] 격자 `(0..N-1, 0..N-1)`. **자석 축·`(0,0)=중앙` 제거**
- [ ] `BoardCoordinates` — `GridToWorld` / `WorldToGrid` / `IsInBounds` Block Blast 좌표계
- [ ] `BoardView` — 자석 축 렌더 제거, 8×8 격자만
- [ ] magnet 중심 좌표에 의존하는 호출부 grep·목록 (Phase 4+에서 교체)

## 구현 내용

| 클래스 | 변경 |
|--------|------|
| `BoardConfigSO` | `BoardSize = 8` 기본 |
| `BoardCoordinates` | origin corner 기준 변환 (좌하 또는 좌상 — 구현 시 하나로 확정) |
| `BoardView` | 자석 축 표시 제거 |
| `BoardGrid` | bounds `[0, N-1]` |

## 범위 밖

- 2D 배치·line clear (각 구현 Phase)
- 스폰 알고리즘

## 코드·에셋

- `JTH/Scripts/Domain/BoardCoordinates.cs`
- `JTH/Scripts/Data/BoardConfigSO.cs`
- `JTH/Scripts/Presentation/BoardView.cs`
