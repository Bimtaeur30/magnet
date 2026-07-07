using HwanLib.MVP.System;
using HwanLib.MVP.System.AbstractMVP.Form;
using HwanLib.MVP.System.BaseMVP;
using HwanLib.MVP.System.BaseMVP.Form;
using HwanLib.MVP.UIData;
using UnityEngine.UI;

namespace HwanLib.MVP.Forms
{
    public class ToggleForm : AbstractVisualForm, IInteractable, IInitializable
    {
        public event FormInteracted OnFormInteracted;

        private Toggle _toggle;

        public void Initialize()
        {
            _toggle = GetComponent<Toggle>();
            _toggle.onValueChanged.AddListener(SliderValueChangeHandler);
        }

        private void SliderValueChangeHandler(bool value)
        {
            OnFormInteracted?.Invoke(UIParams.UIBoolParam.Init(value));
        }

        protected override void UpdateVisual(UIParam data)
        {
            _toggle.isOn = ((UIBoolParam)data).Value;
        }
    }
}