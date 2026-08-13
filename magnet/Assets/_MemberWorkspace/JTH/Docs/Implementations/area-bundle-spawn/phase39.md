# Phase 39 — Unique 4칸 균형 + 손 최적 배치 기즈모

## 목표

1. Unique가 너무 작아 해가 많거나, 너무 커서 자리가 뻔하지 않게 **4칸 테트로미노 중심**으로 재가중.
2. 기존 지급 Explain(`#1…`) 기즈모는 유지하고, **현재 손에 남은 블럭**의 MaxArea 최적 배치(`H1…`)를 추가로 표시.

## 범위

- `DefaultUniqueShapeWeight` / `uniqueShapeWeights` 에셋
- `AreaBundleSelectionGizmo` live hand overlay (+ `areaBundlePool` 참조)

## 비범위

- Unique unlock 알고리즘 자체 변경
- Death% 배제 로직 변경
