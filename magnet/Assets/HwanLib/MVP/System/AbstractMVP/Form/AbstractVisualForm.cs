using System;
using HwanLib.MVP.System.BaseMVP.Form;
using HwanLib.MVP.UIData;

namespace HwanLib.MVP.System.AbstractMVP.Form
{
    public abstract class AbstractVisualForm : BaseForm, IUpdatable
    {
        private Func<UIParam> _source;

        public void BindUpdateSource(Func<UIParam> source) => _source = source;

        public void UpdateForm() => UpdateVisual(_source?.Invoke());

        protected abstract void UpdateVisual(UIParam data);
    }
}
