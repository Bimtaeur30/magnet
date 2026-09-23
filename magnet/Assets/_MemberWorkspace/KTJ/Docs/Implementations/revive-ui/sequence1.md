# Phase 1 변경 기록

## 2 — 2026-09-23 · 부활 거절 후 스킨 획득 누락 수정

- ReviveUIView의 No Thanks 버튼이 GameOverEvent를 직접 발행하던 경로를 RelifeDeclinedEvent로 교체.
- TurnBootstrap이 거절 이벤트를 수신해 기존 RaiseGameOver 경로로 처리하므로 단계·점수 스킨 해금 검사 후 GameOverEvent를 발행.
- RelifeSession.Decline으로 중복 거절을 무시하고 대기 상태를 정리.
- NewNew_02_Main Play 모드에서 1900점 상태로 부활 제안의 No Thanks 클릭: 나무 스킨 해금, 획득 UI 활성화, 결과 UI 대기 확인. 저장 파일 변경 없음. 사용자도 실제 플레이에서 정상 표시 확인.

## 1 — 2026-08-16 · 부활 제안 UI 동작 연결

### 바뀐 것

- `ReviveUIView.cs` 수정
  - Sure/No Thanks 버튼과 Container 직렬화 참조 추가
  - 점수·부활·스킨 이벤트 구독 및 해제 추가
  - Relife 블록 조합 이미지 동적 생성·정리 추가
  - 수락 및 현재 점수 기반 게임 오버 이벤트 전송 추가
- `Docs/INSPECTOR_TOOLTIPS.md` KTJ 필드 목록 갱신

### 메모

- `Container` 바로 아래에 이름이 `BlockSlots`인 `RectTransform`이 있어야 한다.
- No Thanks 오브젝트에는 `Button` 컴포넌트가 필요하다.
