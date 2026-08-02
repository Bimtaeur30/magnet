# block-placement Phase 6 — 2D 배치·BlockPool·ShapeAssembler



> **구현:** `block-placement` · **Jira:** [SCRUM-19](https://bimtaeur30.atlassian.net/browse/SCRUM-19) · **마일스톤:** M3  

> **DESIGN:** v0.7 §4.3 · §4.5 · §3



## 목표 (완료 기준)



- [ ] `GridPlacementValidator` — pivot + offsets, overlap/bounds ( `MagnetSnapSimulator` 대체 )

- [ ] `BlockDragInput` — 보드 위 **2D 드래그**, 그리드 스냅, 고스트 프리뷰

- [ ] `BlockPool` — `Block` 프리팹 1종 rent/return (`GameLib.ObjectPool`)

- [ ] `ShapeAssembler` — 풀 Block을 `IBlockShape.CellOffsets`에 맞게 조립 (드래그·슬롯·고스트)

- [ ] 부착 시 Block → `OccupiedCellView` 분리 (칸 단위)

- [ ] `BoardPlacementBootstrap` — 자석 스냅·회전 호출 제거, grid placement만



## 구현 내용



| 클래스 | 책임 |

|--------|------|

| `GridPlacementValidator` | TryPlace 시뮬 |

| `BlockPool` | Block rent/return |

| `ShapeAssembler` | ShapeBlock 역할 대체 또는 리팩터 |

| `BlockDragInput` | 2D input, ghost valid/invalid |

| `BoardPlacementBootstrap` | Place → (line clear는 line-clear 구현) |



## 범위 밖



- Line clear (`line-clear`)

- 스폰 알고리즘

- 점수 공식 변경



## Deprecated 제거



- `MagnetSnapSimulator`

- X축-only 드래그·Y스냅 연출



## 코드·에셋



- `JTH/Scripts/Domain/Placement/`

- `JTH/Scripts/Input/BlockDragInput.cs`

- `JTH/Scripts/Presentation/ShapeBlock.cs` → ShapeAssembler


