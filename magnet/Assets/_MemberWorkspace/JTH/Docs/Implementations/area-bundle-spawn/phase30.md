# Phase 30 — Clean 체이닝 클리어 폴백

## 목표

Clean Normal이 예약한 다음 패를 지급할 때, **현재 보드**에서 그 패로 라인 클리어가 한 줄도 안 되면 예약을 버리고 평소처럼 다시 뽑는다.

## 범위

1. `Select` — 큐 소비 직전 `SequenceOutcomeEstimator`로 클리어≥1 검사
2. 불가면 Gate 로그 후 일반 cascade(Unique/Normal/Easy)로 진행
3. 가능하면 기존처럼 예약 패 지급

## 비범위

- 예약 시점(큐잉)에 클리어 필터를 미리 걸기
- Explain 스텝을 현재 보드 기준으로 재캡처
- Clean 체이닝 확률·가중 변경

## 수락

- [x] 예약 패가 현재 보드에서 TotalClears≥1이면 지급
- [x] 아니면 큐 비우고 일반 `Select` 경로와 동일하게 뽑기
- [x] Gate 로그에 지급/폐기 구분
- [x] 컴파일 오류 없음
