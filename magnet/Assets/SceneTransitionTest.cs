using _Shared.Magnet.Core.SceneTransition;
using GameLib.EventChannelSystem;
using UnityEngine;
using UnityEngine.InputSystem;

public class SceneTransitionTest : MonoBehaviour
{
    [SerializeField] private EventChannelSO magnetGameChannel;
    
    private void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            magnetGameChannel.RaiseEvent(
                new LoadSceneEvent().Init("02_Main"));
        }
    }
}