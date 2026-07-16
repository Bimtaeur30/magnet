# Phase 1 — 스킨 변경 시 블록 아이콘 런타임 재생성 + UI용 텍스처 타입 수정

> **구현:** `skin-icon-generation`
> **상태:** 완료
> **변경 기록:** [sequence1.md](sequence1.md) (1:1)

## 배경

`PMS.Scripts.Events.SkinEvents`(`Assets/_Shared/Magnet.Core/Events/SkinEvents.cs`)에 `SkinChangedRequestEvent`/`SkinChangedResponseEvent`가 정의만 되어 있고 연결되지 않은 상태였음. 요청: Request가 오면 아이콘을 재생성하고 끝나면 Response를 raise. 또한 기존 에디터용 아이콘 생성기(`BlockShapeIconGenerator`)가 굽는 PNG의 텍스처 타입이 `Default`라 UI(`Image`/`Sprite`)에 못 쓰는 문제도 같이 수정.

기존 `BlockShapeIconGenerator`는 `UnityEditor` API(AssetDatabase, PreviewScene, TextureImporter)로 PNG를 디스크에 굽는 방식이라 **빌드에서는 컴파일조차 되지 않음**. 그런데 `SkinChangedRequestEvent`는 실제 플레이/빌드 중에도 발생해야 하므로, 완전히 새로운 런타임 캡처 경로가 필요했음(사용자 확인 후 "런타임 카메라 즉석 캡처" 방식으로 결정).

## 목표 (완료 기준)

- [x] `BlockShapeIconGenerator.ConfigureImporter`: `textureType`을 `Default` → `Sprite`(`spriteImportMode = Single`)로 변경. 기존 11개 블록 아이콘 PNG를 재생성해 새 임포트 설정 적용 확인(`Sprite` 서브에셋 생성됨, 기존 `Texture2D` 소비처는 그대로 호환)
- [x] `BlockShapeSO`에 `SetIcon(Texture2D)` 추가 — 런타임에서 아이콘을 메모리상으로만 갱신하기 위함(디스크 미저장)
- [x] 신규 `RuntimeBlockIconGenerator`(MonoBehaviour, `PTY.Scripts.Presentation`): `Skin Channel.asset`에서 `SkinChangedEvent`(현재 스킨 캐싱)와 `SkinChangedRequestEvent`(생성 트리거)를 구독. 요청 시 `ShapeBlock` 프리팹 인스턴스를 씬 밖(10000,10000,0)에 두고 전용 `Camera`+`RenderTexture`로 각 `BlockShapeSO`를 즉석 캡처, `SetIcon`으로 교체 후 `SkinChangedResponseEvent` raise
- [x] Play 모드에서 `SkinChangedRequestEvent` raise → `SkinChangedResponseEvent` 수신 확인, 아이콘이 256x256 새 텍스처로 교체됨 확인(콘솔 에러 없음)

## 구현 내용 (뭘 어떻게)

| 파일 | 위치 | 변경 |
|---|---|---|
| `BlockShapeSO.cs` | `Assets/_MemberWorkspace/PTY/Scripts/Data/` | `SetIcon(Texture2D)` 추가 |
| `BlockShapeIconGenerator.cs` | `Assets/_MemberWorkspace/PTY/Scripts/Editor/` | `ConfigureImporter`에서 `textureType = Sprite`, `spriteImportMode = Single` |
| `RuntimeBlockIconGenerator.cs` (신규) | `Assets/_MemberWorkspace/PTY/Scripts/Presentation/` | `SkinChangedRequestEvent`→아이콘 재생성→`SkinChangedResponseEvent` |
| `PTYScene.unity` | `Assets/_MemberWorkspace/PTY/Scenes/` | `RuntimeBlockIconGenerator` GameObject 배치, `Skin Channel.asset`/`ShapeBlock.prefab`/`BlockShapeSource.asset` 배선 |

### 설계 결정

- **채널 주의**: 스킨 관련 이벤트는 공용 `magnetGameChannel`이 아니라 **별도 `Skin Channel.asset`**(`SkinManager.eventChannel`과 동일 인스턴스)에서 raise/listen된다. `RuntimeBlockIconGenerator`도 반드시 이 채널에 배선해야 함(다른 채널에 걸면 이벤트를 영영 못 받음).
- 런타임 캡처는 디스크에 저장하지 않는다 — `BlockShapeSO.icon`을 메모리에서만 교체(Play 모드 종료 시 원복됨). 영속화가 필요해지면 별도 논의 필요.
- 스킨은 `ShapeBlock.ApplySkin(IBlockSkin)`으로 모든 칸에 동일 스프라이트를 입히는 구조라(모양별로 다른 아트 아님), 스킨 변경 시 **모든** BlockShapeSO 아이콘을 한 번에 재생성한다(`BlockShapeSourceSO.Shapes` 순회).
- 캡처 리그(ShapeBlock 인스턴스 + Camera)는 씬 밖 좌표(10000,10000,0)에 두고 재사용 — 레이어 마스크 설정 등 프로젝트 전역 설정 변경 없이 화면에 노출되지 않도록 함.

## 이 Phase 범위 밖

- 아이콘 영속화(재시작 후에도 마지막 생성 아이콘 유지) — 필요 시 별도 요청
- `SkinChangedRequestEvent`를 실제로 raise하는 호출부(UI 등) 배선 — 이번 작업은 리스너(응답)만 구현. 어디서 Request를 raise할지는 범위 밖
- KTJ `BlockSlot_UI.cs` 등 실제 소비처 UI 코드 수정 — 다른 워크스페이스라 손대지 않음

## 코드 — 어디를 보면 되나

| 보려는 것 | 경로 |
|---|---|
| 런타임 리스너 | `Assets/_MemberWorkspace/PTY/Scripts/Presentation/RuntimeBlockIconGenerator.cs` |
| 에디터 생성기(텍스처 타입 수정) | `Assets/_MemberWorkspace/PTY/Scripts/Editor/BlockShapeIconGenerator.cs` |
| 아이콘 SO | `Assets/_MemberWorkspace/PTY/Scripts/Data/BlockShapeSO.cs` |
| 이벤트 정의(수정 안 함, 참고용) | `Assets/_Shared/Magnet.Core/Events/SkinEvents.cs` |
