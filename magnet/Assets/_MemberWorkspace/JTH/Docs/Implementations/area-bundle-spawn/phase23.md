# Phase 23 — ShapeId 가중 × Area

## 목표

Normal/Easy maxArea 우승에 ShapeId(1–42) 가중을 곱해, 플레이 피드백으로 모양 빈도를 직접 조절할 수 있게 한다.

## 규칙

1. `AreaBundlePoolSO.shapeWeights` — 인덱스=ShapeId, `[0]` 미사용, 기본 **1**
2. Normal·Easy: `effective = predictedArea × mean(w(id0), w(id1), w(id2))` → 최댓값
3. 전부 1이면 Phase22와 동일
4. **미적용:** 접대 Exact · 올클 Exact · Unique

## 비범위

- 유일수 튜닝
- 접대 모양별 FitWeight 재정의
- 번들 `weight`(관측횟수) 역할 변경
