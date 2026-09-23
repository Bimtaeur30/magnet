# Phase 1 Sequence

## 1 — 2026-09-23 · 설치 성공 시 가벼운 탭

- 공용 VibrationSettings에 모바일별 VibratePlacement() 추가. IsEnabled를 공유해 진동 설정을 따른다.
- JTH GameFeedbackBootstrap의 설치 진동 호출을 짧은 탭으로 변경하고 DefaultGameFeedbackConfig.PlacementHaptics 활성화.
- KTJ Plugins/iOS/MagnetPlacementHaptics.mm 추가 및 iOS 전용 PluginImporter 확인.
- Unity 및 Android/iOS 조건부 C# 컴파일 통과. 실제 기기 진동은 확인 대기.
