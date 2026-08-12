# Phase 33 — 올클 상태(빈 보드) Normal 가중랜덤

## 목표

보드가 **비어 있는 올클 상태**면 올클 Exact / 접대 / Clean Area를 타지 않고 Normal 번들 **가중 랜덤**으로 준다. (올클 *패를 준 다음 손*이 아니라, 빈 보드일 때의 지급 방식)

## 범위

1. `TrySelectNormalPriority` — `occupied == 0`이면 즉시 `SelectWeightedRandomNormal`
2. 올클 Exact 지급 플래그 방식 제거

## 비범위

- 시작 턴을 빈 보드 랜덤에서 제외
- Easy/Unique 강제 경로 변경

## 수락

- [x] 빈 보드 Select → Normal 가중랜덤
- [x] 칸이 있으면 기존 올클→접대→Area cascade
- [x] Gate 로그 `올클 상태(빈 보드) → Normal 가중랜덤`
- [x] 컴파일 오류 없음
