using GameLib.EventChannelSystem;
using Reflex.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace JTH.Scripts.Bootstrap
{
    public sealed class MagnetProjectInstaller : MonoBehaviour, IInstaller
    {
        [FormerlySerializedAs("_mainEventChannel")]
        [SerializeField] private EventChannelSO mainEventChannel;

        public void InstallBindings(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterValue(mainEventChannel);
        }
    }
}
