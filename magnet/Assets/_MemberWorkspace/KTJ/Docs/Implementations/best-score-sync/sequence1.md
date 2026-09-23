# Phase 1 변경 기록

## 1 — 2026-09-23 · 최고 점수 재활성화 동기화

- `ScoreUIView.Awake()` 오버라이드 삭제: 최초 생성 시에만 실행하던 최고 점수 초기화를 이동했다. 기본 MvvmView의 Awake는 그대로 실행된다.
- `ScoreUIView.OnEnable()` 수정: 이벤트 구독 후 ISaveService.BestScore를 `_displayedBestScore`와 ViewModel에 적용한다. 비활성 중 놓친 이벤트 및 중단된 애니메이션으로 남은 이전 값을 복구하기 위함이다.
- 검증: Unity 재컴파일 오류 0. Editor 임시 코드에서 100 초기화 → 비활성 중 250 저장 → 재활성화 250, 150 제출 후 250 유지, 500 저장 후 중간 표시 251 → 재활성화 500, 메모리 저장소 재로드 500을 확인했다.
- 한계: 실제 Play Mode 및 사용자 게임오버 화면은 미확인. 임시 검증 객체와 코드는 정리했다.
