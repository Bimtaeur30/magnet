# Phase 27 — Unique Unlock 확정 + Unique Shape 가중

## 목표

Unique를 **「막힌 1 + 자유 2 → 둘로 라인클리어 후 막힌 피스 개방」** 으로 고정하고, Unique 전용 Shape 가중 추첨으로 원본 Unique에 가까운 피스 분포를 쓴다.

## 범위

1. `uniqueShapeWeights[1..42]` — Unique 샘플 전용. 기본=Unique 폴더 관측 빈도(0=제외). 작은 ㄱ·미관측 초소형/대각 0
2. `UniqueUnlockGenerator` — 가중 랜덤 트리플 × N. 중복 ID 허용. **강A(단독 언락 불가) 우선**, 없으면 기존처럼 둘로 언락만 되는 후보 채택
3. Orchestrator → 가중 배열 전달
4. 문서·tooltip·풀 에셋 시드

## 비범위

- Death% 게이트로 Unique 열기/닫기
- legal==1 강제수
- Unique 번들 리스트 복구
- Normal/Clean `shapeWeights` 변경

## 수락

- [x] Unique 생성: blocked 1 + unlock 2 placeable + 클리어≥1로 blocked 개방
- [x] 샘플 내에서 단독 언락 불가 후보를 우선 반환
- [x] weight 0 Shape는 Unique 추첨에 안 나옴
- [x] 컴파일 오류 없음
