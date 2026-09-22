# Phase 1 — 슬롯 블록 선택 가시성

- 승인: 사용자 진행 승인. KTJ 슬롯 UI, JTH BlockDragInput 및 공용 이벤트 변경 포함.
- 선택 확정 BlockSelectedEvent 수신 시 해당 슬롯의 동적 Image.enabled=false. 다른 슬롯은 복구. 슬롯 오브젝트/입력은 유지.
- 공용 BlockSelectionEndedEvent에 SlotIndex 전달. BlockDragInput.DisconnectSelection에서 데이터 정리 후 1회 발행. 배치 불가/탭/성공 및 비활성화 경로 포함.
- 성공 시 후보 갱신으로 소모 슬롯은 이미 비워지므로 이미지가 되살아나지 않음. 새 후보가 먼저 지급되어도 종료 이벤트로 표시 복구. 스킨 변경은 enabled를 변경하지 않음.
- BlockSlotContainer의 inGameChannel 직렬화 필드와 BlockSlotsUI.prefab 연결, Tooltip 기록 반영.
- 검증: Unity 컴파일 통과. Editor 임시 프리팹으로 선택/취소/전환/소모/재지급/채널 연결 검증, 임시 입력 컴포넌트로 종료 이벤트 슬롯 번호/중복 방지/비활성화 검증 통과. 실제 마우스 드래그 Play 검증은 미실시.
