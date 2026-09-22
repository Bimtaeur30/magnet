# Phase 1 Sequence

## 1 — 2026-09-23 · 선택 슬롯 숨김과 복구

- InGameEvents: BlockSelectionEndedEvent 추가.
- BlockDragInput: DisconnectSelection 종료 이벤트 발행 및 OnDisable 정리.
- BlockSlotContainer: 선택/종료 구독과 슬롯 표시 전환.
- BlockSlot_UI: SetSelectionHidden 및 재생성 셀의 표시 상태 유지.
- BlockSlotsUI.prefab: 입력과 동일한 InGameChannel 연결.
- Unity 컴파일 및 Editor 코드 검증 통과. 실제 Play 입력 검증 대기.
