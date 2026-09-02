using System;
using System.Collections.Generic;
using GameLib.SoundSystem;
using UnityEngine;

namespace JTH.Scripts.Data
{
    /// <summary>
    /// 콤보 상승·대량 라인클리어 피드백(사운드/진동) 설정. 코드 수정 없이 이 에셋만 채우면 된다.
    /// 배치음·클리어음(SkinDataSO)과는 별개로 위에 얹는 추가 연출 레이어.
    /// </summary>
    [CreateAssetMenu(fileName = "GameFeedbackConfig", menuName = "Magnet/Game Feedback Config")]
    public sealed class GameFeedbackConfigSO : ScriptableObject
    {
        [Serializable]
        public struct ComboTierEntry
        {
            [Tooltip("이 콤보 수 이상부터 이 티어 사운드로 전환. ComboTiers 리스트는 오름차순으로 등록")]
            public int ComboThreshold;

            [Tooltip("이 티어 진입 순간 재생할 사운드. 비우면 이 티어는 사운드 없음")]
            public SoundClipSO Sound;
        }

        [Tooltip("콤보 티어 목록 (오름차순). 예: 2콤보부터 A, 5콤보부터 B로 점점 격해지는 사운드")]
        [field: SerializeField] public List<ComboTierEntry> ComboTiers { get; private set; } = new();

        [Tooltip("콤보 티어가 새로 올라갈 때 진동. 기본 off — 이 에셋을 비워두면 기능 추가 전과 동일하게 동작")]
        [field: SerializeField] public bool ComboTierHaptics { get; private set; } = false;

        [Tooltip("한 번에 이 줄 수 이상 클리어되면 '대량 파괴'로 취급해 아래 사운드/진동 트리거")]
        [field: SerializeField, Min(2)] public int BigClearLineThreshold { get; private set; } = 3;

        [Tooltip("대량 파괴 시 재생할 사운드(라인클리어 스킨 사운드에 얹어서 재생). 비우면 재생 안 함")]
        [field: SerializeField] public SoundClipSO BigClearSound { get; private set; }

        [Tooltip("대량 파괴 시 진동. 기본 off")]
        [field: SerializeField] public bool BigClearHaptics { get; private set; } = false;

        [Tooltip("블록을 놓을 때마다(클리어 여부 무관) 가벼운 진동")]
        [field: SerializeField] public bool PlacementHaptics { get; private set; } = false;

        [Tooltip("줄이 클리어될 때마다(대량 여부 무관) 진동. 기본 off")]
        [field: SerializeField] public bool LineClearHaptics { get; private set; } = false;
    }
}
