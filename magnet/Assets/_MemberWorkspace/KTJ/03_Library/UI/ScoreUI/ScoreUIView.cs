using Mvvm;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.UI
{
    public sealed partial class ScoreUIView : MvvmView<ScoreUIViewModel>
    {
        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame)
            {
                Test();
            }
        }

        public void Test()
        {
            ViewModel.SetCurrentScore(Random.Range(0, 100));
        }
    }
}