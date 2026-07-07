using LitMotion;
using HwanLib.MVP.Forms.Module.Gauge;
using HwanLib.MVP.System.AbstractMVP.Form;
using HwanLib.MVP.System.BaseMVP;
using HwanLib.MVP.UIData;
using UnityEngine;

namespace HwanLib.MVP.Forms
{
    public class GaugeForm : AbstractVisualForm, IInitializable
    {
        [SerializeField] private float duration = 0.2f;
        [SerializeField] private GaugeType gaugeType;
        
        private AbstractGaugeModule _gaugeModule;
        
        public void Initialize()
        {
            InitGaugeForm(gaugeType);
        }

        public void InitGaugeForm(GaugeType type)
        {
            this.gaugeType = type;
            
            switch (this.gaugeType)
            {
                case GaugeType.PosY:
                    _gaugeModule = new PosYGaugeModule();
                    break;
                case GaugeType.Filled:
                    _gaugeModule = new FilledGaugeModule();
                    break;
            }
            
            _gaugeModule.InitGauge(gameObject);
        }

        protected override void UpdateVisual(UIParam data)
        {
            float ratio = ((UIFloatParam)data).Value;
            
            _gaugeModule.SetGauge(ratio, duration, Ease.OutQuart);
        }

        private void OnDestroy()
        {
            _gaugeModule.OnDestroy();
        }

    }
}
