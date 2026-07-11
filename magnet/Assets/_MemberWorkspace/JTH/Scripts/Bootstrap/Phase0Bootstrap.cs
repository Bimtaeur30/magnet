using GameLib.EventChannelSystem;
using JTH.Scripts.Events;
using UnityEngine;

namespace JTH.Scripts.Bootstrap
{
    public sealed class Phase0Bootstrap : MonoBehaviour
    {
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
            magnetGameChannel.RaiseEvent(MagnetGameEvents.Phase0ReadyEvent);
        }

        private void OnPhase0Ready(Phase0ReadyEvent _)
        {
        }
    }
}
