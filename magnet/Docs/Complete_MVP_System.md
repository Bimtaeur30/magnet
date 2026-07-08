## Magnet MVP System 완전 가이드

이 문서는 **이 프로젝트에서 실제로 쓰는 MVP 시스템 전체 흐름**을 정리한 것입니다.  
일반 교과서식 MVP 설명이 아니라, **지금 코드 기준으로 “어디에 뭘 넣어야 새 UI를 붙일 수 있는지”** 에 초점을 둡니다.

---

## 1. 이 MVP 시스템을 왜 알아야 하나

- **UI를 추가하거나 수정하려면** 반드시 이 MVP 흐름을 따라야 합니다.
- Presenter·View·Model 뿐 아니라, **Form / UIParam / UIManager / UIId / EventChannel** 이 모두 연결되어 있어
  일부만 이해하면 디버깅이 매우 어렵습니다.
- 이 문서 하나만 읽고 나면:
  - `어디에 어떤 스크립트를 추가해야 하는지`
  - `어떤 메서드 시그니처로 Model을 써야 하는지`
  - `버튼 / 텍스트 / 게이지 같은 기본 UI를 어떻게 추가하는지`
  를 스스로 판단할 수 있어야 합니다.

---

## 2. 전체 구조 한눈에 보기

### 2.1 주요 구성요소

- **Model (`IModel`)**
  - UI가 사용할 **상태·행동을 제공하는 클래스**입니다.
  - 규약에 맞는 메서드를 정의하면, Form 이 **문자열로 그 메서드를 찾아 호출**합니다.

- **View (`BaseView`)**
  - 하나의 UI 화면(프리팹)을 대표하는 **루트 뷰**입니다.
  - Canvas 활성/비활성, 전체 Update 진입점을 담당합니다.
  - 실제 텍스트·게이지·버튼 등은 **Form** 들이 맡습니다.

- **Presenter (`BasePresenter`)**
  - 해당 UI 프리팹의 **조립자 & 라이프사이클 관리자**입니다.
  - `Model` 생성, `View` 연결, `Form` 초기화 및 **문자열 바인딩**을 모두 여기서 합니다.
  - `Open(payload)` / `Close()` 로 열고 닫습니다.

- **Form (`BaseForm` 계열)**
  - 실제 버튼, 텍스트, 슬라이더 같은 **위젯 단위**입니다.
  - 인스펙터에서 `interactMethod` / `updateMethod` 문자열을 넣어두면,
    `Model` 의 메서드와 자동으로 연결됩니다.

- **UIParam 계열**
  - `Form` 과 `Model` 사이에서 오가는 **데이터 컨테이너**입니다.
  - 예: `UIStringParam` 은 문자열 1개를 담아서 TextForm 에 전달.

- **UIManager**
  - UI 프리팹들을 미리 인스턴스화하고, **열기/닫기/토글/모달 스택/씬 전환 정책**을 담당합니다.
  - `UIId` + `EventChannel` 기반으로 외부에서 UI를 여닫습니다.

---

## 3. 핵심 타입별 책임 (코드 기준)

### 3.1 Model: `IModel`

- 경로: `Assets/HwanLib/MVP/System/BaseMVP/IModel.cs`
- 내용은 비어 있는 마커 인터페이스입니다.
- 역할:
  - 모든 Model 은 `IModel` 을 구현해야 하며,
  - **규약에 맞는 메서드를 작성하면** `MVPBinding` 이 이를 찾아 Form 과 연결합니다.

### 3.2 Presenter: `BasePresenter`

- 경로: `Assets/HwanLib/MVP/System/BaseMVP/BasePresenter.cs`
- 핵심 역할:
  - `Model` 과 `View` 를 만들고 묶어줍니다.
  - 자식에 붙어 있는 모든 `BaseForm` 을 찾아 **문자열 기반으로 Model 메서드에 바인딩**합니다.
  - `Open(payload)` / `Close()` 로 UI 라이프사이클을 제어합니다.

#### 3.2.1 생성·초기화

- `InitializePresenter()` 흐름:
  - `Model = CreateModel();`
  - `View = ResolveView();`
  - 자식에서 모든 `BaseForm` 검색
    - `IInitializable` 이면 `Initialize()`
    - `IInteractable` 이고 `InteractMethod` 가 설정되어 있으면:
      - `MVPBinding.ResolveInteract(Model, form.InteractMethod)` 로 `void M(UIParam)` 찾아 델리게이트 생성
      - `OnFormInteracted` 에 핸들러 등록
    - `IUpdatable` 이고 `UpdateMethod` 가 설정되어 있으면:
      - `MVPBinding.ResolveUpdate(Model, form.UpdateMethod)` 로 `UIParam M()` 찾아 델리게이트 생성
      - `BindUpdateSource` 로 주입
  - `View.InitializeView(forms)` 호출
  - `View.OnViewClosed` 를 구독해 닫힘을 감지

#### 3.2.2 열고 닫기

- `Open<T>(T payload)`:
  - `IsOpen = true`
  - 필요 시 `TimeManager.Stop`, `CursorManager.Free` 로 게임 정지 + 커서 풀기
  - `View.OpenView()` 호출

- `Close()`:
  - `IsOpen` 이고 아직 닫는 중이 아닐 때만 동작
  - `View.CloseView()` 호출 → View 쪽에서 `OnViewClosed` 이벤트 발행 → Presenter 가 `OnClosed` 전달
  - `OnClosed` 이벤트는 `UIManager` 가 구독하여 스택·풀 상태를 갱신합니다.

### 3.3 View: `BaseView`

- 경로: `Assets/HwanLib/MVP/System/BaseMVP/BaseView.cs`
- 역할:
  - Canvas 루트를 잡고, 열기/닫기/전체 갱신 진입점을 제공합니다.
  - 실제 컨트롤 갱신은 `Form` 들이 담당합니다.

#### 3.3.1 주요 메서드

- `InitializeView(IReadOnlyList<BaseForm> forms)`
  - 첫 번째 자식에서 `Canvas` 를 찾고, 이를 루트로 사용합니다.
  - 초기에는 `SetActive(false)` 로 꺼 둡니다.

- `OpenView()`
  - Canvas GameObject 활성화
  - `UpdateView()` 를 한 번 호출해 **초기 상태를 그립니다.**

- `UpdateView()`
  - 자식 `BaseForm` 중 `IUpdatable` 인 것들을 찾아 `UpdateForm()` 호출
  - 실제 UI 데이터 갱신은 각 Form 이 `UIParam` 을 받아 처리합니다.

- `CloseView()`
  - Canvas 비활성화
  - `OnViewClosed` 이벤트 발행 (Presenter가 구독)

### 3.4 Form: `BaseForm` + 파생 클래스

- 베이스: `Assets/HwanLib/MVP/System/BaseMVP/Form/BaseForm.cs`
  - 직렬화 필드:
    - `interactMethod` : `IInteractable` 일 때 Model 의 `void M(UIParam)` 메서드명
    - `updateMethod` : `IUpdatable` 일 때 Model 의 `UIParam M()` 메서드명
  - 인스펙터에서 문자열로 입력한 이 값으로 **MVPBinding 이 메서드를 찾습니다.**

- 예시: 클릭 Form
  - `AbstractClickForm` (`System/AbstractMVP/Form/AbstractClickForm.cs`)
  - `ButtonForm` (`Forms/ButtonForm.cs`) 등
  - 공통:
    - `IInteractable` 구현
    - 포인터/Submit 입력을 감지해 `OnFormInteracted(UIParams.UIClickParam)` 발행

- 예시: 텍스트 Form
  - `TextForm` (`Forms/TextForm.cs`)
  - `AbstractVisualForm` 을 상속하고, `UpdateVisual(UIParam data)` 를 구현
  - `UIStringParam` 으로 캐스팅해 텍스트를 갱신

### 3.5 IInteractable / IUpdatable

- `IInteractable` (`System/BaseMVP/Form/IInteractable.cs`)
  - `event FormInteracted OnFormInteracted;`
  - Form 이 **사용자 입력을 Model 로 전달하는 통로**입니다.

- `IUpdatable` (`System/BaseMVP/Form/IUpdatable.cs`)
  - `BindUpdateSource(Func<UIParam> source)` : Presenter 가 Model 의 `UIParam M()` 을 주입
  - `UpdateForm()` : 주입된 소스를 호출해 UI 를 갱신

### 3.6 UIParam 계열

- 베이스: `UIParam` (`UIData/UIParam.cs`)
- 예: `UIStringParam` (`UIData/UIStringParam.cs`)
  - `public string Value;`
  - `Init(string value)` 로 값 세팅
  - TextForm 등에서 캐스팅 후 사용

실제로 사용할 때는 보통 정적 풀 또는 헬퍼(`UIParams.XXX`) 를 통해 재사용하는 패턴을 씁니다.

### 3.7 MVPBinding: 문자열 → 메서드 바인딩

- 경로: `Assets/HwanLib/MVP/System/MVPBinding.cs`
- 역할:
  - Model 타입에서 다음 시그니처를 가진 메서드를 찾습니다.
    - **Interact**: `void MethodName(UIParam param)`
    - **Update**: `UIParam MethodName()`
  - `BaseForm.InteractMethod` / `UpdateMethod` 에 들어있는 문자열과 이름이 같은 메서드만 인정합니다.

- 제공 메서드:
  - `ResolveInteract(object model, string methodName) → Action<UIParam>`
  - `ResolveUpdate(object model, string methodName) → Func<UIParam>`

즉, **메서드 이름과 시그니처 둘 다 일치해야** 바인딩이 성공합니다.

---

## 4. UIManager 흐름: UI가 언제 어떻게 생성·관리되는가

### 4.1 초기화

- 경로: `Assets/HwanLib/MVP/System/GenerateUI/UIManager.cs`
- 싱글톤(`LightSingleton<UIManager>`)으로 동작합니다.
- `Initialize()` 에서 하는 일:
  - `WorldUICamera` 인스턴스화 및 카메라 스택 설정
  - `EventSystem` 활성화
  - `UIRegistrySO` 에 등록된 모든 UI 프리팹에 대해 `SpawnEntry` 호출
  - `uiChannel`(EventChannelSO)에 `OpenUIEvent` / `CloseUIEvent` / `ToggleUIEvent` 리스너 등록
  - 씬 로드시 정책 적용을 위해 `SceneManager.sceneLoaded` 구독

### 4.2 프리팹 스폰 (`SpawnEntry`)

- 각 프리팹에서 `BasePresenter` 를 읽어 설정을 확인:
  - `MultiableCount > 0` 이면 풀 생성 (`MultiablePool`)
  - 아니면 단일 인스턴스로 생성:
    - `CreateInstance(prefab, worldCam)`:
      - `Instantiate`
      - `InitializePresenter()` 호출 → Model·View·Form 연결
      - `UICameraStack.AssignWorldCamera` 로 카메라 설정
      - `OnClosed` 를 구독해 스택/풀 정리
    - `_byType` / `_single` 딕셔너리에 등록

### 4.3 열기/닫기 이벤트

- 열기(`OpenUIEvent`):
  - `Id` 가 풀에 있으면:
    - 풀에서 Presenter 하나 획득 → `CanOpen` 체크 → `SortingOrder` 지정 → `Open(payload)`
  - 단일이면:
    - `OpenSingle(single, payload)` 호출

- 토글(`ToggleUIEvent`):
  - 단일 Presenter 만 대상
  - 열려 있으면 `Close()`, 아니면 `OpenSingle(single, null)`

- 닫기(`CloseUIEvent`):
  - 단일 Presenter 찾은 뒤 `Close()`

### 4.4 모달 스택 / 정렬

- `UILayer` 와 `_modalStack` 으로 Modal UI 의 정렬 순서를 관리합니다.
- HUD 는 고정, Popup/System 레이어는 스택에 push 하면서 SortingOrder 를 부여합니다.
- 닫힐 때:
  - 풀 소속이면 풀에 반환
  - 단일이면 모달 스택에서 제거 및 정렬 재계산

---

## 5. 실제 실행 흐름 (사용자 입장에서)

1. **어딘가에서 UI를 연다.**
   - 예: `uiChannel.RaiseEvent(GameEvents.OpenUI.Init(UIId.Inventory, payload))`
2. `UIManager.HandleOpen(OpenUIEvent)` 가 호출된다.
3. 해당 `UIId` 에 매핑된 `BasePresenter` 를 찾는다.
4. 필요 시 `InitializePresenter()` 가 이미 끝난 상태에서:
   - `Presenter.Open(payload)` 호출
   - `View.OpenView()` → Canvas 활성화 + `UpdateView()` 1회
5. View 의 `UpdateView()` 가 자식 Form 들 중 `IUpdatable` 인 것에 대해 `UpdateForm()` 호출
6. 각 Form 은 주입받은 `Func<UIParam>` 을 호출해 Model 에서 값을 받아오고, 자신의 비주얼을 갱신한다.
7. 사용자가 버튼/슬라이더 등을 조작하면:
   - `IInteractable.OnFormInteracted(UIParam)` 이벤트 발행
   - Presenter 가 `MVPBinding.ResolveInteract` 로 연결해 둔 Model 메서드를 호출 (`void M(UIParam)`)
   - Model 이 내부 상태를 바꾸거나, 다른 시스템에 이벤트를 보낸다.
8. 필요 시 Presenter 나 외부 로직이 다시 `UpdateView()` 를 호출해 최신 상태를 반영한다.
9. UI 를 닫을 때:
   - Presenter 의 `Close()` 호출
   - `View.CloseView()` → `OnViewClosed` → Presenter 의 `OnClosed` → UIManager 가 스택/풀 정리

---

## 6. 새 기능 추가 가이드 (실전 레시피)

### 6.1 새로운 버튼 행동 추가하기

목표 예시: 인벤토리 UI에 `Sort` 버튼을 추가하고, 누르면 Model 의 정렬 로직을 호출한다.

1. **Form 추가**
   - 인벤토리 View 프리팹에서 버튼 GameObject 에 `ButtonForm`(또는 다른 `AbstractClickForm` 파생) 컴포넌트를 붙입니다.

2. **Model 메서드 작성**
   - 해당 UI 의 Model 클래스(예: `InventoryModel : IModel`)에 다음 시그니처 메서드를 추가합니다.
     - `void OnClickSort(UIParam param)`
     - 이름은 자유지만, **인스펙터에 적을 문자열과 동일해야 합니다.**

3. **Form ↔ Model 문자열 연결**
   - 버튼의 `BaseForm.interactMethod` 에 `OnClickSort` 를 입력합니다.
   - Presenter 가 초기화 시 `MVPBinding.ResolveInteract(Model, "OnClickSort")` 를 통해 자동 연결합니다.

4. **동작 확인**
   - 플레이 후 버튼을 눌렀을 때 Model 의 `OnClickSort` 가 호출되는지 로그 등으로 확인합니다.

### 6.2 새로운 텍스트(상태 표시) 추가하기

목표 예시: 코인 개수를 상단 HUD 에 표시하는 Text 를 MVP 방식으로 연결.

1. **Form 추가**
   - HUD View 프리팹에서 Text GameObject 에 `TextForm` 을 붙입니다.

2. **Model 메서드 작성**
   - HUD Model 클래스에 다음 시그니처 메서드 추가:
     - `UIParam GetCoinText()`
     - 내부에서 `UIStringParam` 을 얻어와 `Init` 후 반환합니다.

3. **Form ↔ Model 문자열 연결**
   - TextForm 의 `BaseForm.updateMethod` 에 `GetCoinText` 입력.

4. **Update 타이밍**
   - Presenter 는 `View.UpdateView()` 를 호출하여, Form 들의 `UpdateForm()` 을 불러줍니다.
   - 코인 변화 이벤트가 발생할 때마다 Presenter 나 다른 곳에서 `View.UpdateView()` 를 다시 호출해주면 최신 값이 반영됩니다.

### 6.3 새로운 화면(Presenter/View/Model) 자체 추가하기

1. **스크립트 생성**
   - `MVPScriptGenerator` (Editor) 를 사용:
     - Base 폴더, 카테고리, 이름을 입력하고 `Generate` 를 실행
     - `XXXModel`, `XXXPresenter`, `XXXView` 3개 스크립트가 생성됩니다.

2. **프리팹 준비**
   - Canvas 루트를 가진 UI 프리팹을 만든 뒤, 루트에 `XXXPresenter` 와 `XXXView` 를 붙입니다.
   - Presenter 의:
     - `layer` (HUD/Popup/System)
     - `id` (UIId enum)
     - `multiableCount` (0=단일, N>0=Multiable)
     를 설정합니다.

3. **UIId 등록**
   - `MVPScriptGenerator` 가 자동으로 `UIId.cs` 에 enum 항목을 추가해 줍니다.
   - 필요 시 수동 확인.

4. **레지스트리 등록**
   - `UIRegistrySO` 에 해당 프리팹을 등록합니다.

5. **씬 정책 (선택)**
   - 특정 씬에서 시작하자마자 열리게 하고 싶다면, `UIScenePolicySO` 에서 해당 씬의 `OpenOnStart` 목록에 UIId 를 추가합니다.

6. **Model/Forms 연결**
   - 이후 6.1, 6.2 의 방식대로 버튼/텍스트/슬라이더 등 Form 을 붙이고,
     Model 메서드와 문자열로 연결합니다.

---

## 7. 자주 하는 실수 & 체크리스트

### 7.1 메서드 시그니처 불일치

- **문제**
  - `void OnClickSort()` 처럼 `UIParam` 파라미터를 빼먹거나,
  - `UIStringParam GetCoinText()` 처럼 반환 타입이 `UIParam` 이 아니면 바인딩이 실패합니다.
- **규약**
  - Interact: `void MethodName(UIParam param)`
  - Update: `UIParam MethodName()`

### 7.2 메서드 이름 오타

- Form 의 `interactMethod` / `updateMethod` 에 적은 문자열과
  Model 메서드 이름이 한 글자라도 다르면 연결이 안 됩니다.
- 에디터에서 경고 로그:
  - `"{Presenter}/{Form}: interact '...' 미해결"`
  - `"{Presenter}/{Form}: update '...' 미해결"`

### 7.3 UIParam 캐스팅 실수

- TextForm 처럼 특정 타입(`UIStringParam`) 으로 캐스팅할 때,
  Model 이 다른 타입의 `UIParam` 을 반환하면 런타임 오류가 납니다.
- 항상 **한 Form 에서 기대하는 UIParam 타입과 Model 이 반환하는 타입을 맞추어야** 합니다.

### 7.4 Presenter 에 과도한 로직 넣기

- 이 시스템에서 Presenter 는 **조립과 UI 라이프사이클 담당**에 집중시키는 것이 좋습니다.
- 실제 게임 도메인 로직은 Model 이나 별도 도메인 서비스/시스템에 두고,
  Presenter 는 그 사이를 연결하는 쪽에 가깝게 유지합니다.

---

## 8. 이 시스템을 기준으로 기능을 설계할 때

- **질문 1: 이 행동은 어디에 있어야 하는가?**
  - UI 위젯의 입력 → Form (`IInteractable`)
  - UI 표시용 데이터 계산 → Model (`UIParam` 반환 메서드)
  - 여러 시스템을 엮는 오케스트레이션 → Presenter 또는 상위 도메인 서비스

- **질문 2: 이 상태는 언제 갱신되는가?**
  - 이벤트 기반이라면: 이벤트를 받은 곳에서 Model 상태를 바꾸고, 필요 시 `View.UpdateView()` 호출
  - 폴링/주기적 갱신이라면: Presenter 나 상위 시스템에서 적절한 타이밍에 `UpdateView()` 호출

- **질문 3: 외부에서 이 UI 를 어떻게 여는가?**
  - `UIId` + `EventChannel` 을 통해 `OpenUIEvent/CloseUIEvent/ToggleUIEvent` 를 날리는지 확인합니다.

이 문서를 기준으로, 새 UI 기능을 설계할 때는 항상:

1. **어떤 Model 상태/메서드가 필요한지**
2. **어떤 Form 이 어떤 UIParam 을 주고받을지**
3. **Presenter 가 언제 Open/Close/Update 를 호출할지**
4. **외부에서 이 UI 를 여닫는 경로(UIManager/이벤트)가 무엇인지**

를 먼저 정리한 뒤 구현을 시작하면, 시스템과 자연스럽게 맞게 됩니다.

