# Phase 1 — 토글 버튼 Inspector 지정

## 목표 및 완료 조건
- Inspector에서 토글을 실행할 Button을 지정한다.
- Additional Buttons 목록에 여러 버튼을 추가해 같은 대상과 상태를 토글한다.
- 빈 항목과 중복 버튼은 무시하며, 비활성화 시 실제 등록한 모든 버튼의 리스너를 해제한다.
- 미지정 시 같은 오브젝트의 Button을 사용한다.
- 버튼이 없으면 이벤트 등록·해제를 건너뛴다.

## 구현 및 범위
- `02_Script/UI/_etc/ToggleActiveBtn_UI.cs`: btn 직렬화, Awake 자동 연결 조건, OnEnable/OnDisable null 검사.
- `Docs/INSPECTOR_TOOLTIPS.md`: KTJ 버튼 필드 설명 기록.
- KTJ 구현 인덱스 및 Phase/Sequence 기록 갱신.
- 기존 RequireComponent, 토글 상태 및 이벤트 동작 유지. 씬·프리팹 수정 없음.
- 사용자 요청에 따라 Button 참조를 직렬화한다.

## 검증
- 지정한 버튼의 클릭으로 상태가 한 번 전환되는지 확인.
- 추가 버튼 각각의 클릭, 기본 버튼과 중복된 항목, 빈 항목 처리 확인.
- 미지정 시 기존 버튼 동작, 재활성화 후 리스너 중복 여부 확인.
- Unity 컴파일 및 Play Mode 확인은 미실행.
