# Phase 1 변경 기록

## 1 — 2026-09-16 · 얼음 전용 결정 파편 제작

**바뀐 것**

- `Assets/_MemberWorkspace/PTY/IceSkin/`에 전용 셰이더, 색상별 머티리얼, 8개 burst 프리팹과 PoolItemSO를 생성했다.
- `IceCrystalBurst.shader`는 각진 이중 결정, 청록 림광, 짧은 백색 스파클을 렌더한다.
- `Assets/_Shared/ScriptableObjects/Skins/Ice.asset`와 `Assets/GameLib/ObjectPool/PoolManager.asset`의 얼음 PoolItemSO 연결을 새 PTY 에셋으로 교체했다.
- HLSL 예약어 `point`가 D3D11 컴파일 오류를 내므로 셰이더의 변수명을 `p`로 수정했다.

**검증**

- YAML 참조 체인(스킨 → PoolItemSO → 프리팹 → 머티리얼 → 셰이더)과 신규 GUID 중복 여부를 정적 확인했다.
- 현재 자동화 표면에 Unity Editor가 노출되지 않아 셰이더 컴파일·게임 내 시각 재생은 Unity에서 확인 대기다.
