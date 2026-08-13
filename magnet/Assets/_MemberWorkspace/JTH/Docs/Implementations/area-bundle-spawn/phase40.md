# Phase 40 — 라인필 히트맵 스폰 (Unique/Normal 단순화)

## 목표

스폰 cascade를 **Unique(점유≥40) → Normal 히트맵 → Easy** 로 단순화한다.
접대·올클·Clean체인·Area MaxArea·완주·Death 배제를 선택에서 제거한다.

## 범위

1. `LineFillHeatmap` / `HeatmapHandScorer` — 행·열 `(n−empty)` 를 찬칸 인접 빈칸에만 가산, 손 점수=배치 칸 합(3! 순서).
2. `AreaBundleOrchestrator` — Unique/Normal/Easy만. dirty=`occupied ≥ UniqueMinOccupied`.
3. `AreaBundlePoolSO.UniqueMinOccupied=40`, `maxCandidatesToScore=64`.
4. Deal/HandCompare 라벨·점수를 히트맵 기준으로 맞춤.

## 비범위

- Clean/AllClear/Hospitality 리스트·튜닝 필드 삭제(미사용 잔존 OK)
- AreaScoreCalculator 자체 삭제(기즈모 등 잔존 가능)
- Unique unlock 알고리즘 변경
