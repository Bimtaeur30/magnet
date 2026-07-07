using Reflex.Core;
using UnityEngine;

namespace JTH.Scripts.Bootstrap
{
    /// <summary>
    /// JTH 루트 Reflex Installer. EventChannelSO는 [SerializeField] 직렬화 — 여기서 등록하지 않는다.
    /// </summary>
    public sealed class MagnetProjectInstaller : MonoBehaviour, IInstaller
    {
        public void InstallBindings(ContainerBuilder containerBuilder)
        {
        }
    }
}
