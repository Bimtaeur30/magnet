using _Shared.Magnet.Core.SceneTransition;
using UnityEngine;
using UnityEngine.InputSystem;

public class SceneTransitionTest : MonoBehaviour
{
    [SerializeField] private SceneLoadManager sceneLoadManager;
    [SerializeField] private SceneDefSO targetScene;

    private void Update()
    {
        //if (Keyboard.current.spaceKey.wasPressedThisFrame)
        //{
        //    sceneLoadManager.LoadScene(targetScene);
        //}
    }
}
