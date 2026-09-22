# Phase 1 Sequence

## 1 — 2026-09-23 · 칸 중심 및 화면 안전 영역

- 사용자 허용 후 JTH GameBoard.GridToWorldCenter의 Y축 반 칸 누락 수정.
- KTJ ComboUIView.OnComboChanged에서 좌표계 보정 및 ClampComboToScreen 호출. GetScreenBounds로 그래픽 표시 범위와 스케일 피벗 측정.
- ComboUIViewModel.ShowCombo에 선택적 fitScale 추가. SetComboScale에 배율 적용.
- 컴파일 및 Editor 수치 검사 152개 통과. 씬/프리팹 수정 없음. 임시 검증 오브젝트 제거.
