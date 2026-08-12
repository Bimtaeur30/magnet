# Phase 31 — Normal Area 라인클리어 필수

## 목표

Normal(및 Clean 체이닝이 쓰는 동일 후보 스코어)은 완주만으로는 부족하고, 추정 최선 시퀀스에 **라인 클리어 ≥1**이 있어야 한다. 없으면 Easy로 폴백한다.

## 범위

1. `ScoreSurvivors` — `SequenceFound && TotalClears ≥ 1`만 통과
2. Gate 로그 문구 갱신
3. Easy(`TrySelectByMaxArea`)는 클리어 비필수 유지

## 비범위

- Easy·Unique 클리어 강제
- 빔 폭·Death 배제 수치 변경

## 수락

- [x] Normal Area 후보에 클리어 0 패 미포함
- [x] 후보 없으면 Normal→Easy 폴백
- [x] Easy는 기존과 동일(클리어 비필수)
- [x] 컴파일 오류 없음
