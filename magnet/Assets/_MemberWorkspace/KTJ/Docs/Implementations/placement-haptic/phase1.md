# Phase 1 — 블록 설치 탭 진동

- 승인: 2026-09-23 사용자 진행 승인.
- 목표: 성공한 블록 설치마다 짧은 단발 진동. 취소·배치 실패는 제외. 기존 진동 on/off 토글 적용.
- 설치 성공 후 발행되는 BlockPlacedEvent의 기존 GameFeedbackBootstrap 구독 사용. PlacementHaptics가 켜진 경우 VibrationSettings.VibratePlacement() 호출.
- Android API 29 이상은 VibrationEffect.EFFECT_TICK, API 26~28은 20ms createOneShot, 그 이전은 20ms Vibrator.vibrate 사용.
- iOS는 KTJ Plugins/iOS의 UIImpactFeedbackGenerator(UIImpactFeedbackStyleLight) 네이티브 플러그인 사용.
- DefaultGameFeedbackConfig의 PlacementHaptics 활성화. 기존 라인 클리어·콤보 진동 설정은 유지.
- 검증: Unity Editor 컴파일 오류 없음. Bootstraps.prefab의 GameFeedbackBootstrap이 변경된 설정 에셋과 인게임 채널을 참조함을 확인. Android/iOS 조건부 C# 임시 빌드 각각 경고·오류 0. Unity PluginImporter에서 iOS 플러그인 iOS 전용 설정 확인. 실기기 촉감 및 iOS Xcode 빌드는 미검증.
