using GameLib.EventChannelSystem;
using Mvvm;
using UnityEngine;

namespace Game.UI
{
    public sealed partial class ScoreUIView : MvvmView<ScoreUIViewModel>
    {
        [SerializeField] private EventChannelSO InGameChannel;
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Q))
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
