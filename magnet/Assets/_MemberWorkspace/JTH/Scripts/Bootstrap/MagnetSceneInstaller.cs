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
        [Tooltip("슬롯 선택 시 스테이징 영역에 미리보기로 표시할 BlockPieceView")]
        [SerializeField] private BlockPieceView stagingBlockView;

        public void InstallBindings(ContainerBuilder containerBuilder)
        {
            Debug.Assert(blockSpawnBootstrap != null, "[MagnetSceneInstaller] BlockSpawnBootstrap is not assigned.", this);
            Debug.Assert(stagingBlockView != null, "[MagnetSceneInstaller] BlockPieceView is not assigned.", this);

            containerBuilder.RegisterValue(blockSpawnBootstrap);
            containerBuilder.RegisterValue(stagingBlockView);
        }
    }
}
