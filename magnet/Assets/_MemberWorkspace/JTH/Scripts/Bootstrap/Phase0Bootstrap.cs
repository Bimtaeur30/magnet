using GameLib.EventChannelSystem;
using JTH.Scripts.Events;
using UnityEngine;
using UnityEngine.Serialization;

namespace JTH.Scripts.Bootstrap
{
    public sealed class Phase0Bootstrap : MonoBehaviour
    {
        [FormerlySerializedAs("mainEventChannelSO")]
        [FormerlySerializedAs("_eventChannel")]
        [SerializeField] private EventChannelSO magnetGameChannel;

        private void OnEnable()
        {
            magnetGameChannel.AddListener<Phase0ReadyEvent>(OnPhase0Ready);
        }

        private void OnDisable()
        {
            magnetGameChannel.RemoveListener<Phase0ReadyEvent>(OnPhase0Ready);
        }

        private void Start()
        {
            Debug.Assert(magnetGameChannel != null, "[Phase0Bootstrap] magnetGameChannel is not assigned.", this);
            Debug.Log("[Phase0] Bootstrap started — raising Phase0ReadyEvent");
            magnetGameChannel.RaiseEvent(MagnetGameEvents.Phase0ReadyEvent);
        }

        private void OnPhase0Ready(Phase0ReadyEvent _)
        {
            Debug.Log("[Phase0] EventChannel verified OK");
        }
    }
}
