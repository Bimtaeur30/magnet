using HwanLib.MVP.System;
using HwanLib.MVP.System.AbstractMVP.Form;
using HwanLib.MVP.System.BaseMVP;
using HwanLib.MVP.System.BaseMVP.Form;
using HwanLib.MVP.UIData;
using UnityEngine.UI;

namespace HwanLib.MVP.Forms
{
    public class SliderForm : AbstractVisualForm, IInteractable, IInitializable
    {
        public event FormInteracted OnFormInteracted;

        private Slider _slider;

        public void Initialize()
        {
            _slider = GetComponent<Slider>();
            _slider.onValueChanged.AddListener(SliderValueChangeHandler);
        }

        private void SliderValueChangeHandler(float value)
        {
            OnFormInteracted?.Invoke(UIParams.UIFloatParam.Init(value));
        }

        protected override void UpdateVisual(UIParam data)
        {
            _slider.value = ((UIFloatParam)data).Value;
        }
    }
}