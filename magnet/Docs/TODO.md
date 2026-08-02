# TODO



각자 자기 섹션만 수정하세요. 팀 공통 작업은 `Common`에 적습니다.



## Common



- [ ] `Docs/DESIGN.md` v0.7 Block Blast 피벗 — Jira(SCRUM-17~23) 설명 동기화 (JTH)



## JTH



**역할:** 인게임 코어 로직 (Jira SCRUM 기준) · **UI/HUD/인벤토리 담당 아님**  

**Jira:** [SCRUM Backlog](https://bimtaeur30.atlassian.net/jira/software/projects/SCRUM/boards/1/backlog) · `hwanji203@gmail.com`  

**기획:** v0.7 Block Blast 피벗 (2026-07-21 grill-me). 스폰 알고리즘은 **`hybrid-spawn-algorithm`** (핸드오프 체인 + 특수 티어 병합, 2026-08-02 grill-me).



- [x] (공통) M0 — Reflect DI, EventChannelSO, asmdef/공유 폴더 합의 → `common-bootstrap`

- [ ] SCRUM-17 — 8×8 격자, 자석 축 제거, 좌표 단순화 (`block-coordinates` Phase 3)

- [ ] SCRUM-18 — BlockBlastPoolSO, 3슬롯·턴 리필 (`random-block-spawn` Phase 6). *스폰 알고리즘 TBD*

- [ ] SCRUM-19 — 2D 드래그·grid placement·BlockPool/ShapeAssembler (`block-placement` Phase 6)

- [ ] SCRUM-20 — Line clear 행·열 삭제·연쇄 (`line-clear` Phase 1). *clear-reassembly deprecated*

- [ ] SCRUM-22 — 3후보 전부 배치 불가 게임오버 (`game-over` Phase 1)

- [x] SCRUM-21 — ~~보드 90° 회전~~ → **Cancelled** (턴 FSM은 SCRUM-18)

- [ ] SCRUM-23 — line clear 점수·콤보 공식 (`score-logic` Phase 5)

- [x] `blocked-ring-dim` — **Deprecated** (v0.6 테두리 UX)



## KTJ



## PMS



## PTY



