# line-clear Phase 1 — Line clear (행·열)

> **구현:** `line-clear` · **Jira:** [SCRUM-20](https://bimtaeur30.atlassian.net/browse/SCRUM-20) · **마일스톤:** M5  
> **DESIGN:** v0.7 §4.4

## 목표 (완료 기준)

- [ ] `LineClearDetector` — 꽉 찬 행·열 목록 반환
- [ ] `LineClearService` — 해당 칸 `BoardSession`에서 제거, 연쇄 until empty
- [ ] `BoardPlacementBootstrap` — Place 후 line clear chain (재조립·회전 **없음**)
- [ ] `PlacedBlocksView` — 삭제 칸 Block **pool return** (Shape 통째 삭제 X)
- [ ] `LineClearedEvent` Raise (줄 수, 점수 placeholder — score-logic Phase 5)
- [ ] FX: `PlayParticleEffectEvent` 재사용 가능

## 구현 내용

| 클래스 | 책임 |
|--------|------|
| `LineClearDetector` | full rows/cols on 8×8 |
| `LineClearService` | RemoveCells + chain |
| `BoardPlacementBootstrap` | 오케스트레이션 순서: Place → LineClear → Score → Consume |

## 범위 밖

- 점수 공식 상세 (`score-logic` Phase 5)
- 스폰 알고리즘

## Deprecated (호출 제거)

- `SquareClearDetector`, `ClearReassemblyService`, `BlockedRingDetector`

## 코드·에셋

- `JTH/Scripts/Domain/Clear/` (신규 Line*)
- `JTH/Scripts/Bootstrap/BoardPlacementBootstrap.cs`
- `JTH/Scripts/Presentation/PlacedBlocksView.cs`
