# Phase 9 — Normal 올클·멀티클리어 우선 선택

## 목표

깔짝깔짝 Area 최대만으로는 시원한 폭발·올클이 거의 안 나온다. Normal 경로에 **올클 최우선 → ≥3줄 클리어 최대 → Area** 순을 넣는다.

## grill 확정 (2026-08-02)

- Unique가 먼저 (dirty). 올클은 Unique 실패/스킵 후.
- 올클: Normal 후보 중 3피스 완주 후 보드 비움 → **75%** 지급. 낙첨 시 올클 후보 **제외**.
- 빈 보드(`occupied==0`)·올클 지급 후 **쿨다운 1턴** → 올클 검사 스킵 (무한 올클 방지).
- ≥4줄: **50%** 클리어 최대 / ≥5줄: **100%**. 낙첨 시 Area(후보 유지).
- 풀은 **별도 없이 Normal**. Easy/Relife는 Area만 유지.

## 결과

- [x] `AreaBundlePoolSO` — `allClearProbability`·`allClearCooldownTurns`·멀티 soft/hard·`outcomeBeamWidth`
- [x] `AreaBundleTier.AllClear` / `MultiClear`
- [x] `AreaBundleOrchestrator.TrySelectNormalPriority` — 빔 추정 + 우선순위
- [x] Bootstrap 로그 티어 색상
- [x] Tooltip / phases / sequence 기록

## 비범위

- Easy·Unique 선택식 변경
- 올클 전용 번들 풀 / Momentum 번들 이식
- 직전 턴 실클리어 칸 수 기반 게이트 (쿨다운·빈보드로 대체)
