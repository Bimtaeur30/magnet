# line-clear-preview-fx Sequence 1

> Phase 1 구현 기록 · 프리뷰 라인클리어 림·스파크

## 변경 상세

- 파일: `Scripts/Domain/Clear/LineClearPreviewDetector.cs`
  - 심볼: `LineClearPreviewDetector.Detect(...)` — 메서드 (추가)
    - 설명: `BoardGrid.Clone()` 후 피벗에 블록을 가상 점유하고 `LineClearDetector.Detect`로 클리어 라인을 반환한다.
    - 이유: 드래그 프리뷰 중 실보드를 오염시키지 않고 클리어 예고를 판정하기 위해.

- 파일: `Scripts/Domain/Clear/ClearedRegionPerimeter.cs`
  - 심볼: `ClearedRegionPerimeter.GridEdge` — 중첩 타입 (추가)
    - 설명: 그리드 코너 좌표의 시작·끝·바깥 법선을 담는 읽기 전용 세그먼트.
    - 이유: 도메인 단위로 외곽을 넘기고 Presentation에서 월드 변환만 하게 분리.
  - 심볼: `ClearedRegionPerimeter.Build(...)` — 메서드 (추가)
    - 설명: 클리어 칸 합집합에서 이웃이 없는 변을 모아 같은 행/열의 연속 구간을 긴 엣지로 병합한다.
    - 이유: 레퍼런스처럼 합집합 바깥 테두리 하나에 림을 붙이기 위해.
  - 심볼: `ClearedRegionPerimeter.MergeHorizontal(...)` — 메서드 (추가)
    - 설명: North/South 변을 행별로 정렬·런 병합한다.
    - 이유: 가로 림 세그먼트 생성.
  - 심볼: `ClearedRegionPerimeter.MergeVertical(...)` — 메서드 (추가)
    - 설명: East/West 변을 열별로 정렬·런 병합한다.
    - 이유: 세로 림 세그먼트 생성.

- 파일: `Scripts/Data/LineClearPreviewConfigSO.cs`
  - 심볼: `LineClearPreviewConfigSO.EffectItem` — 프로퍼티 (추가)
    - 설명: 풀링할 `PoolItemSO` 참조.
    - 이유: EffectManager가 Pop할 프리팹을 인스펙터에서 고르게.
  - 심볼: `LineClearPreviewConfigSO.Color` — 프로퍼티 (추가)
    - 설명: 림·스파크 색(기본 흰색, 그라데이션 없음).
    - 이유: 색을 코드 수정 없이 바꿀 수 있게.
  - 심볼: `LineClearPreviewConfigSO.RimThickness` — 프로퍼티 (추가)
    - 설명: 늘린 원형 림 두께(월드).
    - 이유: 가장자리 두께 튜닝.
  - 심볼: `LineClearPreviewConfigSO.RimOutset` — 프로퍼티 (추가)
    - 설명: 테두리에서 바깥으로 미는 거리.
    - 이유: 블록 면에 파묻히지 않게.
  - 심볼: `LineClearPreviewConfigSO.PulseMinAlpha` / `PulseMaxAlpha` / `PulsePeriod` — 프로퍼티 (추가)
    - 설명: 림 숨쉬기 알파 범위와 주기.
    - 이유: 약한 호흡 연출 튜닝.
  - 심볼: `LineClearPreviewConfigSO.StripSizeMultiplier` — 프로퍼티 (추가)
    - 설명: 줄 네모 크기 배율 (1=칸 동일).
    - 이유: 블록과 같은 크기면 잘 안 보여서 살짝 키울 수 있게.

- 파일: `Scripts/Data/PlacementConfigSO.cs`
  - 심볼: `PlacementConfigSO.LineClearPreview` — 프로퍼티 (추가)
    - 설명: 라인클리어 프리뷰 설정 SO 참조.
    - 이유: 배치 설정 묶음에서 프리뷰 FX 설정을 함께 주입.

- 파일: `Scripts/Input/BlockDragInput.cs`
  - 심볼: `BlockDragInput.presentationChannel` — 필드 (추가)
    - 설명: PresentationChannel에 프리뷰 이펙트 이벤트를 Raise한다.
    - 이유: ParticleEffectManager가 구독하는 채널과 맞춘다.
  - 심볼: `BlockDragInput._lineClearPreviewActive` — 필드 (추가)
    - 설명: 현재 Hide 중복 Raise를 막기 위한 활성 플래그.
    - 이유: 스냅 해제 매 프레임 Hide 스팸 방지.
  - 심볼: `BlockDragInput.UpdateLineClearPreview(...)` — 메서드 (추가)
    - 설명: 시뮬→외곽→월드 엣지 변환 후 `LineClearPreviewEffectEvent` Raise.
    - 이유: 스냅 위치에서만 클리어 예고 이펙트를 켠다.
  - 심볼: `BlockDragInput.HideLineClearPreview()` — 메서드 (추가)
    - 설명: InitHide Raise로 이펙트를 끈다.
    - 이유: 스냅 해제·선택 해제·OnDisable 시 정리.
  - 심볼: `BlockDragInput.UpdateViews()` — 메서드 (수정)
    - 설명: 스냅 성공 시 `UpdateLineClearPreview`, 실패 시 Hide 호출을 추가.
    - 이유: 프리뷰와 이펙트 수명을 동기화.
  - 심볼: `BlockDragInput.DisconnectSelection()` / `OnBlockSelected` / `OnDisable` — 메서드 (수정)
    - 설명: 선택 끊김·재선택·비활성 시 Hide 호출.
    - 이유: 고스트가 사라질 때 림이 남지 않게.

- 파일: `_Shared/Magnet.Core/Events/PresentationEvents.cs`
  - 심볼: `PresentationEvents.LineClearPreviewEffectEvent` — 필드 (추가)
    - 설명: 재사용 이벤트 싱글톤.
    - 이유: EventChannel Type 라우팅용.
  - 심볼: `LineClearPreviewEffectEvent` — 클래스 (추가)
    - 설명: Effect·Edges·색·림/스파크 파라미터. Edges 비면 Hide.
    - 이유: 유지형 프리뷰를 원샷 PlayParticle과 다른 페이로드로 전달.
  - 심볼: `LineClearPreviewEffectEvent.Init(...)` / `InitHide()` / `IsHide` — 멤버 (추가)
    - 설명: Show 파라미터 채우기 / Hide 초기화 / Hide 판정.
    - 이유: Raise 시 `new` 금지 패턴 유지.
  - 심볼: `LineClearPreviewEdge` — 구조체 (추가)
    - 설명: 월드 Start/End/Outward.
    - 이유: Shared 이벤트에 보드 도메인을 넣지 않고 월드 기하만 전달.

- 파일: `PTY/Scripts/Vfx/PooledLineClearPreviewEffect.cs`
  - 심볼: `PooledLineClearPreviewEffect.Show(...)` — 메서드 (추가)
    - 설명: 이벤트 파라미터로 림 배치·스파크 설정을 적용하고 활성 루프를 켠다.
    - 이유: EffectManager가 Pop한 인스턴스를 유지형으로 재생.
  - 심볼: `PooledLineClearPreviewEffect.HideAndClear()` — 메서드 (추가)
    - 설명: 림·파티클을 끄고 엣지 목록을 비운다.
    - 이유: Push 전 상태 리셋.
  - 심볼: `PooledLineClearPreviewEffect.ResetItem()` — 메서드 (추가)
    - 설명: HideAndClear + parent 해제.
    - 이유: `AbstractMonoPoolable` 풀 반환 계약.
  - 심볼: `PooledLineClearPreviewEffect.UpdatePulse()` — 메서드 (추가)
    - 설명: sine으로 림 알파를 min~max 왕복.
    - 이유: 살짝 숨쉬는 림.
  - 심볼: `PooledLineClearPreviewEffect.PlaceStrip(...)` / `SetStripCount(...)` — 메서드 (추가)
    - 설명: 클리어 줄마다 네모 스프라이트를 Size에 맞춰 배치. 부족하면 프로토타입 복제.
    - 이유: 풀 프리팹 아래 Strips 구조.

- 파일: `PTY/Scripts/Vfx/ParticleEffectManager.cs`
  - 심볼: `ParticleEffectManager._activeLineClearPreview` — 필드 (추가)
    - 설명: 현재 유지 중인 프리뷰 인스턴스.
    - 이유: 드래그 중 Pop을 한 번만 하고 Update는 Show 재호출.
  - 심볼: `ParticleEffectManager.HandleLineClearPreviewEffect(...)` — 메서드 (추가)
    - 설명: Hide면 Push, Show면 Pop(필요 시)+Show.
    - 이유: 채널 → 풀 호출 진입점.
  - 심볼: `ParticleEffectManager.ReleaseLineClearPreview()` — 메서드 (추가)
    - 설명: 활성 인스턴스 Hide 후 Push.
    - 이유: OnDisable·Hide 이벤트에서 누수 방지.
  - 심볼: `ParticleEffectManager.OnEnable` / `OnDisable` — 메서드 (수정)
    - 설명: `LineClearPreviewEffectEvent` 구독/해제 추가.
    - 이유: 새 이벤트 배선.

## 에셋

- `PTY/Prefabs/Vfx/LineClearPreviewEffect.prefab` — Strips(네모 광원) 풀 프리팹
- `PTY/Prefabs/Vfx/LineClearSoftSquare.png` — soft-edge 정사각 스프라이트
- `GameLib/ObjectPool/Items/LineClearPreview.asset` — PoolItem (PoolManager 등록)
- `JTH/ScriptableObjects/DefaultLineClearPreviewConfig.asset` — 기본 흰색 설정
- `DefaultPlacementConfig` / `Input.prefab` — LineClearPreview·presentationChannel 배선

## Sequence 1 보완 — 네모 스트립

- 늘린 원형 림 → 클리어 줄마다 **1×8 / 8×1 네모**를 블록보다 낮은 `sortingOrder`로 깔아 빛나게 변경.
- 이벤트 페이로드 `Edges` → `Strips` (`LineClearPreviewStrip`: Center, Size, IsRow).
- Sparks 파티클 제거 (추후 추가).
