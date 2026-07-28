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
            Debug.Assert(gameBoard != null, "[gameBoard] BoardPlacementBootstrap is not assigned.", this);

            containerBuilder.RegisterValue(blockSpawnBootstrap);
            containerBuilder.RegisterValue(boardPlacementBootstrap);
            containerBuilder.RegisterValue(gameBoard);
        }
    }
}
