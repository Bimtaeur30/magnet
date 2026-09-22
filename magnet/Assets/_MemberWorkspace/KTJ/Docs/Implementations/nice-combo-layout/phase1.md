# Phase 1 — 따봉 칸 중심 및 콤보 화면 제한

- 추가 수정: 콤보 파티클은 Default 블록보다 앞인 ComboParticles 전용 Sorting Layer 사용. 자식 렌더러/그룹의 order=32767, 루트 그룹 sortAtRoot=true. 증가하는 블록 order와 독립적으로 정렬. Unity 컴파일/Editor 정렬 검사 통과.

- 승인: 2026-09-23 사용자 진행 승인. JTH GameBoard.cs 변경은 추가 질문에 사용자가 명시적으로 허용.
- 원인: UniqueCorrectPlacementEvent는 GameBoard.GridToWorldCenter를 사용하지만 기존 함수는 X축 반 칸만 더함. NiceIcon 피벗은 이미 중앙이므로 프리팹 수정 없이 좌표 원천 수정.
- GameBoard.GridToWorldCenter: 보드 로컬에서 X/Y 각각 0.5칸을 더한 뒤 월드 변환. 기존 2D 이벤트의 Z=0 유지. 이 함수를 사용하는 다른 칸 이펙트에도 적용.
- ComboUIView: 부모 로컬 위치와 anchoredPosition 차이를 보정. TMP glyph bounds와 다른 Graphic의 사각형, 스케일 피벗을 포함해 최대 스케일 1.11에서 화면 경계 측정. safeArea와 Canvas pixelRect 교집합에서 16픽셀 여백 유지. 너무 큰 UI는 축소해 맞춤.
- ComboUIViewModel: ShowCombo의 선택적 fitScale로 등장/퇴장 트윈 전체에 동일 축소 배율 적용. 콤보 파티클은 최종 제한된 UI 위치를 사용.
- 검증: Unity 컴파일 성공. 임시 오브젝트 Editor eval에서 콤보 24개 경계/스케일/과대 크기 검사와 칸 중심 128개 이동/회전/비균등 스케일 검사 통과. 실제 씬 시각 검증은 미실시.
