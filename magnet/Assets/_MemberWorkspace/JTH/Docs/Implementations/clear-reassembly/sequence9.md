# Sequence 9 — clear-reassembly Phase 9

## 1 — 2026-07-17 · 파괴 칸 PlayParticleEffectEvent 연결

**바뀐 것** — 수정: `Scripts/Presentation/PlacedBlocksView.cs`, `Scripts/Magnet.JTH.asmdef`, `Prefabs/Board.prefab`, `KTJ/.../_System.prefab`(PlacedBlocksView 직렬화만) · 생성: `phase9.md`, `sequence9.md`

**변경 상세 (왜/무엇)**  
- 파일: `Scripts/Presentation/PlacedBlocksView.cs`
  - 심볼: `PlacedBlocksView.magnetGameChannel` — 필드 `EventChannelSO` (추가)
    - 설명: `PlayParticleEffectEvent`를 raise할 메인 게임 채널을 Inspector에서 받는다.
    - 이유: SO는 SerializeField만 허용. ParticleEffectManager가 같은 채널을 구독한다.
  - 심볼: `PlacedBlocksView.blockBlastEffect` — 필드 `PoolItemSO` (추가)
    - 설명: 파괴 파편 파티클 풀 항목(`BlockBlast`) 참조.
    - 이유: 이벤트 Init에 넘길 Effect 에셋. View 책임으로 SerializeField에 둔다(grill A).
  - 심볼: `PlacedBlocksView._currentSkin` — 필드 `IBlockSkin` (추가)
    - 설명: 현재 적용 스킨을 캐시한다.
    - 이유: 파괴 시 `Sprite.texture`를 이벤트에 넣기 위해(grill 텍스처 A).
  - 심볼: `PlacedBlocksView._boardView` / `BoardView` — 필드·프로퍼티 (추가)
    - 설명: 부모 `BoardView`를 지연 캐시한다.
    - 이유: 바깥 방향 Quaternion을 보드 Transform 회전으로 월드 변환(grill 공간 B).
  - 심볼: `PlacedBlocksView.OnSkinInitialized` / `OnSkinChanged` — 메서드 (수정)
    - 설명: 스킨 적용 전에 `_currentSkin`을 갱신한다.
    - 이유: Destroy 시점에도 최신 스킨 텍스처를 쓰기 위해.
  - 심볼: `PlacedBlocksView.Awake` — 메서드 (수정)
    - 설명: `magnetGameChannel`·`blockBlastEffect` null Assert 추가.
    - 이유: 미할당 시 조용히 실패하지 않도록.
  - 심볼: `PlacedBlocksView.DestroyCellViews` — 메서드 (수정)
    - 설명: Destroy 전 칸마다 `PlayParticleEffectEvent.Init(effect, pos, rot, texture)` raise.
    - 이유: 파괴 연출 트리거 지점. Block이 아니라 View 일괄 파괴부(grill 합의).
    - 영향: `ParticleEffectManager`가 구독해 풀에서 재생.
  - 심볼: `PlacedBlocksView.GetOutwardWorldRotation` — 메서드 (추가)
    - 설명: 격자 바깥 방향 → 로컬 Quaternion(+Y 기준) → `BoardView.rotation` 합성.
    - 이유: 파티클 기본이 위쪽. 보드 회전이 있어도 시각적 바깥으로 쏘기 위해.
  - 심볼: `PlacedBlocksView.GetOutwardGridDirection` — 메서드 (추가)
    - 설명: 모서리는 대각, 변은 직교 바깥 벡터를 반환한다.
    - 이유: grill 방향 A(변=직교/모서리=대각).
  - 심볼: `PlacedBlocksView.Sign` — 메서드 (추가)
    - 설명: int 부호를 -1/0/1로 반환한다.
    - 이유: 바깥 방향 벡터 구성용.
- 파일: `Scripts/Magnet.JTH.asmdef`
  - 심볼: `references` — `ObjectPool.Runtime` GUID 추가
    - 설명: `PoolItemSO` 타입 참조를 허용한다.
    - 이유: `blockBlastEffect` SerializeField 컴파일에 필요.
- 파일: `Prefabs/Board.prefab` · `Assets/_MemberWorkspace/KTJ/06_Prefab/System/_System.prefab`
  - 심볼: `PlacedBlocksView.magnetGameChannel` / `blockBlastEffect` (+ `_System`의 `skinChannel`) — 직렬화 값 (추가)
    - 설명: MagnetGameChannel·BlockBlast(·SkinChannel) 에셋 할당.
    - 이유: Play 시 Assert·이벤트 동작에 필수. Main Installer는 `_System` View를 참조.

## 2 — 2026-07-17 · PresentationChannel 채널 불일치 수정

**바뀐 것** — 수정: `Scripts/Presentation/PlacedBlocksView.cs`, `Prefabs/Board.prefab`, `KTJ/.../_System.prefab`

**변경 상세 (왜/무엇)**  
- 파일: `Scripts/Presentation/PlacedBlocksView.cs`
  - 심볼: `PlacedBlocksView.magnetGameChannel` — 필드 (삭제) → `presentationChannel` (추가)
    - 설명: `PlayParticleEffectEvent` raise 채널을 `PresentationChannel`로 바꾼다.
    - 이유: `ParticleEffectManager`가 `presentationChannel`만 구독. magnetGameChannel로 보내면 리스너가 없어 파티클이 안 나옴.
  - 심볼: `PlacedBlocksView.DestroyCellViews` — 메서드 (수정)
    - 설명: `presentationChannel.RaiseEvent(...)`로 호출.
    - 이유: 구독 채널과 송신 채널을 일치시킨다.
- 파일: `Prefabs/Board.prefab` · `_System.prefab`
  - 심볼: `presentationChannel` 직렬화 — `PresentationChannel.asset` 할당
    - 설명: guid `80cb95b8...` PresentationChannel 연결.
    - 이유: 씬의 ParticleEffectManager와 동일 에셋이어야 함.

**메모** — 최초 요청에 PresentationChannel이 명시돼 있었으나 magnetGameChannel로 잘못 연결했음.

---
