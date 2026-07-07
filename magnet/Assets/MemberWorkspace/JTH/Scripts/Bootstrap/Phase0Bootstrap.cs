using GameLib.EventChannelSystem;
using JTH.Scripts.Events;
using Reflex.Attributes;
using UnityEngine;

namespace JTH.Scripts.Bootstrap
{
    public sealed class Phase0Bootstrap : MonoBehaviour
    {
        [Inject] private readonly EventChannelSO _eventChannel;

        private void OnEnable()
        {
            _eventChannel.AddListener<Phase0ReadyEvent>(OnPhase0Ready);
        }

        private void OnDisable()
        {
            _eventChannel.RemoveListener<Phase0ReadyEvent>(OnPhase0Ready);
        }

        private void Start()
        {
            Debug.Log("[Phase0] Bootstrap started — raising Phase0ReadyEvent");
            _eventChannel.RaiseEvent(GameEvents.Phase0ReadyEvent);
        }

        private void OnPhase0Ready(Phase0ReadyEvent _)
        {
            Debug.Log("[Phase0] EventChannel + Reflex DI verified OK");
        }
    }
}
