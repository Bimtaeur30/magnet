# Phase 1 — AllClearUIView 연출

- 승인: 2026-09-17 사용자 진행 승인 및 선택적 직렬화 파티클 추가 요청.
- 목표: AllClearEvent 수신 시 바인딩된 텍스트 알파/로컬 스케일을 0에서 1로 트윈하고, 완료 후 n초 유지 뒤 즉시 0으로 초기화.
- 구현: AllClearUIView.cs에서 EventChannelSO 구독/해제, LitMotion 단일 시간 트윈, OutQuad 보간, 재수신 시 취소 후 재시작. UI 시간은 timeScale에 영향받지 않음.
- Inspector: magnetGameChannel에 게임 이벤트 채널 연결. tweenDuration 기본 0.25초, visibleDuration 기본 1초. allClearParticle은 선택적 파티클 인스턴스이며 null 허용. 연결 시 이벤트와 함께 재생하고 초기화/비활성화 시 StopEmittingAndClear.
- 사용자 명시 요청에 따라 파티클은 SerializeField 사용.
- 범위: KTJ AllClearUIView 수동 파일, KTJ 구현 기록 및 공용 Tooltip의 KTJ 항목. 자동 생성 바인딩과 씬/프리팹 수정 없음.
- 검증: Unity 6000.3.10f1에서 갱신 후 실제 컴파일 타입을 Editor eval로 검증. 임시 오브젝트/채널 생성 후 제거. null 파티클, 초기값, 0.125초 alpha/scale 0.75, 0.25초 1, 1초 유지, 1.25초 초기화, 연속 이벤트의 기존 모션 취소, OnDisable 초기화 및 구독 해제 통과.
- 제한: Editor에서 MotionHandle.Time을 이동한 코드 검증이며 실제 씬 Play Mode/시각 검증 및 파티클 연결 재생 확인은 미실시.
