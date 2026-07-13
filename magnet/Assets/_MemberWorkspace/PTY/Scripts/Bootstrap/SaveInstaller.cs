using GameLib.EventChannelSystem;
using PTY.Scripts.Save;
using PTY.Scripts.Save.Local;
using Reflex.Core;
using UnityEngine;

namespace PTY.Scripts.Bootstrap
{
    /// <summary>
    /// SCRUM-28 저장 구조. 클라우드 로그인 미구현 상태라 로컬 저장만 SaveService에 배선한다.
    /// (로그인 구현 시 ICloudSaveProvider 플랫폼 분기를 다시 추가하고 SaveService에 주입할 자리)
    /// </summary>
    public sealed class SaveInstaller : MonoBehaviour, IInstaller
    {
        [SerializeField] private EventChannelSO magnetGameChannel;

        public void InstallBindings(ContainerBuilder containerBuilder)
        {
            Debug.Assert(magnetGameChannel != null, "[SaveInstaller] EventChannelSO is not assigned.", this);

            ILocalSaveRepository localRepository = new JsonFileSaveRepository();
            ISaveService saveService = new SaveService(localRepository, magnetGameChannel);

            containerBuilder.RegisterValue(saveService, new[] { typeof(ISaveService) });
        }
    }
}
