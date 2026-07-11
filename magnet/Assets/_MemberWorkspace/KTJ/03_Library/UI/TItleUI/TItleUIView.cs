using Mvvm;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public sealed partial class TItleUIView : MvvmView<TItleUIViewModel>
    {
        [SerializeField] private Button startButton;
        [SerializeField] private float fadeDuration = 1f;

        protected override void OnEnable()
        {
            base.OnEnable();
            ViewModel.PlayFadeIn(fadeDuration);
            startButton.onClick.AddListener(() => ViewModel.PlayButtonScaleAnim(1.1f));
        }

        protected override void OnDisable()
        {
            ViewModel.StopFade();
            base.OnDisable();
        }
    }
}
