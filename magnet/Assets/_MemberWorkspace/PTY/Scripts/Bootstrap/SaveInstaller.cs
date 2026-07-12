using GameLib.EventChannelSystem;
using PTY.Scripts.Save;
using PTY.Scripts.Save.Cloud;
using PTY.Scripts.Save.Local;
using Reflex.Core;
using UnityEngine;

namespace PTY.Scripts.Bootstrap
{
    /// <summary>
    /// SCRUM-28 저장 구조 뼈대. 플랫폼별 ICloudSaveProvider를 골라 SaveService를 ISaveService로 등록한다.
    /// 실제 저장 로직은 JsonFileSaveRepository / GooglePlayGamesSaveProvider / AppleGameKitSaveProvider에서 채운다.
    /// </summary>
    public sealed class SaveInstaller : MonoBehaviour, IInstaller
    {
        [SerializeField] private EventChannelSO magnetGameChannel;

        public void InstallBindings(ContainerBuilder containerBuilder)
        {
            Debug.Assert(magnetGameChannel != null, "[SaveInstaller] EventChannelSO is not assigned.", this);

            ILocalSaveRepository localRepository = new JsonFileSaveRepository();
            ICloudSaveProvider cloudProvider = CreateCloudProvider();
            ISaveService saveService = new SaveService(localRepository, cloudProvider, magnetGameChannel);

            containerBuilder.RegisterValue(saveService, new[] { typeof(ISaveService) });
        }

        private static ICloudSaveProvider CreateCloudProvider()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return new GooglePlayGamesSaveProvider();
#elif UNITY_IOS && !UNITY_EDITOR
            return new AppleGameKitSaveProvider();
#else
            return new NullCloudSaveProvider();
#endif
        }
    }
}
