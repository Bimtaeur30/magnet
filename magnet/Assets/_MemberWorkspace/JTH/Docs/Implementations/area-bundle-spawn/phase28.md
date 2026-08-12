# Phase 28 — 패 선택 Explain 기즈모

## 목표

방금 뽑은 패를 **시뮬로 직접 넣어서** 고른 이유(MaxArea 우승 수 / Unique 언락 수)를 Scene 기즈모 칸으로만 표시한다. 텍스트 라벨 없음.

## 범위

1. `AreaBundleExplainStep` — 피스 슬롯·피벗·절대 칸
2. `AreaBundleMetrics.TryGetBestSequenceExplain` — MaxArea 우승 경로 스텝 캡처
3. `UniqueUnlockGenerator` — 언락 2수 + 막힌 피스 개방 배치 스텝
4. `AreaBundleSelectionResult.ExplainSteps`
5. `AreaBundleSelectionGizmo` — Play 중 OnDrawGizmos 칸 오버레이 (스텝별 색, Unique blocked=빨강)

## 비범위

- Handles.Label / Reason 텍스트 기즈모
- Death%·게이트 로그 UI화
- 플레이어 입력 하이라이트와 연동

## 수락

- [x] Play 중 Scene 뷰에서 최근 패의 시뮬 배치 칸이 색으로 보임
- [x] Unique: 언락 배치 + 막힌 피스 개방 칸 표시
- [x] Normal/Easy/Hospitality/AllClear: MaxArea 우승 시퀀스 칸 표시
- [x] 컴파일 오류 없음
