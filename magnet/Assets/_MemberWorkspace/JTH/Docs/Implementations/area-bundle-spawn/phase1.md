# Phase 1 — Area 점수 도메인

## 목표

grill에서 확정한 Area 점수식을 순수 도메인으로 구현해, 이후 게이트·번들 선택이 동일 API를 쓰게 한다.

## 계획

1. `AreaScoreCalculator.Score(BoardGrid)` — 4-연결 찬/빈 컴포넌트 flood
2. 빈/찬 base 점수 + (점수≥0 찬) 변 보너스
3. `AreaScoreResult`로 total·컴포넌트별 내역 반환 (디버그·로그용)

## 비범위

- 번들 리스트·Drawer·Bootstrap 배선
- deathCount / fullSequence 선택기
