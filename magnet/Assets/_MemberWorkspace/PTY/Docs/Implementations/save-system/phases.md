# 세이브 시스템 — Phase 인덱스

> **구현:** `save-system` · **Jira:** [SCRUM-28](https://bimtaeur30.atlassian.net/browse/SCRUM-28)

| Phase | 제목 | 계획 (뭘 어떻게) | 변경 기록 (Sequence) | 상태 |
|-------|------|------------------|----------------------|------|
| 1 | 구조 (로컬/클라우드 세이브 뼈대) | [phase1.md](phase1.md) | [sequence1.md](sequence1.md) | 완료 |

## 관련

- 이번 구현은 **구조(인터페이스·DI 배선·이벤트)만** 잡는다. 실제 저장 로직(JSON 직렬화, GPGS/Game Center API 호출, 충돌 병합)은 이후 담당자가 각 스텁(`NotImplementedException`)을 채운다.
- `Docs/DESIGN.md` 7절 "시스템" 역할(Save/Load, M7–M8, owner TBD)과 연결됨. 담당자 배정은 팀 결정 사항이라 이 구현에서는 변경하지 않음.
- JTH 소유 `MagnetGameEvents.cs`는 수정하지 않고, PTY 소유 `Scripts/Events/SaveEvents.cs`에 별도 이벤트를 정의함 (`EventChannelSO`는 Type 기준 라우팅이라 같은 `magnetGameChannel`에서 동작).
