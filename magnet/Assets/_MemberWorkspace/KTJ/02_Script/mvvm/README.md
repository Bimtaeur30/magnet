# MVVM Tool

UGUI용 간단한 MVVM 바인딩 코드 생성 툴입니다.

## 사용 순서

1. Unity에서 UGUI 프리팹 또는 씬의 UI 루트를 선택합니다.
2. `Tools/MVVM/Binding Tool`을 엽니다.
3. `Profile Folder`에 바인딩 프로필 ScriptableObject를 만들 위치를 지정합니다.
4. `Generated Root`에 View/ViewModel 폴더들을 만들 위치를 지정합니다.
5. `Create Profile From Selection`을 눌러 바인딩 프로필을 만듭니다.
6. 필요하면 `Scan UI`로 후보 바인딩을 다시 스캔합니다.
7. 바인딩 목록에서 ViewModel 멤버 이름과 모드를 조정합니다.
8. `Generate Code`를 누릅니다.
9. Unity 컴파일이 끝나면 View 컴포넌트와 serialized field가 자동 연결됩니다.
10. 자동 연결이 누락되었거나 수동으로 다시 맞추고 싶으면 `Wire View Fields`를 누릅니다.

## 생성되는 코드

- `ViewClass.cs`: 사용자가 수정하는 partial View 클래스입니다. 기존 파일이 있으면 덮어쓰지 않습니다.
- `ViewClass.Generated.cs`: 바인딩 코드입니다. 다시 생성하면 덮어씁니다.
- `ViewModelClass.cs`: 사용자가 수정하는 partial ViewModel 파일입니다. 기존 파일이 있으면 덮어쓰지 않습니다.
- `ViewModelClass.Generated.cs`: 바인딩에 필요한 ViewModel 프로퍼티/커맨드 코드입니다. 다시 생성하면 덮어씁니다.

## 지원 바인딩

- `TMP_Text.text`, `TextMeshProUGUI.text`, `UnityEngine.UI.Text.text`: OneWay
- `TMP_Text.color`, `TextMeshProUGUI.color`, `UnityEngine.UI.Text.color`: OneWay
- `TMP_Text.color.a`, `TextMeshProUGUI.color.a`, `UnityEngine.UI.Text.color.a`: OneWay
- `Button.onClick`: Command
- `Button.interactable`: OneWay
- `Slider.value`: TwoWay
- `Slider.interactable`: OneWay
- `Toggle.isOn`: TwoWay
- `Toggle.interactable`: OneWay
- `TMP_InputField.text`, `InputField.text`: TwoWay
- `TMP_InputField.interactable`, `InputField.interactable`: OneWay
- `Image.sprite`: OneWay
- `Image.color`: OneWay
- `Image.color.a`: OneWay
- `RawImage.texture`: OneWay
- `RawImage.color`: OneWay
- `RawImage.color.a`: OneWay
- `CanvasGroup.alpha`: OneWay
- `CanvasGroup.interactable`: OneWay
- `CanvasGroup.blocksRaycasts`: OneWay
- `RectTransform.anchoredPosition`: OneWay
- `RectTransform.anchoredPosition.x`: OneWay
- `RectTransform.anchoredPosition.y`: OneWay
- `RectTransform.sizeDelta`: OneWay
- `RectTransform.sizeDelta.x`: OneWay
- `RectTransform.sizeDelta.y`: OneWay
- `RectTransform.localEulerAngles.z`: OneWay
- `RectTransform.localScale`: OneWay
- `RectTransform.localScale.x`: OneWay
- `RectTransform.localScale.y`: OneWay
