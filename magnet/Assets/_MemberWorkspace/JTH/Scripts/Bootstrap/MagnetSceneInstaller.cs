using JTH.Scripts.Presentation;
using Reflex.Core;
using UnityEngine;

namespace JTH.Scripts.Bootstrap
{
    public sealed class MagnetSceneInstaller : MonoBehaviour, IInstaller
    {
        [SerializeField] private BlockSpawnBootstrap blockSpawnBootstrap;
        [SerializeField] private BoardPlacementBootstrap boardPlacementBootstrap;
        [SerializeField] private GameBoard gameBoard;

        public void InstallBindings(ContainerBuilder containerBuilder)
        {
            Debug.Assert(blockSpawnBootstrap != null, "[MagnetSceneInstaller] BlockSpawnBootstrap is not assigned.", this);
            Debug.Assert(boardPlacementBootstrap != null, "[MagnetSceneInstaller] BoardPlacementBootstrap is not assigned.", this);
            Debug.Assert(gameBoard != null, "[MagnetSceneInstaller] GameBoard is not assigned.", this);

            if (blockSpawnBootstrap == null || boardPlacementBootstrap == null || gameBoard == null)
            {
                Debug.LogError(
                    "[MagnetSceneInstaller] Skipping InstallBindings because a required reference is null.",
                    this);
                return;
            }

            containerBuilder.RegisterValue(blockSpawnBootstrap);
            containerBuilder.RegisterValue(boardPlacementBootstrap);
            containerBuilder.RegisterValue(gameBoard);
        }
    }
}
