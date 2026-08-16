# Phase 1 변경 기록

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
