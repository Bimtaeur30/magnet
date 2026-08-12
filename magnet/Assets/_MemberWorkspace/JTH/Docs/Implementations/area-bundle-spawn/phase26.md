# Phase 26 — Normal Clean / Main 2모드 + Clean 체이닝

## 목표

보드 Area에 따라 Normal Area 가중 프로파일을 나눈다.

- **Clean** (`boardArea > survivalAreaMax`): `cleanShapeWeights`(기본 대부분 1·작은 ㄱ 0) — 올클 친화
- **Main** (`boardArea ≤ survivalAreaMax`): `shapeWeights`(현재 생존 튜닝)

Clean Normal Area 지급 시 `cleanChainProbability`로 최적 시퀀스 보드에서 다음 패를 예약한다.

## 디버그

`[AreaBundle:Gate]` 로그를 **한 줄씩** 분리:
- 올클/접대 확률 통과·낙첨
- Clean/Main 모드
- Clean 체이닝 통과·낙첨·예약
