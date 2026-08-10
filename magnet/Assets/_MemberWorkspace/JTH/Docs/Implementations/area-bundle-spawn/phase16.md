# Phase 16 — Area 개수 패널티 제거

## 목표

직사각 개수 패널티와 역할이 겹치는 4-연결 Area 개수 패널티를 점수식에서 뺀다.

## 구현 내용

- `AreaScoreTuning.areaCountPenalty` 삭제
- `AreaScoreResult`의 `AreaCount` / `AreaCountPenalty` 삭제
- `Total = base − rectPenalty` 만 유지
- 풀 에셋·툴팁·튜닝 문서 동기화

## 범위 밖

- size/변 base, 직사각 패널티, Unique/Clear Priority 게이트 변경 없음
