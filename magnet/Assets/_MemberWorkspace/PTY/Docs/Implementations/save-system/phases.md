# 세이브 시스템 — Phase 인덱스

> **구현:** `save-system` · **Jira:** [SCRUM-28](https://bimtaeur30.atlassian.net/browse/SCRUM-28)

| Phase | 제목 | 계획 (뭘 어떻게) | 변경 기록 (Sequence) | 상태 |
|-------|------|------------------|----------------------|------|
| 1 | 구조 (로컬/클라우드 세이브 뼈대) | [phase1.md](phase1.md) | [sequence1.md](sequence1.md) | 완료 |
| 2 | 저장 항목 확장 (플레이 통계 + 장착 스킨) | [phase2.md](phase2.md) | [sequence2.md](sequence2.md) | 완료 |
| 3 | 클라우드 의존 제거, 로컬 저장 실제 구현 | [phase3.md](phase3.md) | [sequence3.md](sequence3.md) | 완료 |

## 관련

- Phase 1·2는 구조(인터페이스·DI 배선·이벤트)만 잡았고, Phase 3에서 로그인 미구현으로 클라우드 의존을 제거하며 로컬 저장(JSON 직렬화) 실제 로직을 구현했다. GPGS/Game Center 연동·충돌 병합은 로그인 기능이 생기면 이어서 구현할 자리로 남겨둠 (Phase 3 참고).
- `Docs/DESIGN.md` 7절 "시스템" 역할(Save/Load, M7–M8, owner TBD)과 연결됨. 담당자 배정은 팀 결정 사항이라 이 구현에서는 변경하지 않음.
- JTH 소유 `MagnetGameEvents.cs`는 수정하지 않고, PTY 소유 `Scripts/Events/SaveEvents.cs`에 별도 이벤트를 정의함 (`EventChannelSO`는 Type 기준 라우팅이라 같은 `magnetGameChannel`에서 동작).
