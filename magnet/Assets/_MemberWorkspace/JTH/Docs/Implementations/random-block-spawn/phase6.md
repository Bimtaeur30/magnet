# random-block-spawn Phase 6 — BlockBlastPoolSO + 3슬롯



> **구현:** `random-block-spawn` · **Jira:** [SCRUM-18](https://bimtaeur30.atlassian.net/browse/SCRUM-18) · **마일스톤:** M2  

> **DESIGN:** v0.7 §4.2



## 목표 (완료 기준)



- [ ] `BlockBlastPoolSO` — Block Blast 표준 8종 (`1x1` … `Z4`) 에셋

- [ ] `BlockDrawer` — PTY `BlockShapeSourceSO` 대신 **BlockBlastPoolSO** 참조

- [ ] `BlockSupply.SlotCount = 3` 확정 (4슬롯 문서·코드 불일치 해소)

- [ ] 스폰 시 0/90/180/270 회전 (기존 Draw 로직 재사용)

- [ ] 턴 FSM: 3슬롯 전부 소진 → `Fill` · `TurnStarted`/`TurnEnded`



## 범위 밖



- **보드 상태 기반 스폰 알고리즘·난이도 곡선** (별도 설계 TBD)

- PTY BlockShape 에디터 수정



## 코드·에셋



- `JTH/Scripts/Data/BlockBlastPoolSO.cs` (신규)

- `JTH/Scripts/Domain/Spawn/BlockDrawer.cs`

- `JTH/Scripts/Domain/Spawn/BlockSupply.cs`

- `JTH/Scripts/Bootstrap/BlockSpawnBootstrap.cs`



## Block Blast 표준 풀 (canonical)



`1x1`, `1x2`, `1x3`, `2x2`, `L3`, `L4`, `T4`, `Z4` — DESIGN §4.2 표 참고.


