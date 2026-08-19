# line-clear-preview-fx Sequence 7

> Phase 6 구현 기록 · 스킨 배치/클리어 사운드

## 1 — 2026-08-18 · SkinDataSO 사운드 슬롯 + 1발 재생

## 변경 상세

- 파일: `_Shared/Magnet.Core/SO/Skin/SkinDataSO.cs`
  - 심볼: `SkinDataSO.HintSound` — 프로퍼티 `SoundClipSO` (추가)
    - 설명: 클리어 예고가 처음 켜질 때 재생할 클립을 보관한다. 스킨 전체 1개이며 색 바리에이션과 무관하다.
    - 이유: 힌트 클립 Animation Event는 칸마다 겹치므로 스킨 슬롯에서 줄당 1발을 고르게.
  - 심볼: `SkinDataSO.LineClearSound` — 프로퍼티 `SoundClipSO` (추가)
    - 설명: 실제 라인클리어 때 재생할 클립을 보관한다. 비면 Bootstrap 전역 explode를 쓴다.
    - 이유: 클리어 파티클처럼 스킨마다 다른 터지는 소리를 에셋만으로 넣게.

- 파일: `Scripts/Presentation/LineClearHintEffector.cs`
  - 심볼: `LineClearHintEffector.soundChannel` — 필드 `EventChannelSO` (추가)
    - 설명: `PlaySoundEvent`를 올릴 사운드 채널을 보관한다.
    - 이유: 객체 간 통신은 EventChannelSO만 쓰고, SO는 Inject하지 않으므로 SerializeField가 필요해서.
  - 심볼: `LineClearHintEffector.Awake()` — 메서드 (수정)
    - 설명: `soundChannel` null Assert를 추가한다.
    - 이유: 채널이 빠진 채 힌트만 돌면 예고음이 영원히 안 나서.
  - 심볼: `LineClearHintEffector.SetHints(...)` — 메서드 (수정)
    - 설명: `SyncSet`이 새 힌트 세트를 시작했다고 하면 `_currentSkin.HintSound`를 1발 재생한다.
    - 이유: 같은 줄 드래그로 칸이 늘 때마다 다시 울리지 않게.
  - 심볼: `LineClearHintEffector.SyncSet(...)` — 메서드 (수정)
    - 설명: 제거 후 `current`가 비고 `desired`가 있으면 true를 반환한다. 기존 힌트 칸이 남아 있으면 false.
    - 이유: 줄 전환(겹침 없음)은 새 소리, 같은 줄 연장은 무음으로 구분하려고.
  - 심볼: `LineClearHintEffector.PlaySound(...)` — 메서드 (추가)
    - 설명: 채널과 클립이 있을 때만 `PlaySoundEvent.Init(clip)`을 Raise한다.
    - 이유: 슬롯이 빈 스킨은 시각 힌트만 하고 소리를 건너뛰게.
    - 영향: `SetHints`만 호출.

- 파일: `Scripts/Bootstrap/BoardPlacementBootstrap.cs`
  - 심볼: `BoardPlacementBootstrap.skinChannel` — 필드 `EventChannelSO` (추가)
    - 설명: 장착 스킨 이벤트를 구독할 채널을 보관한다.
    - 이유: 클리어음을 스킨 SO에서 읽으려면 현재 `SkinDataSO`가 필요해서.
  - 심볼: `BoardPlacementBootstrap._currentSkin` — 필드 `SkinDataSO` (추가)
    - 설명: 장착 중인 스킨을 캐시한다.
    - 이유: Place 순간마다 스킨 리스트를 찾지 않게.
  - 심볼: `BoardPlacementBootstrap.Awake()` — 메서드 (수정)
    - 설명: `skinChannel` null Assert를 추가한다.
    - 이유: 채널이 없으면 클리어음이 전역 fallback만 나와 스킨 슬롯이 죽은 것처럼 보여서.
  - 심볼: `BoardPlacementBootstrap.OnEnable()` — 메서드 (추가)
    - 설명: `SkinChangedEvent` / `SkinInitializedEvent`를 구독한다.
    - 이유: Effector·ExplosionPresenter와 같은 스킨 캐시 경로를 쓰려고.
  - 심볼: `BoardPlacementBootstrap.OnDisable()` — 메서드 (추가)
    - 설명: 스킨 리스너를 해제한다.
    - 이유: 비활성 Bootstrap이 이전 스킨 이벤트를 받지 않게.
  - 심볼: `BoardPlacementBootstrap.OnSkinChanged(...)` — 메서드 (추가)
    - 설명: `evt.CurrentSkin`을 `_currentSkin`에 넣는다.
    - 이유: 인벤토리에서 스킨을 바꾸면 다음 클리어부터 새 소리가 나게.
  - 심볼: `BoardPlacementBootstrap.OnSkinInitialized(...)` — 메서드 (추가)
    - 설명: `evt.Skin`을 `_currentSkin`에 넣는다.
    - 이유: 게임 시작 장착 스킨을 Place 전에 받아 두려고.
  - 심볼: `BoardPlacementBootstrap.PlaceBlock(...)` — 메서드 (수정)
    - 설명: 클리어가 있으면 `ResolveLineClearSound()` 결과를 재생한다. 배치음은 그대로다.
    - 이유: 스킨 클리어음으로 전역 explode를 덮되, 슬롯이 비면 기존 소리를 유지하려고.
  - 심볼: `BoardPlacementBootstrap.ResolveLineClearSound()` — 메서드 (추가)
    - 설명: `_currentSkin.LineClearSound`가 있으면 그것을, 없으면 `blockExplodeSound`를 반환한다.
    - 이유: 사운드 미지정 스킨이 무음이 되지 않게.
    - 영향: `PlaceBlock` 클리어 분기.

- 파일: `Prefabs/Bootstraps.prefab`
  - 심볼: `BoardPlacementBootstrap.skinChannel` — 직렬화 참조 (추가)
    - 설명: Skin Channel 에셋을 연결한다.
    - 이유: 런타임에 장착 스킨 이벤트를 받게.

- 파일: `Prefabs/Board/Placed Blocks View.prefab`
  - 심볼: `LineClearHintEffector.soundChannel` — 직렬화 참조 (추가)
    - 설명: Sound Channel 에셋을 연결한다.
    - 이유: 힌트음 Raise 대상이 필요해서.

- 파일: `Docs/SKIN.md`
  - 심볼: 스킨 제작 가이드 — 문서 (추가)
    - 설명: HintClips·파티클·HintSound/LineClearSound 넣는 법과 금지 사항을 적는다.
    - 이유: 다른 AI가 스킨을 코드로 구현하지 않고 SO 슬롯만 채우게.

- 파일: `Docs/INSPECTOR_TOOLTIPS.md`
  - 심볼: `HintClips` / `HintSound` / `LineClearSound` — 툴팁 행 (추가)
    - 설명: 인스펙터 필드 설명을 공용 표에 넣는다.
    - 이유: Tooltip을 코드에 추가하면 이 문서도 같이 갱신해야 해서.

## 2 — 2026-08-18 · 놨을 때 / 터질 때로 슬롯 정정

## 변경 상세

- 파일: `_Shared/Magnet.Core/SO/Skin/SkinDataSO.cs`
  - 심볼: `SkinDataSO.HintSound` — 프로퍼티 (삭제)
    - 설명: 예고 시작음 슬롯을 제거한다.
    - 이유: 사운드는 드래그 힌트가 아니라 배치와 클리어 두 시점만 필요해서.
  - 심볼: `SkinDataSO.PlaceSound` — 프로퍼티 `SoundClipSO` (추가)
    - 설명: 블록을 보드에 놨을 때 1발. 색 id 배열이 아니다. 이전 `HintSound` 직렬화 이름을 `FormerlySerializedAs`로 받는다.
    - 이유: 스킨당 배치음 하나면 되고, 이미 꽂아 둔 빈 슬롯이 인스펙터에서 사라지지 않게.
  - 심볼: `SkinDataSO.LineClearSound` — 프로퍼티 (수정)
    - 설명: 툴팁을 줄이 터질 때 1발, 색 id와 무관으로 고친다.
    - 이유: 슬롯 의미가 예고가 아니라 클리어임을 인스펙터에서 분명히 하려고.

- 파일: `Scripts/Bootstrap/BoardPlacementBootstrap.cs`
  - 심볼: `BoardPlacementBootstrap.PlaceBlock(...)` — 메서드 (수정)
    - 설명: 배치 성공 시 `ResolvePlaceSound()`, 클리어가 있으면 `ResolveLineClearSound()`를 재생한다.
    - 이유: 놨을 때와 터질 때 둘 다 스킨 슬롯을 타게.
  - 심볼: `BoardPlacementBootstrap.ResolvePlaceSound()` — 메서드 (추가)
    - 설명: `_currentSkin.PlaceSound`가 있으면 그것을, 없으면 `blockPlaceSound`를 반환한다.
    - 이유: 사운드 미지정 스킨이 배치 무음이 되지 않게.
    - 영향: `PlaceBlock`.

- 파일: `Scripts/Presentation/LineClearHintEffector.cs`
  - 심볼: `LineClearHintEffector.soundChannel` — 필드 (삭제)
    - 설명: 힌트음용 사운드 채널을 제거한다.
    - 이유: 예고 클립은 비주얼만 담당하고 소리는 Place에서 나기 때문에.
  - 심볼: `LineClearHintEffector.SetHints(...)` — 메서드 (수정)
    - 설명: `HintSound` 재생을 뺀다. 클립·스프라이트 힌트만 남긴다.
    - 이유: 드래그 중 힌트음이 배치음과 섞이지 않게.
  - 심볼: `LineClearHintEffector.SyncSet(...)` — 메서드 (수정)
    - 설명: `startedFresh` bool 반환을 없애고 다시 void로 둔다.
    - 이유: 힌트음 게이트가 필요 없어서.
  - 심볼: `LineClearHintEffector.PlaySound(...)` — 메서드 (삭제)
    - 설명: Effector의 사운드 Raise를 제거한다.
    - 이유: 재생 책임이 Bootstrap으로 옮아서.

- 파일: `Prefabs/Board/Placed Blocks View.prefab`
  - 심볼: `LineClearHintEffector.soundChannel` — 직렬화 참조 (삭제)
    - 설명: Sound Channel 배선을 뺀다.
    - 이유: 필드가 사라져서.

- 파일: `Docs/SKIN.md`
  - 심볼: `PlaceSound` / `LineClearSound` 슬롯 설명 — 문서 (수정)
    - 설명: 놨을 때·터질 때 두 슬롯, 색 id 배열이 아님을 적는다.
    - 이유: 다른 AI가 예고음이나 색별 사운드 배열을 만들지 않게.

