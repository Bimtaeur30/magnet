# line-clear-preview-fx Sequence 4

> Phase 3 구현 기록 · 기본 스킨 쩌적 힌트

## 1 — 2026-08-16 · 보로노이 쩌적으로 스케일 바운스 대체

## 변경 상세

- 파일: `Graphics/Shaders/BlockShatter.shader`
  - 심볼: `Magnet/BlockShatter` — 셰이더 (추가)
    - 설명: 스프라이트 UV를 보로노이 칸으로 나누고 `_Shatter`에 따라 금을 벌리며 조각을 살짝 밀어 샘플한다. 0이면 원본과 같다. SpriteMask용 스텐실과 아틀라스 `_SpriteUVRect`를 쓴다.
    - 이유: 2~4조각 대각선 대신 불규칙한 다수 조각을 쿼드 스프라이트에서 내기 위해.
    - 영향: `BlockShatter.mat` → Block Skin `SpriteRenderer`.

- 파일: `Scripts/Presentation/BlockShatterHint.cs`
  - 심볼: `BlockShatterHint.ShatterId` / `SpriteUVRectId` — 필드 `int` (추가)
    - 설명: `_Shatter`, `_SpriteUVRect` 셰이더 프로퍼티 ID를 캐시한다.
    - 이유: LateUpdate마다 문자열 조회를 피하려고.
  - 심볼: `BlockShatterHint.skinRenderer` — 필드 `SpriteRenderer` (추가)
    - 설명: MPB를 넣을 칸 스킨 렌더러.
    - 이유: 같은 GO의 SpriteRenderer에만 쩌적을 적용하려고.
  - 심볼: `BlockShatterHint.shatter` — 필드 `float` (추가)
    - 설명: 0~1 쩌적 세기. 힌트 클립이 이 값을 애니메이션한다.
    - 이유: 머티리얼 인스턴스 없이 Animator가 조절할 공개 필드가 필요해서.
  - 심볼: `BlockShatterHint._propertyBlock` — 필드 `MaterialPropertyBlock` (추가)
    - 설명: 칸마다 `_Shatter`와 스프라이트 UV 렉트를 넣는다.
    - 이유: 공용 머티리얼을 쓰면 모든 칸이 같이 쪼개지므로.
  - 심볼: `BlockShatterHint.Reset()` — 메서드 (추가)
    - 설명: 같은 GO의 SpriteRenderer를 `skinRenderer`로 채운다.
    - 이유: 컴포넌트 추가 시 참조를 수동으로 안 넣게.
  - 심볼: `BlockShatterHint.LateUpdate()` — 메서드 (추가)
    - 설명: 매 프레임 `Apply()`를 호출한다.
    - 이유: Playable 클립이 `shatter`를 쓴 뒤 MPB에 반영하려고.
  - 심볼: `BlockShatterHint.Apply()` — 메서드 (추가)
    - 설명: 현재 `shatter`와 스프라이트 아틀라스 렉트를 MPB에 넣어 렌더러에 적용한다.
    - 이유: 힌트 중·스프라이트 교체 직후에도 셰이더가 올바른 UV로 쪼개게.
    - 영향: `Block.ApplySprite` / `ResetShatter`가 호출.
  - 심볼: `BlockShatterHint.ResetShatter()` — 메서드 (추가)
    - 설명: `shatter`를 0으로 두고 `Apply()`한다.
    - 이유: 힌트 종료·풀 반환 때 금이 남은 칸이 나가지 않게.
  - 심볼: `BlockShatterHint.ApplySpriteUVRect(...)` — 메서드 (추가)
    - 설명: `sprite.textureRect`를 텍스처 크기로 나눠 `_SpriteUVRect`(min.xy, size.zw)를 넣는다. 스프라이트가 없으면 (0,0,1,1).
    - 이유: 기본 스킨이 아틀라스라서 메시 UV가 0~1이 아니어서.

- 파일: `Scripts/Presentation/Block.cs`
  - 심볼: `Block.shatterHint` — 필드 `BlockShatterHint` (추가)
    - 설명: Skin의 쩌적 드라이버. 비어 있으면 Awake에서 skinRenderer와 같은 GO에서 찾는다.
    - 이유: 힌트 클립 정지 시 쩌적을 원복할 대상이 필요해서.
  - 심볼: `Block.Awake()` — 메서드 (수정)
    - 설명: `shatterHint`를 보충하고 `ResetShatter()`로 시작한다.
    - 이유: 풀에서 나온 칸이 이전 쩌적 값으로 보이지 않게.
  - 심볼: `Block.StopHintClip()` — 메서드 (수정)
    - 설명: 애니메이터를 끈 뒤 `shatterHint.ResetShatter()`를 호출한다. 스케일 1 원복은 유지한다.
    - 이유: 클립이 멈춘 뒤 금이 고정되지 않게.
  - 심볼: `Block.ApplySprite(...)` — 메서드 (수정)
    - 설명: 스프라이트·색을 넣은 다음 `shatterHint.Apply()`로 UV 렉트를 갱신한다.
    - 이유: 힌트 중 스킨이 바뀌어도 아틀라스 좌표가 맞게.

- 파일: `Graphics/Animations/DefaultHintBounce.anim`
  - 심볼: `DefaultHintBounce` — 클립 (삭제)
    - 설명: 스케일 뾰잉뾰잉 클립을 제거한다.
    - 이유: 쩌적 힌트로 대체해서.

## 2 — 2026-08-16 · 한 번 갈라진 뒤 정지, 금은 굵고 적게

## 변경 상세

- 파일: `Scripts/Presentation/Block.cs`
  - 심볼: `Block.LoopHintClipIfNeeded()` — 메서드 (수정)
    - 설명: 루프 클립은 그대로 두고, 비루프 클립은 끝에 시간을 고정하고 재생 속도를 0으로 둔다. 이전처럼 되감아 다시 시작하지 않는다.
    - 이유: 쩌적은 숨쉬듯 반복하지 않고 갈라진 채로 멈춰 있어야 해서.

- 파일: `Graphics/Shaders/BlockShatter.shader`
  - 심볼: `Magnet/BlockShatter` Frag 잔금 보로노이 — (삭제)
    - 설명: 셀 수 2.35배 잔금 패스를 빼고 큰 칸 보로노이만 쓴다. 기본 Cell Count 3, Crack Width 0.038.
    - 이유: 얇은 금이 너무 많이 생기던 것을 굵고 적은 조각으로 바꾸려고.

## 3 — 2026-08-16 · 조각 수 감소 + 칸 시드 1~8

## 변경 상세

- 파일: `Graphics/Shaders/BlockShatter.shader`
  - 심볼: `_CellCount` — 프로퍼티 (수정)
    - 설명: 기본값을 5에서 3으로 낮춘다.
    - 이유: 칸마다 쪼개지는 조각 수를 줄이려고.
  - 심볼: `_ShatterSeed` — 프로퍼티 `float` (추가)
    - 설명: 칸마다 1~8 시드를 MPB로 넣어 보로노이 해시 오프셋에 쓴다. CBUFFER 밖 PerRendererData.
    - 이유: 같은 머티리얼이어도 줄 안의 칸이 다른 금 무늬를 내게.
  - 심볼: `Hash22` — 함수 (수정)
    - 설명: 입력에 `_ShatterSeed * (19.17, 7.31)`을 더한 뒤 기존 해시를 돌린다.
    - 이유: 시드가 바뀌면 사이트·금 폭·조명 기울기가 전부 달라지게.

- 파일: `Graphics/Materials/BlockShatter.mat`
  - 심볼: `_CellCount` — 머티리얼 값 (수정)
    - 설명: 5에서 3으로 낮춘다.
    - 이유: 셰이더 기본과 맞춰 실제 렌더도 조각이 적게 나오게.

- 파일: `Scripts/Presentation/BlockShatterHint.cs`
  - 심볼: `BlockShatterHint.SeedMin` / `SeedCount` — 상수 `int` (추가)
    - 설명: 시드 범위를 1부터 8개로 고정한다.
    - 이유: 8칸 줄에서 칸마다 다른 시드를 쓰되 껐다 켜도 같은 값이 나오게.
  - 심볼: `BlockShatterHint.ShatterSeedId` — 필드 `int` (추가)
    - 설명: `_ShatterSeed` 프로퍼티 ID를 캐시한다.
    - 이유: LateUpdate마다 문자열 조회를 피하려고.
  - 심볼: `BlockShatterHint._shatterSeed` — 필드 `int` (추가)
    - 설명: 현재 칸의 쩌적 시드(1~8)를 보관한다.
    - 이유: MPB에 매 프레임 같은 값을 넣어 힌트를 껐다 켜도 무늬가 유지되게.
  - 심볼: `BlockShatterHint.Apply()` — 메서드 (수정)
    - 설명: `_Shatter`·UV 렉트와 함께 `_ShatterSeed`를 MPB에 넣는다.
    - 이유: 시드가 셰이더에 전달되어야 칸마다 금이 달라져서.
  - 심볼: `BlockShatterHint.SetSeed(...)` — 메서드 (추가)
    - 설명: 시드를 1~8로 감싼 뒤 `_shatterSeed`에 넣고 `Apply()`한다.
    - 이유: 보드 칸이 바뀌어도 범위를 벗어난 값이 들어가지 않게.
  - 심볼: `BlockShatterHint.SeedFromCell(...)` — 메서드 (추가)
    - 설명: `(x + y * 3) mod 8 + 1`로 칸 좌표를 시드 1~8로 만든다. 가로·세로 줄 모두 인접 칸이 다른 값이 된다.
    - 이유: 같은 칸은 항상 같은 시드, 한 줄의 8칸은 서로 다른 시드가 필요해서.

- 파일: `Scripts/Presentation/Block.cs`
  - 심볼: `Block.SetShatterSeed(...)` — 메서드 (추가)
    - 설명: `shatterHint.SetSeed`로 칸 시드를 넘긴다. 힌트가 없으면 아무 것도 하지 않는다.
    - 이유: Effector가 보드 좌표 시드를 칸 뷰에 넣게.

- 파일: `Scripts/Presentation/LineClearHintEffector.cs`
  - 심볼: `LineClearHintEffector._desiredSeeds` — 필드 `Dictionary<Block, int>` (추가)
    - 설명: 이번 힌트 대상 칸 → 시드 1~8 맵을 재사용한다.
    - 이유: SetHints마다 할당하지 않고, 이미 힌트 중인 칸에도 같은 시드를 다시 넣으려고.
  - 심볼: `LineClearHintEffector.SetHints(...)` — 메서드 (수정)
    - 설명: `previewPivot`을 받아 배치 칸은 보드 좌표, 프리뷰 칸은 `previewPivot + Offset`으로 시드를 계산한다.
    - 이유: 프리뷰 Offset은 피스 로컬이라 줄 좌표와 다르면 같은 줄에서 시드가 겹칠 수 있어서.
  - 심볼: `LineClearHintEffector.ClearHints()` — 메서드 (수정)
    - 설명: `_desiredSeeds`도 비운다.
    - 이유: 힌트가 꺼진 뒤 이전 시드 맵이 남지 않게.
  - 심볼: `LineClearHintEffector.SyncSet(...)` — 메서드 (수정)
    - 설명: 새로 켜는 칸뿐 아니라 이미 힌트 중인 칸에도 `SetShatterSeed`를 먼저 호출한다.
    - 이유: 프리뷰가 줄을 따라 옮겨도 시드가 칸에 맞게 갱신되고, 같은 칸을 다시 가리키면 같은 시드가 유지되게.
  - 심볼: `LineClearHintEffector.RememberDesired(...)` — 메서드 (추가)
    - 설명: 블록을 `_desired`에 넣고 `SeedFromCell(cell)`을 `_desiredSeeds`에 기록한다.
    - 이유: 배치·프리뷰 경로가 같은 시드 규칙으로 모이게.

- 파일: `Scripts/Presentation/GameBoard.cs`
  - 심볼: `GameBoard.SetLineClearHints(...)` — 메서드 (수정)
    - 설명: `previewPivot`을 `PlacedBlocksView`로 그대로 넘긴다.
    - 이유: 프리뷰 칸의 보드 좌표를 Effector까지 전달하려고.

- 파일: `Scripts/Presentation/PlacedBlocksView.cs`
  - 심볼: `PlacedBlocksView.SetLineClearHints(...)` — 메서드 (수정)
    - 설명: `previewPivot`을 `LineClearHintEffector.SetHints`로 넘긴다.
    - 이유: 보드 좌표 시드 계산에 피벗이 필요해서.

- 파일: `Scripts/Input/BlockDragInput.cs`
  - 심볼: `BlockDragInput.UpdateLineClearHints(...)` — 메서드 (수정)
    - 설명: `SetLineClearHints`에 현재 `boardPivot`을 같이 넘긴다.
    - 이유: 프리뷰 칸 시드를 `boardPivot + Offset`으로 고정하려고.
