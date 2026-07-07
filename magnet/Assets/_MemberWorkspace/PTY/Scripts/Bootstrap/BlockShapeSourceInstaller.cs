using System;
using Magnet.Contracts.BlockShapes;
using PTY.Scripts.Data;
using Reflex.Core;
using UnityEngine;

namespace PTY.Scripts.Bootstrap
{
    /// <summary>
    /// BlockShapeSourceSO를 IBlockShapeSource로 Reflex에 등록한다.
    /// SO 직렬화는 이 Installer에만 두고, JTH 소비자는 [Inject] IBlockShapeSource를 쓴다.
    /// </summary>
    public sealed class BlockShapeSourceInstaller : MonoBehaviour, IInstaller
    {
        [SerializeField] private BlockShapeSourceSO blockShapeSource;

        public void InstallBindings(ContainerBuilder containerBuilder)
        {
            Debug.Assert(blockShapeSource != null, "[BlockShapeSourceInstaller] BlockShapeSourceSO is not assigned.", this);
            containerBuilder.RegisterValue(blockShapeSource, new[] { typeof(IBlockShapeSource) });
        }
    }
}
