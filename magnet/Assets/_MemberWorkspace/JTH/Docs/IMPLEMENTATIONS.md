# JTH — 구현 인덱스



개인 작업은 **구현 → Phase → Sequence** 3단계로 기록한다.  

팀 로드맵은 `Docs/DESIGN.md` **v0.7 (Block Blast 피벗)** · 마일스톤 M0~M10.



→ 구현 선택 → `phases.md` → `phaseN.md`(계획) + `sequenceN.md`(변경 기록) → 표의 코드 경로 확인.



| 구현 (slug) | 제목 | Jira | Phase 인덱스 | 상태 |

|-------------|------|------|--------------|------|

| [common-bootstrap](./Implementations/common-bootstrap/phases.md) | 공통 기반 (Reflex·이벤트) | — | phase1 완료 | 구현됨 |

| [block-coordinates](./Implementations/block-coordinates/phases.md) | 8×8 보드·격자 좌표 | [SCRUM-17](https://bimtaeur30.atlassian.net/browse/SCRUM-17) | phase2 완료 → **phase3 Block Blast** | v0.6 구현됨 · **마이그레이션 대기** |

| [random-block-spawn](./Implementations/random-block-spawn/phases.md) | 블록 공급 (3슬롯) | [SCRUM-18](https://bimtaeur30.atlassian.net/browse/SCRUM-18) | phase5 완료 → **phase6 BlockBlastPoolSO** | v0.6 구현됨 · **풀 SO 교체 대기** |

| [block-placement](./Implementations/block-placement/phases.md) | 2D 배치·BlockPool | [SCRUM-19](https://bimtaeur30.atlassian.net/browse/SCRUM-19) | phase5 완료 → phase6 계획 · **phase7 동기화 수정 완료** | v0.6 자석 흡착 · **교체 대기** |

| [line-clear](./Implementations/line-clear/phases.md) | Line clear (행·열) | [SCRUM-20](https://bimtaeur30.atlassian.net/browse/SCRUM-20) | phase1 계획 | **신규** (clear-reassembly 대체) |

| [clear-reassembly](./Implementations/clear-reassembly/phases.md) | 클리어 재조립 (v0.6) | SCRUM-20 | phase9 완료 | **Deprecated** |

| [board-rotation](./Implementations/board-rotation/phases.md) | 보드 회전 (v0.6) | [SCRUM-21](https://bimtaeur30.atlassian.net/browse/SCRUM-21) | phase3 완료 | **Deprecated** |

| [block-destruction](./Implementations/block-destruction/phases.md) | 블록 파괴 (구) | SCRUM-20 | phase4 완료 | **Deprecated** |

| [blocked-ring-dim](./Implementations/blocked-ring-dim/phases.md) | 막힌 테두리 dim (v0.6) | — | phase3 완료 | **Deprecated** |

| [game-over](./Implementations/game-over/phases.md) | 게임 오버 (배치 불가) | [SCRUM-22](https://bimtaeur30.atlassian.net/browse/SCRUM-22) | phase1 계획 | 미착수 |

| [score-logic](./Implementations/score-logic/phases.md) | 점수·콤보 | [SCRUM-23](https://bimtaeur30.atlassian.net/browse/SCRUM-23) | phase5 완료 + sequence6 (콤보 0·GO 점수) | **구현됨** |

| [block-selection-algorithm](./Implementations/block-selection-algorithm/phases.md) | 블록 선택 알고리즘 (보드 상태 기반 스폰) | — | phase9 완료 | **Deprecated** (코드 제거 · Docs/`BLOCKBLAST_ANALYSIS`만 유지) |

| [blockblast-handoff-algorithm](./Implementations/blockblast-handoff-algorithm/phases.md) | BlockBlast 역공학 핸드오프 알고리즘 (42-ID · 7→1370→2100 체인) | — | phase1 완료 | **Deprecated** (알고리즘 코드 제거 · `BlockBlastCatalog`만 AreaBundle이 재사용) |

| [hybrid-spawn-algorithm](./Implementations/hybrid-spawn-algorithm/phases.md) | 하이브리드 스폰 (핸드오프 체인 기본 + 특수 티어 5종 게이트 · 42-ID 통일) | — | phase2 완료 | **Deprecated** (코드 제거 · area-bundle-spawn으로 교체) |

| [area-bundle-spawn](./Implementations/area-bundle-spawn/phases.md) | Area-번들 스폰 (히트맵 Normal · Unique occ≥40) | — | **phase40 완료** | **구현됨 (현재 배선)** |

| [line-clear-preview-fx](./Implementations/line-clear-preview-fx/phases.md) | 프리뷰 라인클리어 예고 (스킨 클립 + 클리어 이펙트 + 사운드) | — | **phase7 완료** | **구현됨** · 물방울 버스트 포함 · 스킨 제작은 `Docs/SKIN.md` |

| [relife](./Implementations/relife/phases.md) | Relife 오퍼·수락 (Easy 패 이어하기) | — | **phase1 완료** | **구현됨** |



**스폰 알고리즘** (보드 상태·난이도 곡선): DESIGN §4.9 → … → 현재 **`area-bundle-spawn`**(Unique + Normal 히트맵). `BlockBlastCatalog`·`PlacementSimulator` 공유.



**UI / HUD / 인벤토리 / 메뉴는 JTH 담당·Jira 범위 밖.**



## v0.7 구현 순서 (권장)



1. `block-coordinates` Phase 3 — 8×8, 자석 제거  

2. `block-placement` Phase 6 — 2D 배치 + BlockPool  

3. `line-clear` Phase 1 — line clear  

4. `board-rotation` deprecated 제거 + 턴 FSM 정리  

5. `score-logic` Phase 5 — 점수 공식  

6. `random-block-spawn` Phase 6 — BlockBlastPoolSO (알고리즘 X)  

7. `game-over` Phase 1  



## 계층 (용어)



| 용어 | 의미 | 파일 | 예 |

|------|------|------|-----|

| **구현** | 기능·Jira 이슈·요청 단위 | `Implementations/[slug]/` | line-clear, block-placement |

| **Phase** | 그 구현을 쪼갠 단계 | `phaseN.md` | grid placement |

| **Sequence** | Phase 변경 기록 | `sequenceN.md` | Block Blast 피벗 |



## 새 AI 세션



`IMPLEMENTATIONS.md` + **진행 중 구현**의 `phases.md` + 해당 **`phaseN.md`·`sequenceN.md`** 만 읽는다.



## 파일 형식



**`phaseN.md`** — 목표 · 구현 내용 · 범위 밖 · 코드·에셋 맵



**`sequenceN.md`** — Phase와 1:1 변경 기록 (`## N — 날짜 · 제목`)


