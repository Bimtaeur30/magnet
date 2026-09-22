# Phase 1 Sequence

## 2 — 2026-09-23 · 콤보 파티클 정렬

- 블록 order가 _nextMaskSlot++로 증가하므로 고정 order만으로 우선순위를 보장할 수 없음. ProjectSettings/TagManager.asset에 Default보다 앞인 ComboParticles Sorting Layer 추가.
- ComboUIView.SetParticleSorting에서 모든 자식 ParticleSystemRenderer와 SortingGroup에 전용 레이어 및 order 32767 적용. 루트 SortingGroup을 생성/재사용하고 sortAtRoot로 부모 그룹의 정렬 영향에서 분리. 재생 전에 적용.
- Unity 컴파일 및 임시 오브젝트 Editor eval에서 레이어 우선순위, 자식 렌더러, 독립 루트 그룹 검증 통과. 실제 화면 검증 대기.

## 1 — 2026-09-23 · 칸 중심 및 화면 안전 영역

- 사용자 허용 후 JTH GameBoard.GridToWorldCenter의 Y축 반 칸 누락 수정.
- KTJ ComboUIView.OnComboChanged에서 좌표계 보정 및 ClampComboToScreen 호출. GetScreenBounds로 그래픽 표시 범위와 스케일 피벗 측정.
- ComboUIViewModel.ShowCombo에 선택적 fitScale 추가. SetComboScale에 배율 적용.
- 컴파일 및 Editor 수치 검사 152개 통과. 씬/프리팹 수정 없음. 임시 검증 오브젝트 제거.
