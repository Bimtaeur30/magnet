# Phase 1 — 부활 제안 표시와 선택 처리

2026-09-23 수정: No Thanks는 RelifeDeclinedEvent를 발행하고 TurnBootstrap의 공통 게임오버 경로에서 스킨 해금 검사 후 GameOverEvent를 발행한다. 기존 아래 초기 구현 설명의 직접 GameOverEvent 발행 방식은 더 이상 사용하지 않는다.

## 목표와 완료 기준

- `RelifeOfferedEvent` 수신 시 부활 Container를 활성화한다.
- 전달된 블록 셀 좌표를 `Container/BlockSlots` 아래에 조합 이미지로 표시한다.
- Sure 선택 시 `RelifeAcceptedEvent`, No Thanks 선택 시 현재 점수의 `GameOverEvent`를 전송한다.

## 구현 내용

- `ReviveUIView`가 버튼, Container, 이벤트 채널을 Inspector에서 참조한다.
- `ScoreChangedEvent`의 최신 점수를 보관해 게임 오버 이벤트에 전달한다.
- `BlockSlot_UI`와 같은 경계 계산 및 중앙 정렬 방식으로 셀 이미지를 동적 생성한다.
- 스킨 초기화·변경 이벤트를 받아 생성된 셀에 현재 스킨을 적용한다.

## 범위 밖

- 부활 판정 및 쉬운 블록 생성 로직
- 게임 오버 이벤트 데이터 구조 변경
- 씬 UI 배치와 연출 변경

## 코드·에셋 맵

- `02_Script/UI/ReviveUI/ReviveUIView.cs` — 이벤트, 버튼, 동적 블록 이미지 표시
