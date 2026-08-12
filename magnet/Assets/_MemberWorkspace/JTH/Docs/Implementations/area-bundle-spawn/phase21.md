# Phase 21 — Hospitality 구멍·윤곽 Exact 핏

## 목표

접대 기회를 **즉시 클리어 가능 피스**가 아니라, **윤곽이 n% 이상 찬 4-연결 빈 구멍에 Exact로 쏙 들어가는 피스**로 재정의한다.

## 규칙

1. 구멍 = 4-연결 빈 영역 (카탈로그 최대 피스 크기 초과는 제외)
2. 윤곽 = 구멍 밖 8이웃(보드 안만). `채움% = 점유/윤곽수`
3. `채움% ≥ hospitalityContourMinFill`(기본 0.7) + Exact 핏 ID 존재 → 자격 구멍
4. 구멍은 채움% 내림차순
5. Normal 번들 중 핏 슬롯 수 최대 → 동점이면 높은 구멍 커버 → 동점이면 예측 Area
6. `CanSurvive` 실패면 버림
7. 후보 확정 후 `hospitalityProbability`(기본 0.35) — 낙첨 시 이번 턴 Normal

## 구현

- `OpportunityDetector.FindQualifyingHoles` / Exact 핏 / 번들 비교
- `AreaBundlePoolSO.hospitalityContourMinFill` · `hospitalityProbability`
- `TrySelectHospitality` 재작성 + 확률 게이트
