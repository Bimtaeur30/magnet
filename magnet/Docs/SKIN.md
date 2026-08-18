# 스킨 제작 가이드

인게임 칸 비주얼·클리어 예고·클리어 FX·사운드는 모두 `SkinDataSO`에 넣는다. **새 스킨은 코드 없이 에셋만** 추가한다.

에셋 위치: `Assets/_Shared/ScriptableObjects/Skins/`  
메뉴: `Create > Skin > SkinData`

구현 코드: `line-clear-preview-fx` (`JTH/Docs/Implementations/line-clear-preview-fx/`).

---

## 슬롯

| 필드 | 단위 | 비면 |
|------|------|------|
| `Sprites` | 색 바리에이션 | 스킨 없음 |
| `HintClips` | `Sprites`와 같은 인덱스 | 그 색은 예고 애니메이션 스킵 (스프라이트만) |
| `LineClearEffects` | 같은 인덱스, `PoolItemSO` | 그 색은 칸 파티클 스킵 |
| `FireCenteredLineClear` + `CenterLineClearEffect` | 스킨 전체 | 칸마다 안 쏘고 줄 가운데 1발 |
| `PlaceSound` | 스킨 전체, `SoundClipSO` | 전역 `blockPlaceSound` |
| `LineClearSound` | 스킨 전체, `SoundClipSO` | 전역 `blockExplodeSound` |

사운드는 **색 id별로 나누지 않는다.** 스킨당 배치 1개 + 클리어 1개.

같은 스킨의 7색은 보통 **같은 HintClip**을 공유한다. 색마다 다른 클립이 필요하면 슬롯만 다르게 넣는다.

---

## 클리어 예고 (Hint)

1. `AnimationClip`을 만든다. 타깃은 Block Skin의 Animator (Playable 출력).
2. 쩌적이면 `BlockShatterHint.shatter` (0→1)를 키프레임한다. 셰이더 `_Shatter`를 클립에 직접 넣지 않는다. `LateUpdate`가 MPB로 넣는다.
3. 스케일이면 Transform만 키프레임한다. 예: `DefaultHintShatter.anim`, `JellyHintBoing.anim`.
4. `SkinDataSO.HintClips`에 넣는다.

**사운드를 클립 Animation Event로 넣지 않는다.** 힌트는 줄의 칸마다 동시에 재생되어 소리가 겹친다.

---

## 클리어 FX · 사운드

- 칸 파티클: `LineClearEffects[i]` → 실제 클리어 때 칸 중심에서 1발
- 가운데 1발: `FireCenteredLineClear` 켜고 `CenterLineClearEffect` 지정
- **놨을 때:** `PlaceSound`. 비면 `BoardPlacementBootstrap.blockPlaceSound`
- **터질 때:** `LineClearSound`. 비면 `BoardPlacementBootstrap.blockExplodeSound`

둘 다 `BoardPlacementBootstrap.PlaceBlock`에서 1발. `SoundClipSO`는 `Create > GGMLib > Sound Clip`. `PlaySoundEvent`로만 재생한다.

---

## 새 스킨 체크리스트

1. `SkinDataSO` 생성, `Sprites` · `icon` · 해금 값
2. `SkinDataListSO`에 등록 (인벤토리·장착)
3. 예고 클립 → `HintClips` (재사용 가능)
4. 클리어 파티클 `PoolItemSO` → `LineClearEffects` 또는 가운데 1발
5. `PlaceSound` / `LineClearSound` (`SoundClipSO`, 색 id 배열 아님)
6. 코드·Block 프리팹·Animation Event 수정 금지

---

## 건드리지 말 것

- Block 프리팹에 `AnimationSoundPlayer` 추가
- Hint 클립에 사운드 Animation Event
- 공용 `BlockShatter.mat` 값을 인스턴스마다 직접 변경 (칸이 같이 쪼개짐)
- 배치/클리어음을 칸 루프에서 `PlaySoundEvent` 호출
