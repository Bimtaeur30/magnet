# UGUI Arc Text

레거시 UGUI `Text`의 글자를 아치 형태로 휘어 주는 메시 효과입니다.

## 사용법

1. Canvas 아래의 `Text` 오브젝트를 선택합니다.
2. `Add Component > UI > Effects > UGUI Arc Text`를 추가합니다.
3. `Arc Angle`을 조절합니다.
   - 양수: 위로 볼록
   - 음수: 아래로 볼록
   - 0: 원래 모양
4. 글자가 곡선의 접선을 따라 회전하지 않게 하려면 `Rotate Characters`를 끕니다.

런타임에서는 `ArcAngle` 프로퍼티로 각도를 변경할 수 있습니다.

```csharp
using KTJ.CodexTool;

arcText.ArcAngle = 45f;
```

> 이 컴포넌트는 `UnityEngine.UI.Text` 전용입니다. TextMeshProUGUI에는 적용되지 않습니다.
