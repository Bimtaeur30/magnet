# Phase 1 Sequence

## 1 — 2026-09-17 · 올클리어 텍스트 트윈과 선택적 파티클

- AllClearUIView: Awake/OnEnable 초기화, OnAllClear 이벤트 처리, StopAnimation 취소, ResetVisuals 초기화, OnDisable 정리 추가.
- TextTMPAlpha/TextTMPScale 기존 MVVM 바인딩 사용. 단일 LitMotion으로 등장 및 유지 시간을 관리.
- tweenDuration/visibleDuration/allClearParticle 직렬화 필드와 한국어 Tooltip 추가. 파티클 null은 정상 상태로 처리.
- Docs/INSPECTOR_TOOLTIPS.md KTJ 표 및 구현 인덱스/Phase 기록 갱신.
- 검증: Unity Editor eval의 상태/수명주기 검증 통과. 실제 씬 Play 및 파티클 연결 검증 대기.
