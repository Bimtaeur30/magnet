# Phase 1 Sequence

## 3 — 2026-09-23 · Gradation 반복 및 콤보 월드 파티클

- 사용자 계획 승인(진행)에 따라 AllClearUIView의 바인딩된 TMP에서 UIEffect를 자동 탐색. gradationDuration(기본 1초) 동안 gradationOffset 0→1을 Linear/Restart 무한 반복. 시간 배율 무시, OnDisable에서 취소 및 0 초기화. 기존 등장/퇴장 모션과 독립적으로 동작.
- Magent.KTJ.asmdef에 Coffee.UIEffect 런타임 어셈블리 GUID 참조 추가.
- ComboUIView에 선택적 comboParticle 직렬화 필드 추가(사용자 명시 요청). ShowCombo로 UI 위치 반영 후 UI 피벗의 화면 좌표를 이벤트 위치의 카메라 깊이로 월드 변환. StopEmittingAndClear → 위치 설정 → Play 순으로 재생, 비활성화 시 정리.
- Inspector: ComboUIView의 Combo Particle에 씬의 ParticleSystem 인스턴스를 연결. 미지정은 생략. AllClearUIView의 Gradation Duration으로 반복 시간 조절. UIEffect는 바인딩된 텍스트와 같은 오브젝트에 있어야 함.
- Unity 재컴파일 오류 없음. 임시 오브젝트 Editor eval로 Offset 0.25초/1.25초 반복 값, 정리, 직교/원근 카메라의 Overlay 및 카메라 UI 좌표 변환, 파티클 재생/재시작, null 생략 검증 통과. 임시 오브젝트 제거.
- 실제 씬의 최종 시각 연출과 파티클 렌더링 순서는 미검증. 씬/프리팹 및 자동 생성 바인딩은 수정하지 않음.

## 2 — 2026-09-22 · 등장 탄성 복구 및 퇴장 트윈

- 사용자 수정 승인. OnAllClear의 시간 트윈을 Linear로 복구하고 등장 스케일에 OutElastic, 알파에 OutQuad 적용.
- 유지 후 tweenDuration 동안 스케일은 1 - InBack, 알파는 1 - InQuad로 퇴장한 뒤 초기화. 기본 총 시간은 1.5초.
- tweenDuration/visibleDuration Tooltip과 공용 KTJ 표 동기화.
- Unity 재컴파일 오류 없음. 임시 오브젝트의 ViewModel을 명시적으로 초기화한 Editor eval에서 등장 오버슈트, 등장 완료, 유지, 퇴장 페이드, 완료 초기화, 재수신 취소, 비활성화 초기화 통과. 임시 오브젝트 제거 완료.
- 실제 씬의 시각적 재생은 미검증.

## 1 — 2026-09-17 · 올클리어 텍스트 트윈과 선택적 파티클

- AllClearUIView: Awake/OnEnable 초기화, OnAllClear 이벤트 처리, StopAnimation 취소, ResetVisuals 초기화, OnDisable 정리 추가.
- TextTMPAlpha/TextTMPScale 기존 MVVM 바인딩 사용. 단일 LitMotion으로 등장 및 유지 시간을 관리.
- tweenDuration/visibleDuration/allClearParticle 직렬화 필드와 한국어 Tooltip 추가. 파티클 null은 정상 상태로 처리.
- Docs/INSPECTOR_TOOLTIPS.md KTJ 표 및 구현 인덱스/Phase 기록 갱신.
- 검증: Unity Editor eval의 상태/수명주기 검증 통과. 실제 씬 Play 및 파티클 연결 검증 대기.
