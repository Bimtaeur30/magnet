using GameLib.EventChannelSystem;
using Magnet.Core.SceneTransition;
using UnityEngine;

public class SceneMove : MonoBehaviour
{
    [Header("SceneMove")]
    [SerializeField] private EventChannelSO SceneTransitionChannel;
    [SerializeField] private SceneDefSO TargetScene;

    public void Move()
    {
        SceneTransitionChannel.RaiseEvent(SceneTransitionEvents.LoadSceneEvent.Init(TargetScene));
    }
}
