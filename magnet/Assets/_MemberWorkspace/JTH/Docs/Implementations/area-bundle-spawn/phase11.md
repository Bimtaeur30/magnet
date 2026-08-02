# Phase 11 — Blocks2 Normal 번들 재구축

## 목표

데스크톱 `Blocks/Blocks2` 스크린샷(~347)을 42-ID 핸드로 읽어, Normal 풀에서 **관측되지 않은 패를 빼고** · **관측(및 large 포함) 패를 넣는다**. Unique는 동적 생성 유지.

## 정책

- 입력: L→R 3조각 ShapeId. ShapeId **1**(1x1)·**37+**(대각 Unique 계열) 포함 핸드는 제외.
- 유지: multiset **freq≥2** 또는 **large**(11–13, 21–24, 35–36) 포함(freq1도).
- weight = `clamp(count, 1..5)`. 순서는 스크린샷에서 가장 많이 나온 L→R.

## 결과

- [x] `AreaBundleStarterData.CreateNormal` — 59 → **195**
- [x] `DefaultAreaBundlePool` Fill Starter 동기화
- [x] Tooltip / phases / IMPLEMENTATIONS 갱신
