# 스킨 아이콘 생성 — Phase 인덱스

> **구현:** `skin-icon-generation`

| Phase | 제목 | 계획 (뭘 어떻게) | 변경 기록 (Sequence) | 상태 |
|-------|------|------------------|----------------------|------|
| 1 | 스킨 변경 시 블록 아이콘 런타임 재생성 + UI 텍스처 타입 수정 | [phase1.md](phase1.md) | [sequence1.md](sequence1.md) | 완료 |

## 관련

- `SkinChangedRequestEvent`/`SkinChangedResponseEvent`는 다른 세션/팀원이 이미 `Assets/_Shared/Magnet.Core/Events/SkinEvents.cs`에 정의해둔 것을 그대로 사용(정의 자체는 수정 안 함).
- 스킨 이벤트 채널은 공용 `magnetGameChannel`이 아니라 별도 `Skin Channel.asset` — 헷갈리기 쉬운 부분, [sequence1.md](sequence1.md) 참고.
- `SkinChangedRequestEvent`를 실제로 raise하는 UI/호출부는 이번 구현 범위 밖.
