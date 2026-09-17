# Phase 1 변경 기록

## 2 — 2026-09-05 · 여러 토글 버튼 등록 지원
- 사용자 정정 요청: 직렬화로 여러 토글 버튼을 추가할 수 있도록 확장.
- `additionalButtons`: Inspector에서 추가 버튼을 등록하는 List<Button> 필드 추가. 기존 btn 연결 유지.
- `RegisterButton()`: HashSet으로 빈 항목 및 중복 등록 방지.
- `OnDisable()`: 실제 등록한 버튼에서 리스너를 해제하고 등록 목록 초기화.
- Tooltip 표 및 Phase 목표 갱신. Unity 컴파일 및 Play Mode 검증은 미실행.

## 1 — 2026-09-05 · 버튼 직렬화
- 사용자 계획 승인: `진행`.
- `ToggleActiveBtn_UI.btn`: SerializeField 및 한국어 Tooltip 추가.
- `Awake()`: 직렬화된 버튼이 없을 때만 GetComponent로 연결.
- `OnEnable()` / `OnDisable()`: 버튼 null 검사 후 클릭 리스너 등록·해제.
- KTJ Tooltip 표, Phase 및 구현 인덱스 갱신.
- Unity 컴파일 및 Play Mode 확인은 미실행.
- `git diff --check` 통과.
- `dotnet build Magent.KTJ.csproj --no-restore --verbosity quiet` 시도: 로컬 Microsoft SDKs 경로 접근 거부(MSB4184)로 빌드 실행이 차단됨. C# 컴파일 성공 여부는 미확인.
