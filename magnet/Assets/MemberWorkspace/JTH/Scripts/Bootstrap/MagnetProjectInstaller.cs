using GameLib.EventChannelSystem;
using Reflex.Core;
using UnityEngine;

namespace JTH.Scripts.Bootstrap
{
    public sealed class MagnetProjectInstaller : MonoBehaviour, IInstaller
    {
        [SerializeField] private EventChannelSO _mainEventChannel;

        public void InstallBindings(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterValue(_mainEventChannel);
        }
    }
}
