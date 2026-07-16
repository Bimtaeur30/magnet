using JTH.Scripts.Presentation;
using Reflex.Core;
using UnityEngine;

namespace JTH.Scripts.Bootstrap
{
    /// <summary>
    /// 씬 MonoBehaviour 참조를 Installer에서 한 번 배선하고 Reflex에 등록한다.
    /// 소비자는 [Inject]로 받는다 (Inspector 드래그 배선 금지).
    /// </summary>
    public sealed class MagnetSceneInstaller : MonoBehaviour, IInstaller
    {
        [SerializeField] private BlockSpawnBootstrap blockSpawnBootstrap;
        [SerializeField] private BoardPlacementBootstrap boardPlacementBootstrap;
        [SerializeField] private PlacedBlocksView placedBlocksView;
        [SerializeField] private BoardView boardView;

        public void InstallBindings(ContainerBuilder containerBuilder)
        {
            Debug.Assert(blockSpawnBootstrap != null, "[MagnetSceneInstaller] BlockSpawnBootstrap is not assigned.", this);
            Debug.Assert(boardPlacementBootstrap != null, "[MagnetSceneInstaller] BoardPlacementBootstrap is not assigned.", this);
            Debug.Assert(placedBlocksView != null, "[MagnetSceneInstaller] PlacedBlocksView is not assigned.", this);
            Debug.Assert(boardView != null, "[MagnetSceneInstaller] BoardView is not assigned.", this);

            containerBuilder.RegisterValue(blockSpawnBootstrap);
            containerBuilder.RegisterValue(boardPlacementBootstrap);
            containerBuilder.RegisterValue(placedBlocksView);
            containerBuilder.RegisterValue(boardView);
        }
    }
}
