## 1 — 2026-07-16 · 스킨 변경 시 블록 아이콘 런타임 재생성 + UI 텍스처 타입 수정

**바뀐 것**
- 수정: `Assets/_MemberWorkspace/PTY/Scripts/Data/BlockShapeSO.cs` — `SetIcon(Texture2D)` 추가
- 수정: `Assets/_MemberWorkspace/PTY/Scripts/Editor/BlockShapeIconGenerator.cs` — `ConfigureImporter`의 `textureType`을 `Default`→`Sprite`(`spriteImportMode=Single`)로 변경
- 생성: `Assets/_MemberWorkspace/PTY/Scripts/Presentation/RuntimeBlockIconGenerator.cs` — `Skin Channel.asset`에서 `SkinChangedEvent`/`SkinChangedRequestEvent` 구독, 런타임 캡처 후 `SkinChangedResponseEvent` raise
- 수정: `Assets/_MemberWorkspace/PTY/Scenes/PTYScene.unity` — `RuntimeBlockIconGenerator` GameObject 추가·배선
- 수정(재생성 반영): `Assets/_MemberWorkspace/PTY/Sprites/BlockIcons/*.png(.meta)` 11개 — 새 텍스처 타입(Sprite)으로 재임포트

**메모**
- 텍스처 타입 수정 후 `BlockShapeIconGenerator.GenerateAllIcons()`를 직접 실행해 기존 11개 PNG에 새 임포트 설정을 즉시 반영함(콘솔: "11개 블록 아이콘 생성 완료", 에러 없음). `AssetDatabase.LoadAssetAtPath<Sprite>()`로 스프라이트 서브에셋이 정상 생성됨을 확인.
- Play 모드에서 `Skin Channel.asset`으로 `SkinChangedRequestEvent`를 raise → `SkinChangedResponseEvent` 수신 확인(`responseReceived=True`), 첫 번째 `BlockShapeSO`의 `Icon`이 기존 PNG(`1x2-Shape_Icon`)에서 256x256 신규 캡처 텍스처로 교체됨을 확인. 콘솔 에러 없음.
- **주의**: 스킨 이벤트는 공용 `magnetGameChannel`이 아니라 별도 `Assets/_Shared/ScriptableObjects/Skin Channel.asset`에서 오간다(`SkinManager.eventChannel`과 동일 인스턴스). 처음에 이걸 놓치면 리스너가 이벤트를 영원히 못 받는 버그가 생기므로 씬에 배선할 때 반드시 이 채널을 써야 함.
- `SkinChangedRequestEvent`를 실제로 raise하는 호출부는 이번 작업 범위 밖(리스너/응답 측만 구현).
